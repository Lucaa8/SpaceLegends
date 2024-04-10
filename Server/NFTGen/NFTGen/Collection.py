from app import baseURL

def _encore_token_type(collection: int, row: int, col: int) -> int:
    return (collection << 16) | (row << 8) | col

def _decode_token_type(encoded_type: int) -> tuple[int, int, int]:
    return (encoded_type >> 16) & 0xFF, (encoded_type >> 8) & 0xFF, encoded_type & 0xFF

def _decode_token_type_formatted(encoded_type: int) -> str:
    decoded_type: tuple[int, int, int] = _decode_token_type(encoded_type)
    return f"{str(decoded_type[0]).zfill(2)}_r{str(decoded_type[1]).zfill(2)}c{str(decoded_type[2]).zfill(2)}.png"

class Collection:
    def __init__(self, collection_id: int, name: str, description: str):
        self._collection_id = collection_id
        self._name = name
        self._description = description

    def get_collection_id(self) -> str:
        return str(self._collection_id).zfill(2)

    def json(self) -> dict[str, str]:
        return {
            'ID': self.get_collection_id(),
            'Name': self._name,
            'Description': self._description
        }

    @property
    def collection_id(self):
        return self._collection_id

    @property
    def name(self):
        return self._name


collections = [
    Collection(0, "Earth", "The default planet."),
    Collection(1, "Mars", "The red planet."),
]

def get_collection(collection_id: int) -> Collection | None:
    for collection in collections:
        if collection.collection_id == collection_id:
            return collection
    return None

class Item:
    def __init__(self, item_id: int, name: str, description: str):
        decode_id = _decode_token_type(item_id)
        self._item_id = item_id
        self._collection : Collection = get_collection(decode_id[0])
        self._row = decode_id[1]
        self._col = decode_id[2]
        self._name = name
        self._description = description

    def get_image(self):
        return f"{self._collection.get_collection_id()}_{self._collection.name}_r{str(self._row).zfill(2)}c{str(self._col).zfill(2)}.png"

    def json(self):
        return {
            "name": self._name,
            "description": self._description,
            "external_url": "https://space-legends.luca-dc.ch/",
            "image": f"{baseURL}{self.get_image()}",
            "attributes": [
                {
                    "display_type": "number",
                    "trait_type": "Rarity",
                    "value": 2
                }
            ]
        }

items = [
    Item(257, "Earth N°1", "x description")
]