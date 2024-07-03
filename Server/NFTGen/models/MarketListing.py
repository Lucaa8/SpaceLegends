from database import db
from sqlalchemy import func


class MarketListing(db.Model):
    __tablename__ = 'market_listing'
    id = db.Column(db.Integer, primary_key=True, autoincrement=True)
    user_id = db.Column(db.Integer, db.ForeignKey('users.id'), nullable=False)
    nft_id = db.Column(db.Integer, db.ForeignKey('nfts.id'), nullable=False)
    added_time = db.Column(db.DateTime, server_default=func.now(), nullable=False)
    price = db.Column(db.Float, nullable=False)
    bought_time = db.Column(db.DateTime, nullable=True)
    bought_by = db.Column(db.Integer, db.ForeignKey('users.id'), nullable=True)
    cancelled = db.Column(db.Boolean, nullable=False, server_default='0')

    nft = db.relationship('NFT', backref='nft_item', lazy=True)

    @staticmethod
    def add_listing(user_id, nft_id, price) -> bool:
        try:
            listing = MarketListing(user_id=user_id, nft_id=nft_id, price=price)
            db.session.add(listing)
            db.session.commit()
            return True
        except Exception as e:
            db.session.rollback()
            print(f"An unknown error occurred while registering a new market listing (with nft_id=={nft_id}): {e}")
            return False

    @staticmethod
    def remove_listing(user_id, nft_id) -> bool:
        listing = MarketListing.find_by_nft(nft_id)
        if listing is not None:
            if listing.user_id != user_id:
                return False
            try:
                listing.cancelled = True
                listing.nft.is_listed = False
                db.session.commit()
                return True
            except Exception as e:
                db.session.rollback()
                print(f"An unknown error occurred while removing a market listing (with nft_id=={nft_id}): {e}")
        return False

    @staticmethod
    def find_valid_listings_count(user_id: int) -> int:
        listing = db.session.query(func.count(MarketListing.id).label('count')).filter(
            MarketListing.user_id == user_id,
            MarketListing.bought_time == None, # Has not already ended
            MarketListing.cancelled == False # Has not been cancelled by the user
        ).first()
        return listing[0]

    @staticmethod
    def find_by_nft(nft_id: int) -> 'MarketListing | None':
        listing = db.session.query(MarketListing).filter(
            MarketListing.nft_id == nft_id,
            MarketListing.bought_time == None, # Has not already ended
            MarketListing.cancelled == False # Has not been cancelled by the user
        ).first()
        return listing
