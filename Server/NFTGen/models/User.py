import sqlalchemy.exc
from database import db


class User(db.Model):
    __tablename__ = 'users'

    id = db.Column(db.Integer, primary_key=True, autoincrement=True)
    username = db.Column(db.String(16), unique=True, nullable=False)
    email = db.Column(db.String(255), unique=True, nullable=False)
    email_verified = db.Column(db.Boolean, nullable=False, default=False, comment='If the user has proven that this is his email')
    email_verification_code = db.Column(db.String(12), nullable=False, comment='The code that the user must verify')
    display_name = db.Column(db.String(24))
    password = db.Column(db.String(128), nullable=False)
    salt = db.Column(db.String(32), nullable=False)
    wallet_address = db.Column(db.String(42), nullable=False)
    wallet_verified = db.Column(db.Boolean, nullable=False, default=False, comment='If the user has proven that this is his wallet')

    # Relationship to nfts
    nfts = db.relationship('NFT', backref='user', lazy=True)

    def __repr__(self):
        return (
            f"User(id={self.id},\n"
            f"username={self.username},\n"
            f"email={self.email} (Verified: {'Yes' if self.email_verified else 'No'}),\n"
            f"display_name={self.display_name},\n"
            f"wallet_address={self.wallet_address} (Verified: {'Yes' if self.wallet_verified else 'No'}))"
        )

    @staticmethod
    def create_user(username: str, email: str, email_code: str, display_name: str | None, password: str, salt: str, wallet: str) -> 'User':
        try:
            user = User(username=username, email=email, email_verification_code=email_code, display_name=display_name, password=password, salt=salt, wallet_address=wallet)
            db.session.add(user)
            db.session.commit()
            return user
        except sqlalchemy.exc.IntegrityError:
            db.session.rollback()
            raise Exception('User already exists')
        except Exception as e:
            db.session.rollback()
            print(f"An unknown error occurred while registering an new user: {e}")
            raise Exception('Failed to create a new user. Invalid data.')

