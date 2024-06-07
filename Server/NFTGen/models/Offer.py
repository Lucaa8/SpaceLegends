from database import db


class Offer(db.Model):
    __tablename__ = 'offer'
    id = db.Column(db.Integer, primary_key=True, autoincrement=True)
    type = db.Column(db.String(32), nullable=False) # "sdt", "heart"
    value = db.Column(db.Integer, nullable=False)
    bonus_value = db.Column(db.Integer, nullable=False, server_default=0) #e.g. for the SDT, you can receive 5 heart as bonus
    bonus2_value = db.Column(db.Integer, nullable=False, server_default=0) #e.g. for the SDT, can can receive a CREL as bonus
    price = db.Column(db.Float, nullable=False)
