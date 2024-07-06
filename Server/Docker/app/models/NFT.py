from datetime import datetime

from database import db
from sqlalchemy import func, or_
import chain
from collection import Item


class NFT(db.Model):
    __tablename__ = 'nfts'

    id = db.Column(db.Integer, primary_key=True)
    type = db.Column(db.BigInteger)
    user_id = db.Column(db.Integer, db.ForeignKey('users.id'), nullable=False)
    # server_default means that the database server which handles the default field. So even if I add manually an entry from Adminer, this default field would be filled for me.
    created_at = db.Column(db.DateTime, server_default=func.now())
    is_minted = db.Column(db.Boolean, default=False, comment='Whether the NFT is already minted on the blockchain or not')
    is_pending = db.Column(db.Boolean, default=False, comment='Whether the NFT is currently in the process of being minted on the blockchain or not')
    is_listed = db.Column(db.Boolean, default=False, comment='Whether the NFT is currently listed on the market or not')
    dropped_by_level_id = db.Column(db.Integer, db.ForeignKey('game_level.id'), nullable=True) # if null then it has been received by offer or whatever and default probabilities are used

    @staticmethod
    def create(item: Item, user_id: int, dropped_by_level_id: int):
        nft = NFT(type=item.item_id, user_id=user_id, dropped_by_level_id=dropped_by_level_id)
        db.session.add(nft)
        db.session.commit()

    def as_complete_nft(self):
        from collection import get_item
        nft = get_item(self.type)
        data = nft.to_metadata()
        data['id'] = f"#{self.id}"
        data['listed'] = self.is_listed
        data['pending'] = self.is_pending
        # Could be fetched on the chain but the profile load during 2-3 seconds and thats bad. For chain fetched information look at the token explorer.
        data['created'] = self.created_at
        for attr in data['attributes']:
            data[attr['trait_type']] = nft.format_rarity() if attr['trait_type'] == 'Rarity' else attr['value']
        del data['attributes']
        return data

    @staticmethod
    def list_on_market(user_id: int, nft_id: int, price: float) -> bool:
        from models.MarketListing import MarketListing
        listed = MarketListing.add_listing(user_id, nft_id, price)
        if listed:
            nft = db.session.query(NFT).filter(NFT.id == nft_id).first()
            nft.is_listed = True
            db.session.commit()
        return listed

    @staticmethod
    def can_list_on_market(user_id: int, nft_id: int) -> str:
        from models.User import User
        vendor: User = User.get_user_by_id(user_id)
        if vendor is None:
            return f"User with id {user_id} not found"
        from chain import cosmic
        eth: float = cosmic.get_available_eth(vendor)
        if eth < cosmic.max_fee:
            return f"You need at least {cosmic.max_fee} SETH to be able to list a NFT on the market (Gas fee). You have {eth} SETH on your account."
        nft = db.session.query(NFT).filter(NFT.id == nft_id).first()
        if nft is None or nft.user_id != user_id or not nft.is_minted:
            return f"The NFT with id {nft_id} does not exist or is not yours."
        if nft.is_minted and nft.is_pending:
            return f"This NFT is not in your wallet yet. Please wait for the transaction to finish."
        if nft.is_listed:
            return f"This NFT is already listed."
        results = db.session.query(
            func.count(NFT.type).label('count')
        ).filter(
            NFT.user_id == user_id,
            NFT.is_minted == 1,
            NFT.is_pending == 0,
            NFT.is_listed == 0,
            NFT.type == nft.type
        ).first()
        if results.count <= 1:
            return "You must have a minimum of 2 *MINTED* and *UNLISTED* NFTs of the same type to be able to list one of them."
        return "OK"

    @staticmethod
    def buy(buyer, nft_id) -> 'str | Any':
        from models.MarketListing import MarketListing
        listing = MarketListing.find_by_nft(nft_id)
        if listing is None:
            return "This listing does not exist, has been removed or already bought."
        if listing.nft.user_id == buyer.id:
            return "You cant buy your own NFT!"
        if buyer.money_sdt < listing.price:
            return f"Buyer has not enough money (Buyer has {buyer.money_sdt} but price is {listing.price})"
        # Removes money to buyer
        db.session.add(buyer)
        buyer.money_sdt -= listing.price
        db.session.commit()
        # Adds money to vendor
        from models.User import User
        vendor: User = User.get_user_by_id(listing.user_id)
        vendor.money_sdt += listing.price
        db.session.commit()
        # Closes the listing and change NFT owner
        db.session.add(listing)
        listing.bought_by = buyer.id
        listing.bought_time = datetime.utcnow()
        listing.nft.is_listed = False
        listing.nft.is_pending = True # Is not listed anymore but is_pending until the nft is in the buyer wallet. Needed so new buyer cant list this nft before receiving it
        listing.nft.user_id = buyer.id
        db.session.commit()
        from chain import cosmic
        cosmic.transfer_nft(vendor, buyer, nft_id)
        return vendor

    @staticmethod
    def on_buy(token_id):
        from database import flask_app
        with flask_app.app_context():
            nft = db.session.query(NFT).filter(NFT.id == token_id).first()
            nft.is_pending = False
            db.session.commit()

    @staticmethod
    def mint(token_id, wallet, username):
        from database import flask_app
        with flask_app.app_context():
            nft = db.session.query(NFT).filter(NFT.id == token_id).first()
            if nft is None or nft.is_minted or nft.is_pending:
                return
            nft.is_pending = True
            db.session.commit()
            chain.cosmic.mint_nft(wallet, username, nft.id, nft.type)

    @staticmethod
    def on_mint(token_id):
        from database import flask_app
        with flask_app.app_context():
            nft = db.session.query(NFT).filter(NFT.id == token_id).first()
            if nft.is_pending:
                nft.is_minted = True
                nft.is_pending = False
                db.session.commit()

    @staticmethod
    def get_nft_count_by_type(user_id):
        results = db.session.query(
            NFT.type,
            func.count(NFT.type).label('count')
        ).filter(
            NFT.user_id == user_id,
            # The NFT could not be minted yet, but I still count it as unlocked so he can see it in his game
            or_(NFT.is_minted == 1, NFT.is_pending == 1)
        ).group_by(
            NFT.type
        ).all()
        return results

    # This method is used to determine how many unopened relics a player has for each collection.
    # It's then displayed in his shop and if count > 0 then he can open a relic of that collection.
    # It's important that pending transactions to mint a nft are NOT included in here otherwise a player can open the same relic multiple time before it was minted
    @staticmethod
    def get_unminted_nft_by_collections(user_id):
        from collection import _decode_token_type
        results = db.session.query(NFT.type).filter(NFT.user_id == user_id, NFT.is_minted == 0, NFT.is_pending == 0).all()
        collec = {}
        for nft in results:
            c = _decode_token_type(nft[0])[0]
            collec[c] = collec.get(c, 0) + 1

        return collec

    # This method is used to find a new nft to open when the player clicks on the "open" button of a collection in his shop.
    # # It's important that pending transactions to mint a nft are NOT included in here otherwise a player can open the same relic multiple time before it was minted
    @staticmethod
    def get_first_unminted_nft(user_id, collec_id):
        from collection import _decode_token_type
        results = db.session.query(NFT).filter(NFT.user_id == user_id, NFT.is_minted == 0, NFT.is_pending == 0).all()
        for nft in results:
            c = _decode_token_type(nft.type)[0]
            if c == collec_id:
                return nft
        return None

    # Misleading name as the function returns the number of relics found (you can test if complete if is_collection_complete(user_id, collection_id) == 9
    @staticmethod
    def is_collection_complete(user_id, collection_id) -> int:
        from collection import _decode_token_type
        # The NFT could not be minted yet, but I still count it as unlocked so he can receive the bonus perk associated with this collection if complete
        results = db.session.query(NFT.type).filter(NFT.user_id == user_id, or_(NFT.is_minted == 1, NFT.is_pending == 1)).all()
        unique_nft = set()
        for nft in results:
            c = _decode_token_type(nft[0])[0]
            if c == collection_id:
                unique_nft.add(nft[0])
        return len(unique_nft)

    @staticmethod
    def is_token_minted(token_id):
        result = db.session.query(NFT.is_minted).filter(NFT.id == token_id).first()
        return result is not None and result[0]
