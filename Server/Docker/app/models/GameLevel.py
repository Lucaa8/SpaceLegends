from database import db


class GameLevel(db.Model):
    __tablename__ = 'game_level'
    id = db.Column(db.Integer, primary_key=True, autoincrement=True)
    collection = db.Column(db.String(32), nullable=False) # "00_Earth", "01_Mars"
    level = db.Column(db.Integer, nullable=False) # 0 for tutorial then 1, 2, 3, ...
    unlock_requirements = db.Column(db.Integer, nullable=False, server_default='0') # number of stars required to unlock this level
    difficulty = db.Column(db.String(6), nullable=False) # EASY, NORMAL, HARD (Determines the loot pool of this level)

    probabilities = db.relationship('CRELPropability', backref='probs', lazy=True)

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
