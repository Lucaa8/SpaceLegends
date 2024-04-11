from app import baseURL


def _encore_token_type(collection: int, row: int, col: int, rarity: int) -> int:
    return (collection << 27) | (row << 11) | (col << 3) | rarity


def _decode_token_type(encoded_type: int) -> tuple[int, int, int, int]:
    return (encoded_type >> 27) & 0xFFF, (encoded_type >> 11) & 0xFF, (encoded_type >> 3) & 0xFF, encoded_type & 0x7


def _decode_token_type_formatted(encoded_type: int) -> str:
    decoded_type: tuple[int, int, int, int] = _decode_token_type(encoded_type)
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
        self._collection: Collection = get_collection(decode_id[0])
        self._row = decode_id[1]
        self._col = decode_id[2]
        self._rarity = decode_id[3]
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
                    "trait_type": "Collection",
                    "value": f"{self._collection.get_collection_id()} {self._collection.name}"
                },
                {
                    "display_type": "number",
                    "trait_type": "Row",
                    "value": f"{str(self._row).zfill(2)}"
                },
                {
                    "display_type": "number",
                    "trait_type": "Column",
                    "value": f"{str(self._col).zfill(2)}"
                },
                {
                    "display_type": "number",
                    "trait_type": "Rarity",
                    "value": self._rarity
                    #Common, Rare, Epic, Legendary
                }
            ]
        }


items = [
    # Earth (refaire pr ajouter rareté)
    Item(257, "Earth N°1", "Icy tendrils cling to the skeletal remains of urban progress, a frozen testament to a world in silent aftermath."),
    Item(258, "Earth N°2", "Morning light whispers over the icy tomb of civilization, where once the bustle of life thrived now stands a chilling hush."),
    Item(259, "Earth N°3", "The sun graces the frost-laden edifices of humanity’s hubris, offering no warmth to the cold desolation of regret."),
    Item(513, "Earth N°4", "A car, once a symbol of freedom, now lies shackled in the relentless grip of an eternal winter’s embrace."),
    Item(514, "Earth N°5", "A convoy of icy relics, stand in silent parade amidst the frozen breath of Earth’s desolate sigh."),
    Item(515, "Earth N°6", "The urban graveyard sprawls into the horizon, with icy shrouds enveloping the echoes of a silenced world."),
    Item(769, "Earth N°7", "Beneath a crystal cloak, a lone car rests, a frosty epitaph to the stillness that has befallen the Earth."),
    Item(770, "Earth N°8", "A blanket of frost encases remnants of a rush hour frozen in time, an arctic echo of once warmer days."),
    Item(771, "Earth N°9", "Nature’s icy fingers clasp over the remains of humanity's haste, a chilling reminder of the planet’s stoic indifference."),
    # Mars
    Item(65793, "Mars N°1", "Against the canvas of the cosmos, ships sail towards the crimson horizon, their silhouettes a beacon of mankind’s unwavering resolve."),
    Item(65794, "Mars N°2", "As dust dances in the Martian twilight, explorers ascend in a trio of light, defying the gravity of their challenges."),
    Item(65795, "Mars N°3", "The behemoth of interplanetary travel stands sentinel over a barren land, a monolith to human ambition amidst the Martian desolation."),
    Item(66049, "Mars N°4", "Biodomes bloom in the desert’s embrace, fragile bubbles of life against the might of a relentless Martian storm."),
    Item(66050, "Mars N°5", "A Martian settlement clings to survival, its domed oasis standing resilient in the face of a sprawling, dust-swept expanse."),
    Item(66051, "Mars N°6", "Towering columns of human ingenuity rise from the Martian soil, steel veins channeling the lifeblood of a nascent civilization."),
    Item(66305, "Mars N°7", "Astronauts tread softly on alien soil, their figures casting long shadows over the red dust that is now their home."),
    Item(66306, "Mars N°8", "Workers toil amidst a network of machinery, their efforts a testament to humanity’s persistence under a foreign sun."),
    Item(66307, "Mars N°9", "Pioneers traverse the rugged Martian terrain, their paths illuminated by the artificial glow of a settlement on the frontier of the unknown.")
]
