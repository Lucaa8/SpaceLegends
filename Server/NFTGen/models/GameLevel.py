from database import db


class GameLevel(db.Model):
    __tablename__ = 'game_level'
    id = db.Column(db.Integer, primary_key=True, autoincrement=True)
    collection = db.Column(db.String, Nullable=False) # "00_Earth", "01_Mars"
    level = db.Column(db.Integer, Nullable=False) # 0 for tutorial then 1, 2, 3, ...
    unlock_requirements = db.Column(db.Integer, nullable=False, server_default=0) # number of stars required to unlock this level
    difficulty = db.Column(db.String, nullable=False) # EASY, NORMAL, HARD (probabilities of drops of relics determined by this)

    probabilities = db.relationship('CRELPropability', backref='probs', lazy=True)

