from database import db
from models.GameLevel import GameLevel


class UserProgress(db.Model):
    __tablename__ = 'user_progress'
    id = db.Column(db.Integer, primary_key=True, autoincrement=True)
    user_id = db.Column(db.Integer, db.ForeignKey('users.id'), nullable=False)
    game_level_id = db.Column(db.Integer, db.ForeignKey('game_level.id'), nullable=False)
    stars_collected = db.Column(db.Integer, nullable=False, server_default='0')
    kills = db.Column(db.Integer, nullable=False, server_default='0')
    deaths = db.Column(db.Integer, nullable=False, server_default='0')
    total_games = db.Column(db.Integer, nullable=False, server_default='0')
    total_completions = db.Column(db.Integer, nullable=False, server_default='0')
    relics_found = db.Column(db.Integer, nullable=False, server_default='0')

    __table_args__ = (db.UniqueConstraint('user_id', 'game_level_id', name='unique_player_level'),)

    def get_level(self):
        return db.session.query(GameLevel).filter(GameLevel.id == self.game_level_id).first()

    def as_json(self):
        return {
            'stars': self.stars_collected,
            'kills': self.kills,
            'deaths': self.deaths,
            'games': self.total_games,
            'completions': self.total_completions
        }

    @staticmethod
    def get_progress(user_id, game_id):
        return db.session.query(UserProgress).filter(
            UserProgress.user_id == user_id,
            UserProgress.game_level_id == game_id
        ).first()
    