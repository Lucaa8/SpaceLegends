import json
import os
import schedule
from threading import Thread
import time
from utils import decrypt_wallet_key, encrypt_wallet_key

from eth_account import Account
from eth_account.signers.local import LocalAccount
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
        schedule.every(5).seconds.do(self.check_gas_price)

        def check_gas_thread():
            while True:
                schedule.run_pending()
                time.sleep(5)
        Thread(target=check_gas_thread).start()

    def check_gas_price(self):
        from models.ChainTx import ChainTx
        current_gas_price = self.w3.eth.gas_price + self.w3.to_wei(3, 'gwei')
        for tx in ChainTx.get_all_unsent():
            self.send_tx(tx, current_gas_price)
            pass

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

        if price_eth < 0.005:
            signed_txn = self.w3.eth.account.sign_transaction(func_txn, private_key=self.account.key) #decrypt_wallet_key(tx.from_pkey)
            tx.sent()
            tx_hash = self.w3.eth.send_raw_transaction(signed_txn.rawTransaction)
            print(f'Transaction hash: {self.w3.to_hex(tx_hash)}')
            receipt = self.w3.eth.wait_for_transaction_receipt(tx_hash)
            # c le receipt ici dessous. Sinon chercher pk sent_at et completed_at ne sont pas mis à jour. Ensuite, actualiser gas et gas_price dans la db aussi.
            # finalement, voir si c'est possible de lancer les tx en parralèle (si le fait de lancer transaction nonce x, autorise transaction x+1 a s'executer meme si pas encore confirmée
            # Sinon simplement mettre bool qui dit "si boucle d'il y a 5min pas terminée, ne pas relancer", si ca va devenir le bordel
            # AttributeDict({'blockHash': HexBytes('0xa2d61f7c12e068efdbd4163b18a584c6ddffd059698ca6c1113ea34895b26308'), 'blockNumber': 6193413, 'contractAddress': None, 'cumulativeGasUsed': 1049429, 'effectiveGasPrice': 3492800329, 'from': '0xF3c5a73fD7E7721C3Bf500b65D4656C4e66A2C98', 'gasUsed': 172722, 'logs': [AttributeDict({'address': '0x4cBCBb04878F75CbD0f0CD9e10613480f2b48Aec', 'topics': [HexBytes('0xddf252ad1be2c89b69c2b068fc378daa952ba7f163c4a11628f55a4df523b3ef'), HexBytes('0x0000000000000000000000000000000000000000000000000000000000000000'), HexBytes('0x000000000000000000000000f3c5a73fd7e7721c3bf500b65d4656c4e66a2c98'), HexBytes('0x0000000000000000000000000000000000000000000000000000000000000000')], 'data': HexBytes('0x'), 'blockNumber': 6193413, 'transactionHash': HexBytes('0xa59039bd44cc0ec16257c74d18f2268224f0f85ae95da7a2df786316049bf539'), 'transactionIndex': 9, 'blockHash': HexBytes('0xa2d61f7c12e068efdbd4163b18a584c6ddffd059698ca6c1113ea34895b26308'), 'logIndex': 16, 'removed': False})], 'logsBloom': HexBytes('0x00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000200000000000000008000000000002000000000000000000000000000000000000020000000000000000000800000000000000000000000010000000000000000001000000000000000000000000000000000002000000000000000000000000000000000000000040000000000000000000000000000000000000000000000002000000000000000000000000000000000000000000000000008020000000000000000000000000000000000000000000000000000000000000000000'), 'status': 1, 'to': '0x4cBCBb04878F75CbD0f0CD9e10613480f2b48Aec', 'transactionHash': HexBytes('0xa59039bd44cc0ec16257c74d18f2268224f0f85ae95da7a2df786316049bf539'), 'transactionIndex': 9, 'type': 0})
            tx.completed(receipt)

    def check_address(self, address: str) -> bool:
        return self.w3.is_address(address) and self.w3.is_checksum_address(address)

    def mint_nft(self, addr_to: str, uid_to: str, token_id: int, token_type: int):
        from models.ChainTx import ChainTx
        addr_to = self.w3.to_checksum_address(addr_to)
        ChainTx.add_tx(self.account.address, encrypt_wallet_key(self.account.key.hex()), 'mint', (addr_to, uid_to, token_id, token_type))

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
