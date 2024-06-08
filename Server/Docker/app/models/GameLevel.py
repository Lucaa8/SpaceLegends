from database import db
from sqlalchemy.orm import joinedload


class GameLevel(db.Model):
    __tablename__ = 'game_level'
    id = db.Column(db.Integer, primary_key=True, autoincrement=True)
    collection = db.Column(db.String(32), nullable=False) # "00_Earth", "01_Mars"
    level = db.Column(db.Integer, nullable=False) # 0 for tutorial then 1, 2, 3, ...
    unlock_requirements = db.Column(db.Integer, nullable=False, server_default='0') # number of stars required to unlock this level
    difficulty = db.Column(db.String(6), nullable=False) # EASY, NORMAL, HARD (Determines the loot pool of this level)

    probabilities = db.relationship('CRELPropability', backref='probs', lazy=True)
    user_progress = db.relationship('UserProgress', backref='game_level', lazy=True)


def get_game_level_with_player_progress(game_level_id: int, user_id: int):
    from models.UserProgress import UserProgress
    result = db.session.query(GameLevel).join(UserProgress).filter(
        GameLevel.id == game_level_id,
        UserProgress.user_id == user_id
    ).options(
        joinedload(GameLevel.player_progress)
    ).first()

    return result

