from database import db
from datetime import datetime
from sqlalchemy import func


class NFT(db.Model):
    __tablename__ = 'nfts'

    id = db.Column(db.Integer, primary_key=True)
    type = db.Column(db.BigInteger)
    user_id = db.Column(db.Integer, db.ForeignKey('users.id'), nullable=False)
    # server_default means that the database server which handles the default field. So even if I add manually an entry from Adminer, this default field would be filled for me.
    created_at = db.Column(db.DateTime, server_default=func.now())
    is_minted = db.Column(db.Boolean, default=False, comment='Whether the NFT is already minted on the blockchain or not')
    dropped_by_level_id = db.Column(db.Integer, db.ForeignKey('game_level.id'), nullable=True) # if null then it has been received by offer or whatever and default probabilities are used

    def as_complete_nft(self):
        from collection import get_item
        nft = get_item(self.type)
        data = nft.to_metadata()
        data['id'] = f"#{self.id}"
        # Could be fetched on the chain but the profile load during 2-3 seconds and thats bad. For chain fetched information look at the token explorer.
        data['created'] = self.created_at
        for attr in data['attributes']:
            data[attr['trait_type']] = nft.format_rarity() if attr['trait_type'] == 'Rarity' else attr['value']
        del data['attributes']
        return data

    @staticmethod
    def get_nft_count_by_type(user_id):
        results = db.session.query(
            NFT.type,
            func.count(NFT.type).label('count')
        ).filter(
            NFT.user_id == user_id,
            NFT.is_minted == 1
        ).group_by(
            NFT.type
        ).all()
        return results

    @staticmethod
    def get_unminted_nft_by_collections(user_id):
        from collection import _decode_token_type
        results = db.session.query(NFT.type).filter(NFT.user_id == user_id, NFT.is_minted == 0).all()
        collec = {}
        for nft in results:
            c = _decode_token_type(nft[0])[0]
            collec[c] = collec.get(c, 0) + 1

        return collec

    @staticmethod
    def get_first_unminted_nft(user_id, collec_id):
        from collection import _decode_token_type
        results = db.session.query(NFT.type, NFT.dropped_by_level_id).filter(NFT.user_id == user_id, NFT.is_minted == 0).all()
        for nft in results:
            c = _decode_token_type(nft[0])[0]
            if c == collec_id:
                return nft
        return None

    # Misleading name as the function returns the number of relics found (you can test if complete if is_collection_complete(user_id, collection_id) == 9
    @staticmethod
    def is_collection_complete(user_id, collection_id) -> int:
        from collection import _decode_token_type
        results = db.session.query(NFT.type).filter(NFT.user_id == user_id, NFT.is_minted == 1).all()
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
