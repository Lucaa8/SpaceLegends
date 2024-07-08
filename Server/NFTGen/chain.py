import json
import os
from datetime import datetime

import schedule
from threading import Thread
import time

from utils import decrypt_wallet_key, encrypt_wallet_key

from eth_account import Account
from web3 import Web3
from web3.exceptions import ContractLogicError


class CosmicRelic:
    def __init__(self, alchemy_key: str, nft_abi_path: str, sdt_abi_path: str):
        self._alchemy = f'https://eth-sepolia.g.alchemy.com/v2/{alchemy_key}'
        self.w3 = Web3(Web3.HTTPProvider(self._alchemy))
        assert self.w3.is_connected()
        self.account = Account.from_key(os.getenv("PRIVATE_KEY"))
        with open(nft_abi_path, 'r', encoding='utf-8') as f:
            nft_contract_abi = json.load(f)
        assert nft_contract_abi is not None
        with open(sdt_abi_path, 'r', encoding='utf-8') as f:
            sdt_contract_abi = json.load(f)
        assert sdt_contract_abi is not None
        self.crel = self.w3.eth.contract(address=self.w3.to_checksum_address(os.getenv("NFT_ADDRESS")), abi=nft_contract_abi)
        self.sdt = self.w3.eth.contract(address=self.w3.to_checksum_address(os.getenv("SDT_ADDRESS")), abi=sdt_contract_abi)
        self.max_fee_crl: float = float(os.getenv("MAX_ETH_GAS_FEE_CRL")) # e.g. 0.005, so nft mint action ~180'000 gas is limited to 29 gwei
        self.max_fee_sdt: float = float(os.getenv("MAX_ETH_GAS_FEE_SDT")) # e.g. 0.0011, so sdt mint action ~40'000 gas is limited to 29 gwei too.
        self.working = False
        schedule.every(5).minutes.do(self.check_gas_price)

        def check_gas_thread():
            while True:
                if not self.working:
                    try:
                        schedule.run_pending()
                    except Exception as e:
                        print(f"An error occurred in the chain#check_gas_price method. The {datetime.now()} check has been skipped. Error: {str(e)}")
                        self.working = False
                time.sleep(5)
        Thread(target=check_gas_thread).start()

    def check_gas_price(self):
        self.working = True
        from models.ChainTx import ChainTx
        txs = ChainTx.get_all_unsent()
        print(f"[{datetime.now()}] There are {len(txs)} queued transactions. Current Gas price [Gwei]: {self.w3.from_wei(self.w3.eth.gas_price, 'gwei')}")
        for tx in txs:
            try:
                self.send_tx(tx[0])
            except Exception as e:
                print(f"Something went wrong while trying to send the transaction chain_tx.id=={tx[0].id} with the following data: {tx[0].tx}. Error: {str(e)}")
        self.working = False

    def send_tx(self, tx):

        # The priority fee is somewhere between 1-2 but to be REALLY sure the transaction does not get stuck I add 3 to the max gas price.
        gas_price = self.w3.eth.gas_price + self.w3.to_wei(3, 'gwei')

        func_txn = tx.prebuild_tx().build_transaction({
            'from': tx.from_address,
            'nonce': self.w3.eth.get_transaction_count(tx.from_address),
            'gas': '0',
            'gasPrice': gas_price
        })

        gas = self.w3.eth.estimate_gas(func_txn)
        func_txn.update({'gas': gas})

        price_eth = self.w3.from_wei(gas * gas_price, 'ether')
        max_fee = self.max_fee_crl if tx.tx_type == 'crel' else self.max_fee_sdt

        if price_eth <= max_fee:
            signed_txn = self.w3.eth.account.sign_transaction(func_txn, private_key=decrypt_wallet_key(tx.from_pkey))
            tx.sent()
            tx_hash = self.w3.eth.send_raw_transaction(signed_txn.rawTransaction)
            print(f"[{datetime.now()}] [{self.w3.from_wei(gas_price, 'gwei')} Gwei] Transaction chain_tx.id=={tx.id} has been sent to the blockchain with hash: {self.w3.to_hex(tx_hash)}. Waiting for confirmation...")
            receipt = self.w3.eth.wait_for_transaction_receipt(tx_hash)
            tx.completed(receipt, self)

    def mint_nft(self, addr_to: str, uid_to: str, token_id: int, token_type: int) -> None:
        from models.ChainTx import ChainTx
        addr_to = self.w3.to_checksum_address(addr_to)
        ChainTx.add_tx(self.account.address, encrypt_wallet_key(self.account.key.hex()), 'crel', 'mint', (addr_to, uid_to, token_id, token_type))

    def transfer_nft(self, user_from, user_to, token_id: int) -> None:
        from models.ChainTx import ChainTx
        addr_to = self.w3.to_checksum_address(user_to.wallet_address)
        ChainTx.add_tx(user_from.wallet_address, user_from.wallet_key, 'crel', 'safeTransfer', (token_id, addr_to, user_to.username))

    def burnFor_sdt(self, addr_from, amount_sdt_ether: float) -> None:
        from models.ChainTx import ChainTx
        addr_from = self.w3.to_checksum_address(addr_from)
        amount_to_wei = self.w3.to_wei(amount_sdt_ether, 'ether')
        ChainTx.add_tx(self.account.address, encrypt_wallet_key(self.account.key.hex()), 'sdt', 'burnFor', (addr_from, amount_to_wei))

    def mint_sdt(self, addr_to: str, amount_sdt_ether: float) -> None:
        from models.ChainTx import ChainTx
        addr_to = self.w3.to_checksum_address(addr_to)
        amount_to_wei = self.w3.to_wei(amount_sdt_ether, 'ether')
        ChainTx.add_tx(self.account.address, encrypt_wallet_key(self.account.key.hex()), 'sdt', 'mint', (addr_to, amount_to_wei))

    def transfer_sdt(self, user_from, user_to, amount_sdt_ether: float) -> None:
        from models.ChainTx import ChainTx
        addr_to = self.w3.to_checksum_address(user_to.wallet_address)
        amount_to_wei = self.w3.to_wei(amount_sdt_ether, 'ether')
        ChainTx.add_tx(user_from.wallet_address, user_from.wallet_key, 'sdt', 'transfer', (addr_to, amount_to_wei))

    @staticmethod
    def event_crel_mint(args: tuple) -> None:
        if args is None or len(args) < 4:
            print("CosmicRelic.event_mint rejected invalid arguments transaction")
            return
        token_id = args[2]
        try:
            from models.NFT import NFT
            NFT.on_mint(token_id)
            print(f"Transaction for token.id=={token_id} has been minted successfully.")
        except Exception as e:
            print(f"CosmicRelic.event_mint failed to mint token with id=={token_id}: {str(e)}")

    # A tester!!
    @staticmethod
    def event_crel_safeTransfer(args: tuple) -> None:
        if args is None or len(args) < 3:
            print("CosmicRelic.event_transfer rejected invalid arguments transaction")
            return
        token_id = args[0]
        try:
            from models.NFT import NFT
            NFT.on_buy(token_id)
            print(f"Transaction for token.id=={token_id} has been transferred successfully.")
        except Exception as e:
            print(f"CosmicRelic.event_transfer failed to transfer token with id=={token_id}: {str(e)}")

    @staticmethod
    def event_sdt_mint(args: tuple) -> None:
        print(f"Mint transaction of {cosmic.w3.from_wei(args[1], 'ether')} SDT for address={args[0]} has been completed successfully.")

    @staticmethod
    def event_sdt_burnFor(args: tuple) -> None:
        print(f"BurnFor transaction of {cosmic.w3.from_wei(args[1], 'ether')} SDT for address={args[0]} has been completed successfully.")

    @staticmethod
    def event_sdt_transfer(args: tuple) -> None:
        print(f"Transfer transaction of {cosmic.w3.from_wei(args[1], 'ether')} SDT to address={args[0]} has been completed successfully.")

    def check_address(self, address: str) -> bool:
        return self.w3.is_address(address) and self.w3.is_checksum_address(address)

    # This method SHOULD NOT being used to get the available eth for an account. This does not count any currently pending transaction which will cost up to self.max_fee (e.g. 0.005) eth.
    def _get_balance_eth(self, address: str) -> float:
        if not self.check_address(address):
            return -1
        wei = self.w3.eth.get_balance(self.w3.to_checksum_address(address))
        return float(self.w3.from_wei(wei, 'ether'))

    # Same as get_balance_eth but removes the currently reserved eth for gas fee of pending transactions/nft listings for that address (like transfer nft)
    def get_available_eth(self, user) -> float:
        eth = self._get_balance_eth(user.wallet_address)
        if eth < 0:
            return -1
        # Every current available nft listing on the market count as a potential transfer for this user so I remove max_fee_crl for each one
        from models.MarketListing import MarketListing
        count = MarketListing.find_valid_listings_count(user.id)
        available_eth = eth - (count * self.max_fee_crl)
        # Every current pending transaction (transaction is waiting for the gwei to be low enough to send it to the chain) needs to be counted as potential maximum send fee
        from models.ChainTx import ChainTx
        unsent = ChainTx.get_all_unsent(from_addr=user.wallet_address)
        for tx in unsent:
            available_eth -= self.max_fee_crl if tx.tx_type == 'crel' else self.max_fee_sdt
        return max(0.0, available_eth)

    def get_balance_sdt(self, address: str) -> float:
        if not self.check_address(address):
            return -1
        try:
            amount_wei = self.sdt.functions.__getitem__("balanceOf")(address).call()
            return self.w3.from_wei(amount_wei, 'ether')
        except ContractLogicError as logic:
            print(f"[chain.py get_balance_sdt] A Contract Logic error occurred while trying to call the function balanceOf with address {address}:")
            print(logic)

    def is_valid(self):
        return self.crel is not None

    def _call(self, func: str, token_id: int):
        try:
            return self.crel.functions.__getitem__(func)(token_id).call()
        except ContractLogicError as logic:
            print(f"[chain.py _call] A Contract Logic error occurred while trying to call the function {func} with token_id {token_id}:")
            print(logic)

    def get_token_type(self, token_id: int) -> int:
        return self._call("getTokenType", token_id)

    def get_ownership_history(self, token_id: int) -> list[tuple[str, str]]:
        return self._call("getOwnershipHistory", token_id)

    def get_token_creation_timestamp(self, token_id: int) -> int:
        return self._call("getTokenCreationTimestamp", token_id)


cosmic: CosmicRelic | None = None


def load():
    global cosmic
    cosmic = CosmicRelic(os.getenv("ALCHEMY_API_KEY"), 'contracts/crel.json', 'contracts/sdt.json')
