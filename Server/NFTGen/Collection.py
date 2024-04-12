import json


def _encode_token_type(collection: int, row: int, col: int, rarity: int) -> int:
    return (collection << 27) | (row << 11) | (col << 3) | rarity


def _decode_token_type(encoded_type: int) -> tuple[int, int, int, int]:
    return (encoded_type >> 27) & 0xFFF, (encoded_type >> 11) & 0xFF, (encoded_type >> 3) & 0xFF, encoded_type & 0x7


def _decode_token_type_formatted(encoded_type: int) -> str:
    decoded_type: tuple[int, int, int, int] = _decode_token_type(encoded_type)
    return f"{str(decoded_type[0]).zfill(2)}_r{str(decoded_type[1]).zfill(2)}c{str(decoded_type[2]).zfill(2)}.png"


class Collection:
    def __init__(self, collection_id: int, name: str, description: str, collection_image_cid: str):
        self._collection_id = collection_id
        self._name = name
        self._description = description
        self._image_cid = collection_image_cid

    def get_collection_id(self) -> str:
        return str(self._collection_id).zfill(2)

    def get_image_base_url(self) -> str:
        return f"https://{self._image_cid}.ipfs.w3s.link/{self.get_collection_id()}_{self.name}_"

    @staticmethod
    def from_file(json_object):
        if ("id" not in json_object) or ("name" not in json_object) or ("description" not in json_object) or ("image_cid" not in json_object):
            print("Invalid JSON. Cannot build collection. Missing values.")
            return None
        return Collection(json_object["id"], json_object["name"], json_object["description"], json_object["image_cid"])

    @property
    def collection_id(self):
        return self._collection_id

    @property
    def name(self):
        return self._name


class Item:
    def __init__(self, item_id: int, name: str, description: str):
        decode_id = _decode_token_type(item_id)
        self._item_id = item_id
        self._collection: Collection = get_collection(decode_id[0])
        self._row = decode_id[1]
        self._col = decode_id[2]
        self._rarity = decode_id[3] # 1=Common, 2=Rare, 3=Epic, 4=Legendary
        self._name = name
        self._description = description

    @property
    def item_id(self):
        return self._item_id

    def get_image(self):
        return f"{self._collection.get_image_base_url()}r{str(self._row).zfill(2)}c{str(self._col).zfill(2)}.png"

    @staticmethod
    def from_file(json_object):
        if ("row" not in json_object) or ("col" not in json_object) or ("rarity" not in json_object) or ("collection_id" not in json_object):
            print("Invalid JSON. Cannot build item token type.")
            return None
        if ("name" not in json_object) or ("description" not in json_object):
            print("Invalid JSON. Cannot retrieve item name and/or description.")
            return None
        item_id = _encode_token_type(json_object["collection_id"], json_object["row"], json_object["col"], json_object["rarity"])
        return Item(item_id, json_object["name"], json_object["description"])

    def to_metadata(self):
        return {
            "name": self._name,
            "description": self._description,
            "external_url": "https://space-legends.luca-dc.ch/",
            "image": self.get_image(),
            "attributes": [
                {
                    "trait_type": "Collection",
                    "value": f"{self._collection.get_collection_id()} {self._collection.name}"
                },
                {
                    "display_type": "number",
                    "trait_type": "Row",
                    "value": self._row
                },
                {
                    "display_type": "number",
                    "trait_type": "Column",
                    "value": self._col
                },
                {
                    "display_type": "number",
                    "trait_type": "Rarity",
                    "max_value": "4",
                    "value": self._rarity
                }
            ]
        }


collections = []
items = []


def get_collection(collection_id: int) -> Collection | None:
    for collection in collections:
        if collection.collection_id == collection_id:
            return collection
    return None


def get_item(item_id: int) -> Item | None:
    for item in items:
        if item.item_id == item_id:
            return item
    return None


def load(file: str) -> None:
    with open(file, 'r', encoding="utf-8") as f:
        data = json.load(f)
        collections.append(Collection.from_file(data["collection"]))
        for item in data["items"]:
            items.append(Item.from_file(item))

