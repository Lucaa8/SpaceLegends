from sqlalchemy import func
import sqlalchemy.exc
from database import db
from datetime import datetime, timedelta


class PerkRental(db.Model):
    __tablename__ = 'perkrental'
    id = db.Column(db.Integer, primary_key=True, autoincrement=True)
    user_id = db.Column(db.Integer, db.ForeignKey('users.id'), nullable=False)
    perk_id = db.Column(db.Integer, db.ForeignKey('perk.id'), nullable=False)
    start_time = db.Column(db.DateTime, server_default=func.now())
    end_time = db.Column(db.DateTime, nullable=False)
    duration = db.Column(db.String(10), nullable=False)  # "1H", "1D", "1W"
    perk = db.relationship('Perk', backref='rentals', lazy=True)

    @staticmethod
    def rent(perk_id, user_id, price: float, duration: str) -> 'PerkRental | None':
        format_duration = f"1{duration[0:1].upper()}" # "1H", "1D", "1W"
        start_time = datetime.utcnow()
        end_time = start_time
        if format_duration == '1H':
            end_time += timedelta(hours=1)
        elif format_duration == '1D':
            end_time += timedelta(days=1)
        elif format_duration == '1W':
            end_time += timedelta(weeks=1)
        try:
            from models.User import User
            u: User = User.get_user_by_id(user_id)
            if u is None or u.money_sdt < price:
                return None
            else:
                u.money_sdt -= price
            perk = PerkRental(user_id=user_id, perk_id=perk_id, start_time=start_time, end_time=end_time, duration=format_duration)
            db.session.add(perk)
            db.session.add(u)
            db.session.commit()
            return perk
        except Exception as e:
            db.session.rollback()
            print(f'A sql error occurred while renting a new perk for user_id=={user_id}, perk_id=={perk_id}, start_time=={start_time}, end_time=={end_time}. Error: {str(e)}')
            return None
