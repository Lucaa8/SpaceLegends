from database import db


class NFT(db.Model):
    __tablename__ = 'nfts'

    id = db.Column(db.Integer, primary_key=True)
    type = db.Column(db.Integer)
    user_id = db.Column(db.Integer, db.ForeignKey('users.id'), nullable=False)
    is_minted = db.Column(db.Boolean, default=False, comment='Whether the NFT is already minted on the blockchain or not')
