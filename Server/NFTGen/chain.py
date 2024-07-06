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
    def __init__(self, alchemy_key: str, contract_address: str, contract_abi_path: str):
        self._alchemy = f'https://eth-sepolia.g.alchemy.com/v2/{alchemy_key}'
        self.w3 = Web3(Web3.HTTPProvider(self._alchemy))
        assert self.w3.is_connected()
        self.account = Account.from_key(os.getenv("PRIVATE_KEY"))
        cs_contract_address = self.w3.to_checksum_address(contract_address)
        with open(contract_abi_path, 'r', encoding='utf-8') as f:
            contract_abi = json.load(f)
        assert contract_abi is not None
        self.crel = self.w3.eth.contract(address=cs_contract_address, abi=contract_abi)
        self.max_fee: float = float(os.getenv("MAX_ETH_GAS_FEE"))
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
        current_gas_price = self.w3.eth.gas_price + self.w3.to_wei(3, 'gwei')
        from models.ChainTx import ChainTx
        txs = ChainTx.get_all_unsent()
        print(f"[{datetime.now()}] There are {len(txs)} queued transactions. Current Gas price [Gwei]: {self.w3.from_wei(current_gas_price, 'gwei')}")
        for tx in txs:
            try:
                self.send_tx(tx[0], current_gas_price)
            except Exception as e:
                print(f"Something went wrong while trying to send the transaction chain_tx.id=={tx[0].id} with the following data: {tx[0].tx}. Error: {str(e)}")
        self.working = False

    def send_tx(self, tx, gas_price: int):

        func_txn = tx.prebuild_tx().build_transaction({
            'from': tx.from_address,
            'nonce': self.w3.eth.get_transaction_count(tx.from_address),
            'gas': '0',
            'gasPrice': gas_price
        })

        gas = self.w3.eth.estimate_gas(func_txn)
        func_txn.update({'gas': gas})

        price_eth = self.w3.from_wei(gas * gas_price, 'ether')

        if price_eth <= self.max_fee:
            signed_txn = self.w3.eth.account.sign_transaction(func_txn, private_key=decrypt_wallet_key(tx.from_pkey))
            tx.sent()
            tx_hash = self.w3.eth.send_raw_transaction(signed_txn.rawTransaction)
            print(f"Transaction chain_tx.id=={tx.id} has been sent to the blockchain with hash: {self.w3.to_hex(tx_hash)}. Waiting for confirmation...")
            receipt = self.w3.eth.wait_for_transaction_receipt(tx_hash)
            tx.completed(receipt, self)

    def mint_nft(self, addr_to: str, uid_to: str, token_id: int, token_type: int) -> None:
        from models.ChainTx import ChainTx
        addr_to = self.w3.to_checksum_address(addr_to)
        ChainTx.add_tx(self.account.address, encrypt_wallet_key(self.account.key.hex()), 'mint', (addr_to, uid_to, token_id, token_type))

    def transfer_nft(self, user_from, user_to, token_id: int) -> None:
        from models.ChainTx import ChainTx
        addr_to = self.w3.to_checksum_address(user_to.wallet_address)
        ChainTx.add_tx(user_from.wallet_address, user_from.wallet_key, 'safeTransfer', (token_id, addr_to, user_to.username))

    @staticmethod
    def event_mint(args: tuple) -> None:
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
    def event_safeTransfer(args: tuple) -> None:
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
        from models.MarketListing import MarketListing
        count = MarketListing.find_valid_listings_count(user.id)
        from models.ChainTx import ChainTx
        unsent = ChainTx.get_all_unsent(from_addr=user.wallet_address)
        count += len(unsent) if unsent is not None else 0
        reserved_eth = count * self.max_fee
        return max(0.0, eth - reserved_eth)

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
    cosmic = CosmicRelic(os.getenv("ALCHEMY_API_KEY"), os.getenv("NFT_ADDRESS"), 'contracts/crel.json')
