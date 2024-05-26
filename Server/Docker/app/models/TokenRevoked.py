from database import db
from sqlalchemy import func, select


class TokenRevoked(db.Model):
    __tablename__ = 'tokens_revoked'

    id = db.Column(db.Integer, primary_key=True, autoincrement=True)
    type = db.Column(db.String(7), nullable=False)
    jti = db.Column(db.String(36), nullable=False, index=True)
    revoked_at = db.Column(db.DateTime, nullable=False, server_default=func.now())

    @staticmethod
    def revoke(jti, token_type):
        db.session.add(TokenRevoked(jti=jti, type=token_type))
        db.session.commit()

    @staticmethod
    def get_by_jti(jti):
        stmt = select(TokenRevoked.id).filter_by(jti=jti)
        try:
            return db.session.execute(stmt).scalar()
        except Exception as e:
            print(f"An unknown error occurred while fetching a revoked token by jti=={jti}: {e}")
        return None
