from database import db


class Perk(db.Model):
    __tablename__ = 'perk'
    id = db.Column(db.Integer, primary_key=True, autoincrement=True)
    type = db.Column(db.String(32), nullable=False) # "damage", "armor", "speed"
    value = db.Column(db.Integer, nullable=False)
    price_hour = db.Column(db.Float, nullable=False)
    price_day = db.Column(db.Float, nullable=False)
    price_week = db.Column(db.Float, nullable=False)
