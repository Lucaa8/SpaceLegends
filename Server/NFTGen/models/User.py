from database import db
from models.NFT import NFT # Not used but forces SQLAlchemy to load the NFT model before doing the relationship


class User(db.Model):
    __tablename__ = 'users'

    id = db.Column(db.Integer, primary_key=True, autoincrement=True)
    username = db.Column(db.String(16), unique=True, nullable=False)
    email = db.Column(db.String(255), unique=True, nullable=False)
    email_verified = db.Column(db.Boolean, nullable=False, default=False, comment='If the user has proven that this is his email')
    email_verification_code = db.Column(db.Integer, nullable=False, comment='The code that the user must verify')
    display_name = db.Column(db.String(24))
    password = db.Column(db.String(128), nullable=False)
    salt = db.Column(db.String(32), nullable=False)
    wallet_address = db.Column(db.String(42), nullable=False)
    wallet_verified = db.Column(db.Boolean, nullable=False, default=False, comment='If the user has proven that this is his wallet')

    # Relationship to nfts
    nfts = db.relationship('NFT', backref='user', lazy=True)

    def __repr__(self):
        return '<User %r>' % self.username