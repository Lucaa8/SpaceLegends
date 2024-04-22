import json
import os
from web3 import Web3
from web3.exceptions import ContractLogicError


class CosmicRelic:
    def __init__(self, alchemy_key: str, contract_address: str, contract_abi_path: str):
        self._alchemy = f'https://eth-sepolia.g.alchemy.com/v2/{alchemy_key}'
        self.w3 = Web3(Web3.HTTPProvider(self._alchemy))
        assert self.w3.is_connected()
        cs_contract_address = self.w3.to_checksum_address(contract_address)
        with open(contract_abi_path, 'r', encoding='utf-8') as f:
            contract_abi = json.load(f)
        assert contract_abi is not None
        self.crel = self.w3.eth.contract(address=cs_contract_address, abi=contract_abi)

    def check_address(self, address: str) -> bool:
        return self.w3.is_address(address) and self.w3.is_checksum_address(address)

    def is_valid(self):
        return self.crel is not None

    def _call(self, func: str, token_id: int):
        try:
            return self.crel.functions.__getitem__(func)(token_id).call()
        except ContractLogicError as logic:
            raise Exception(logic.message)

    def get_token_type(self, token_id: int) -> int:
        return self._call("getTokenType", token_id)

    def get_ownership_history(self, token_id: int) -> list[tuple[str, str]]:
        return self._call("getOwnershipHistory", token_id)

    def get_token_creation_timestamp(self, token_id: int) -> int:
        return self._call("getTokenCreationTimestamp", token_id)


cosmic = None


def load():
    global cosmic
    cosmic = CosmicRelic(os.getenv("ALCHEMY_API_KEY"), os.getenv("NFT_ADDRESS"), 'contracts/crel.json')
