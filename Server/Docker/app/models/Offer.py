from database import db
from sqlalchemy import select


class Offer(db.Model):
    __tablename__ = 'offer'
    id = db.Column(db.Integer, primary_key=True, autoincrement=True)
    type = db.Column(db.String(32), nullable=False) # "sdt", "heart"
    value = db.Column(db.Integer, nullable=False)
    bonus_value = db.Column(db.Integer, nullable=False, server_default='0') #e.g. for the SDT, you can receive 5 heart as bonus
    bonus2_value = db.Column(db.Integer, nullable=False, server_default='0') #e.g. for the SDT, can can receive a CREL as bonus
    price = db.Column(db.Float, nullable=False)

    @staticmethod
    def get_offer_by_id(offer_id: int) -> 'Offer | None':
        stmt = select(Offer).filter_by(id=offer_id)
        try:
            return db.session.execute(stmt).scalar_one_or_none()
        except Exception as e:
            print(f"An unknown error occurred while fetching offer by id=={offer_id}: {e}")
        return None

    def buy(self, user) -> int:
        if user.money_sdt < self.price:
            return -1
        else:
            try:
                user.money_sdt -= self.price
                user.money_heart += self.value
                db.session.add(user)
                db.session.commit()
                return user.money_heart
            except Exception as e:
                db.session.rollback()
                print(f'A sql error occurred while buying an offer ({self.id}) for user_id=={user.id}. Error: {str(e)}')
            return -1
