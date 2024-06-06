from sqlalchemy import func
from database import db


class PerkRental(db.Model):
    __tablename__ = 'perkrental'
    id = db.Column(db.Integer, primary_key=True, autoincrement=True)
    user_id = db.Column(db.Integer, db.ForeignKey('users.id'), nullable=False)
    perk_id = db.Column(db.Integer, db.ForeignKey('perk.id'), nullable=False)
    start_time = db.Column(db.DateTime, server_default=func.now())
    end_time = db.Column(db.DateTime, nullable=False)
    duration = db.Column(db.String(10), nullable=False)  # "1H", "1D", "1W"
    perk = db.relationship('Perk', backref='rentals', lazy=True)
