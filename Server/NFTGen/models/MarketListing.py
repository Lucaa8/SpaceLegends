from database import db
from sqlalchemy import func


class MarketListing(db.Model):
    __tablename__ = 'market_listing'
    id = db.Column(db.Integer, primary_key=True, autoincrement=True)
    user_id = db.Column(db.Integer, db.ForeignKey('users.id'), nullable=False)
    nft_id = db.Column(db.Integer, db.ForeignKey('nfts.id'), nullable=False, unique=True)
    added_time = db.Column(db.DateTime, server_default=func.now(), nullable=False)
    price = db.Column(db.Float, nullable=False)
    bought_time = db.Column(db.DateTime, nullable=True)
    bought_by = db.Column(db.Integer, db.ForeignKey('users.id'), nullable=True)

    @staticmethod
    def add_listing(user_id, nft_id, price) -> bool:
        try:
            listing = MarketListing(user_id=user_id, nft_id=nft_id, price=price)
            db.session.add(listing)
            db.session.commit()
            return True
        except Exception as e:
            db.session.rollback()
            print(f"An unknown error occurred while registering a new market listing: {e}")
            return False
