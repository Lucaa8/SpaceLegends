import sqlalchemy.exc
from sqlalchemy import select, func
from database import db


class User(db.Model):
    __tablename__ = 'users'

    id = db.Column(db.Integer, primary_key=True, autoincrement=True)
    username = db.Column(db.String(16), unique=True, nullable=False)
    email = db.Column(db.String(255), unique=True, nullable=False)
    email_verified = db.Column(db.Boolean, nullable=False, default=False, comment='If the user has proven that this is his email')
    email_verification_code = db.Column(db.String(36), unique=True, nullable=False, comment='The code that the user must verify') # 36 like UUIDs
    display_name = db.Column(db.String(24))
    # server_default means that the database server which handles the default field. So even if I add manually an entry from Adminer, this default field would be filled for me.
    joined_date = db.Column(db.DateTime, server_default=func.now())
    password = db.Column(db.String(128), nullable=False)
    salt = db.Column(db.String(32), nullable=False)
    wallet_address = db.Column(db.String(42), nullable=False)
    wallet_verified = db.Column(db.Boolean, nullable=False, default=False, comment='If the user has proven that this is his wallet')

    # Relationship to nfts
    nfts = db.relationship('NFT', backref='user', lazy=True)

    def __repr__(self):
        return (
            f"User(id={self.id}, username={self.username}, email={self.email} (Verified: {'Yes' if self.email_verified else 'No'}), display_name={self.display_name}, wallet_address={self.wallet_address} (Verified: {'Yes' if self.wallet_verified else 'No'}))"
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

    @staticmethod
    def get_user_by_creds(username: str | None, email: str | None) -> 'User | None':
        stmt = None
        if username is not None:
            stmt = select(User).filter_by(username=username)
        elif email is not None:
            stmt = select(User).filter_by(email=email)
        if stmt is not None:
            try:
                return db.session.execute(stmt).scalar_one_or_none()
            except Exception as e:
                print(f"An unknown error occurred while fetching user by {'email=='+str(email) if email is not None else 'username=='+str(username)}: {e}")
        return None

    @staticmethod
    def validate_email(code: str) -> bool:
        stmt = select(User).filter_by(email_verification_code=code)
        user = None
        try:
            user = db.session.execute(stmt).scalar_one_or_none()
        except Exception as e:
            print(f"An unknown error occurred while fetching user by email_verification_code=={code}: {e}")
        if user and (not user.email_verified):
            try:
                user.email_verified = True
                db.session.commit()
                return True
            except Exception as e:
                db.session.rollback()
                print(f"An unknown error occurred while validating email of user.id=={user.id}: {e}")
        return False
