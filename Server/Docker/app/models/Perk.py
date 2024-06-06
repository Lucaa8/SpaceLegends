from database import db


class Perk(db.Model):
    __tablename__ = 'perk'
    id = db.Column(db.Integer, primary_key=True, autoincrement=True)
    type = db.Column(db.String(32), nullable=False) # "damage", "armor", "speed"
    value = db.Column(db.Integer, nullable=False)

