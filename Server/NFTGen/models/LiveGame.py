from datetime import datetime

from database import db
from sqlalchemy import func
import uuid


class LiveGame(db.Model):
    __tablename__ = 'live_game'
    id = db.Column(db.Integer, primary_key=True, autoincrement=True)
    code = db.Column(db.String(36), unique=True, nullable=False)
    user_id = db.Column(db.Integer, db.ForeignKey('users.id'), nullable=False)
    level_id = db.Column(db.Integer, db.ForeignKey('game_level.id'), nullable=False)
    started_at = db.Column(db.DateTime, nullable=False, server_default=func.now())
    finished_at = db.Column(db.DateTime, nullable=True)
    completed = db.Column(db.Boolean, nullable=False, server_default='0')

    @staticmethod
    def get(code: str) -> 'LiveGame | None':
        game = LiveGame.query.filter_by(code=code).first()
        return game

    def has_ended(self) -> bool:
        return self.finished_at is not None

    @staticmethod
    def start(user, level_id) -> str | None:
        try:
            code = str(uuid.uuid4())
            live = LiveGame(code=code, user_id=user.id, level_id=level_id)
            db.session.add(live)
            db.session.commit()
            return code
        except Exception as e:
            db.session.rollback()
            print(f"Something went wrong while starting a live game session for user.id=={user.id} and level_id=={level_id}: {str(e)}")
        return None

    def finish(self, completed: bool) -> bool:
        try:
            self.finished_at = datetime.now()
            self.completed = completed
            db.session.commit()
            return True
        except Exception as e:
            db.session.rollback()
            print(f"Something went wrong while finishing the live game session ({self.code}) of user.id=={self.user_id}: {str(e)}")
            return False
