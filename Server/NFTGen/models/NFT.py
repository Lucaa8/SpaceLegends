from database import db
from datetime import datetime


class NFT(db.Model):
    __tablename__ = 'nfts'

    id = db.Column(db.Integer, primary_key=True)
    type = db.Column(db.Integer)
    user_id = db.Column(db.Integer, db.ForeignKey('users.id'), nullable=False)
    is_minted = db.Column(db.Boolean, default=False, comment='Whether the NFT is already minted on the blockchain or not')

    def as_complete_nft(self):
        from collection import get_item
        from chain import cosmic
        nft = get_item(self.type)
        created = datetime.utcfromtimestamp(cosmic.get_token_creation_timestamp(self.id))
        data = nft.to_metadata()
        data['id'] = f"#{self.id}"
        data['created'] = created.strftime('%Y-%m-%d at %H:%M:%S')
        for attr in data['attributes']:
            data[attr['trait_type']] = nft.format_rarity() if attr['trait_type'] == 'Rarity' else attr['value']
        del data['attributes']
        return data
