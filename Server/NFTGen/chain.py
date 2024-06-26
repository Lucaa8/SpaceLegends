import json
import os

from eth_account import Account
from eth_account.signers.local import LocalAccount
from web3 import Web3
from web3.exceptions import ContractLogicError
from web3.middleware import geth_poa_middleware
from threading import Thread
import time


class CosmicRelic:
    def __init__(self, alchemy_key: str, contract_address: str, contract_abi_path: str):
        self._alchemy = f'https://eth-sepolia.g.alchemy.com/v2/{alchemy_key}'
        self.w3 = Web3(Web3.HTTPProvider(self._alchemy))
        # self.w3.middleware_onion.inject(geth_poa_middleware, layer=0) # To allow event listening
        assert self.w3.is_connected()
        self.account = Account.from_key(os.getenv("PRIVATE_KEY"))
        cs_contract_address = self.w3.to_checksum_address(contract_address)
        with open(contract_abi_path, 'r', encoding='utf-8') as f:
            contract_abi = json.load(f)
        assert contract_abi is not None
        self.crel = self.w3.eth.contract(address=cs_contract_address, abi=contract_abi)
        # Thread(target=self._listen_for_mint_events).start() # Working (It can take sometime to display the transaction

    def _listen_for_mint_events(self):
        event_filter = self.crel.events.Transfer.create_filter(fromBlock='latest')
        while True:
            try:
                for event in event_filter.get_new_entries():
                    print(f"New mint event: {event}")
                    # Ajoutez votre logique ici pour gérer l'événement
            except Exception as e:
                print(f"Error: {e}")
            time.sleep(5)

    def check_address(self, address: str) -> bool:
        return self.w3.is_address(address) and self.w3.is_checksum_address(address)

    def onchain(self, from_acc: LocalAccount, func_result: any, on_receive: callable(any)):
        current_gas_price = self.w3.eth.gas_price

        func_txn = func_result.build_transaction({
            'from': from_acc.address,
            'nonce': self.w3.eth.get_transaction_count(from_acc.address),
            'gas': '0',
            'gasPrice': current_gas_price
        })

        gas = self.w3.eth.estimate_gas(func_txn)
        func_txn.update({'gas': gas})

        price_eth = gas * current_gas_price

        if price_eth <= 0.001:
            signed_txn = self.w3.eth.account.sign_transaction(func_txn, private_key=from_acc.key)
            tx_hash = self.w3.eth.send_raw_transaction(signed_txn.rawTransaction)
            print(f'Transaction hash: {self.w3.to_hex(tx_hash)}')

            receipt = self.w3.eth.wait_for_transaction_receipt(tx_hash)
            on_receive(receipt)

    def mint_nft(self, addr_to: str, uid_to: str, token_id: int, token_type: int, on_mint: callable(any)):
        addr_to = self.w3.to_checksum_address(addr_to)
        contract_func = self.crel.functions.mint(addr_to, uid_to, token_id, token_type)
        self.onchain(self.account, contract_func, lambda receipt: on_mint(receipt))

    def get_balance_eth(self, address: str) -> float:
        if not self.check_address(address):
            return -1
        wei = self.w3.eth.get_balance(self.w3.to_checksum_address(address))
        return self.w3.from_wei(wei, 'ether')

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
