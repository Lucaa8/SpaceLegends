from database import db
from sqlalchemy import select


class Perk(db.Model):
    __tablename__ = 'perk'
    id = db.Column(db.Integer, primary_key=True, autoincrement=True)
    type = db.Column(db.String(32), nullable=False) # "damage", "armor", "speed"
    value = db.Column(db.Integer, nullable=False)
    price_hour = db.Column(db.Float, nullable=False)
    price_day = db.Column(db.Float, nullable=False)
    price_week = db.Column(db.Float, nullable=False)

    @staticmethod
    def get_perk_by_id(perk_id: int) -> 'Perk | None':
        stmt = select(Perk).filter_by(id=perk_id)
        try:
            return db.session.execute(stmt).scalar_one_or_none()
        except Exception as e:
            print(f"An unknown error occurred while fetching perk by id=={perk_id}: {e}")
        return None
