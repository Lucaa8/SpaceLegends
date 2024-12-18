from database import db
from models.GameLevel import GameLevel


class UserProgress(db.Model):
    __tablename__ = 'user_progress'
    id = db.Column(db.Integer, primary_key=True, autoincrement=True)
    user_id = db.Column(db.Integer, db.ForeignKey('users.id'), nullable=False)
    game_level_id = db.Column(db.Integer, db.ForeignKey('game_level.id'), nullable=False)
    star_1 = db.Column(db.Boolean, nullable=False, server_default='0')
    star_2 = db.Column(db.Boolean, nullable=False, server_default='0')
    star_3 = db.Column(db.Boolean, nullable=False, server_default='0')
    kills = db.Column(db.Integer, nullable=False, server_default='0')
    deaths = db.Column(db.Integer, nullable=False, server_default='0')
    total_games = db.Column(db.Integer, nullable=False, server_default='0')
    total_completions = db.Column(db.Integer, nullable=False, server_default='0')
    relics_found = db.Column(db.Integer, nullable=False, server_default='0')

    __table_args__ = (db.UniqueConstraint('user_id', 'game_level_id', name='unique_player_level'),)

    def get_user(self):
        from models.User import User
        return User.get_user_by_id(self.user_id)

    def get_level(self):
        return db.session.query(GameLevel).filter(GameLevel.id == self.game_level_id).first()

    def as_json(self):
        return {
            'stars': {
                'star_1': self.star_1,
                'star_2': self.star_2,
                'star_3': self.star_3
            },
            'kills': self.kills,
            'deaths': self.deaths,
            'games': self.total_games,
            'completions': self.total_completions
        }

    def update(self):
        try:
            db.session.commit()
        except Exception as e:
            print(f"Something went wrong while updating progress of user.id=={self.user_id} and level_id=={self.game_level_id}: {str(e)}")

    @staticmethod
    def get_progress(user_id, game_id, create=False):
        progress = db.session.query(UserProgress).filter(
            UserProgress.user_id == user_id,
            UserProgress.game_level_id == game_id
        ).first()
        if progress is None and create is True:
            progress = UserProgress(user_id=user_id, game_level_id=game_id)
            db.session.add(progress)
            db.session.commit()
            return progress
        return progress

    @staticmethod
    def get_all():
        return db.session.query(UserProgress).all()

    