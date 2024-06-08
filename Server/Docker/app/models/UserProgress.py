from database import db


class UserProgress(db.Model):
    __tablename__ = 'user_progress'
    id = db.Column(db.Integer, primary_key=True, autoincrement=True)
    user_id = db.Column(db.Integer, db.ForeignKey('users.id'), nullable=False)
    game_level_id = db.Column(db.Integer, db.ForeignKey('game_level.id'), nullable=False)
    stars_collected = db.Column(db.Integer, nullable=False, server_default='0')
    kills = db.Column(db.Integer, nullable=False, server_default='0')
    deaths = db.Column(db.Integer, nullable=False, server_default='0')
    completions = db.Column(db.Integer, nullable=False, server_default='0')
    relics_found = db.Column(db.Integer, nullable=False, server_default='0')

    __table_args__ = (db.UniqueConstraint('user_id', 'game_level_id', name='unique_player_level'),)
    