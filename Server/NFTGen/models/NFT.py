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
