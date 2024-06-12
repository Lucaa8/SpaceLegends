from database import db


class CRELPropability(db.Model):
    __tablename__ = 'crel_probability'
    id = db.Column(db.Integer, primary_key=True, autoincrement=True)
    level_id = db.Column(db.Integer, db.ForeignKey('game_level.id'), nullable=False)
    common = db.Column(db.Float, nullable=False)
    rare = db.Column(db.Float, nullable=False)
    epic = db.Column(db.Float, nullable=False)
    legendary = db.Column(db.Float, nullable=False)

    def as_json(self):
        return [self.common, self.rare, self.epic, self.legendary]

    @staticmethod
    def get_probabilities(level_id):
        return db.session.query(CRELPropability).filter(CRELPropability.level_id == level_id).first()

