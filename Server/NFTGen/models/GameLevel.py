from database import db
import random

drop_pool = {
    # DIFFICULTY : (PROBABILITY, TYPE, VALUE)
    'EASY':   [(0.125, 'HEART', 1), (0.125, 'SDT', 0.03), (0.25, 'RELIC', -1), (0.5, 'NONE', -1)],
    'NORMAL': [(0.180, 'HEART', 2), (0.180, 'SDT', 0.07), (0.64, 'RELIC', -1), (0.0, 'NONE', -1)],
    'HARD':   [(0.120, 'HEART', 6), (0.120, 'SDT', 0.20), (0.76, 'RELIC', -1), (0.0, 'NONE', -1)]
}


class GameLevel(db.Model):
    __tablename__ = 'game_level'
    id = db.Column(db.Integer, primary_key=True, autoincrement=True)
    collection = db.Column(db.String(32), nullable=False) # "00_Earth", "01_Mars"
    level = db.Column(db.Integer, nullable=False) # 0 for tutorial then 1, 2, 3, ...
    unlock_requirements = db.Column(db.Integer, nullable=False, server_default='0') # number of stars required to unlock this level
    difficulty = db.Column(db.String(6), nullable=False) # EASY, NORMAL, HARD (Determines the loot pool of this level)

    probabilities = db.relationship('CRELPropability', backref='probs', lazy=True)

    def generate_reward(self) -> tuple[str, any]:
        pool = drop_pool[self.difficulty]
        probabilities = [prob for prob, item, value in pool]
        reward = random.choices(pool, probabilities)[0]
        rew_type = reward[1]
        if rew_type != 'RELIC':
            return rew_type, reward[2]
        rarities = [1, 2, 3, 4] # COMMON, RARE, EPIC, LEGENDARY relic
        nft_probs: list[float, float, float, float] = self.probabilities[0].as_json()
        result: int = random.choices(rarities, nft_probs)[0]
        from collection import get_random_nft, Item
        item: Item = get_random_nft(self.collection, result)
        return rew_type, item

    @staticmethod
    def get(level_id: int) -> 'GameLevel | None':
        return db.session.query(GameLevel).filter(GameLevel.id == level_id).first()

    @staticmethod
    def get_levels():
        return db.session.query(GameLevel).all()

    def as_json(self):
        return {
            'id': self.id,
            'collection': self.collection,
            'level': self.level,
            'unlock_requirements': self.unlock_requirements,
            'difficulty': self.difficulty,
            'probabilities': self.probabilities[0].as_json()
        }
