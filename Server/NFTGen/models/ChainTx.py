import base64
import json
from datetime import datetime

from database import db, flask_app
from sqlalchemy import func, select


class ChainTx(db.Model):
    __tablename__ = 'chain_tx'
    id = db.Column(db.Integer, primary_key=True, autoincrement=True)
    from_address = db.Column(db.String(42), nullable=False)
    from_pkey = db.Column(db.String(128), nullable=False)
    created_at = db.Column(db.DateTime, nullable=False, server_default=func.now())
    sent_at = db.Column(db.DateTime, nullable=True)
    completed_at = db.Column(db.DateTime, nullable=True)
    tx_type = db.Column(db.String(4), nullable=False) # Either sdt or crel
    tx = db.Column(db.String(1024), unique=False, nullable=False)
    gas = db.Column(db.BigInteger, nullable=True, server_default='0')
    gas_price = db.Column(db.Float, nullable=True, server_default='0.0')

    @staticmethod
    def get_all_unsent(from_addr=None):
        try:
            with flask_app.app_context():
                if from_addr is None:
                    stmt = select(ChainTx).filter_by(sent_at=None)
                else:
                    stmt = select(ChainTx).filter_by(sent_at=None, from_address=from_addr)
                return db.session.execute(stmt).all()
        except Exception as e:
            print(f"An unknown error occurred while fetching all unsent transactions: {e}")
        return list()

    @staticmethod
    def add_tx(wallet_addr: str, wallet_pkey: str, tx_type: str, tx_func: str, tx_args: tuple) -> None:
        args = [tx_func,]
        for arg in tx_args:
            args.append(arg)
        args = json.dumps(args)
        b64_args: str = base64.b64encode(args.encode()).decode()
        with flask_app.app_context():
            try:
                txn = ChainTx(from_address=wallet_addr, from_pkey=wallet_pkey, tx_type=tx_type, tx=b64_args, created_at=datetime.now())
                db.session.add(txn)
                db.session.commit()
            except Exception as e:
                db.session.rollback()
                print(f"An unknown error occurred while registering an new transaction: {e}")

    def decode_tx(self) -> tuple[str, tuple]:
        data = json.loads(base64.b64decode(self.tx).decode('utf-8'))
        return data[0], tuple() if len(data) <= 1 else data[1:]

    def prebuild_tx(self):
        fct, args = self.decode_tx()
        from chain import cosmic
        contract = getattr(cosmic, self.tx_type) # Either sdt or crel
        return getattr(contract.functions, fct)(*args)

    def sent(self):
        with flask_app.app_context():
            try:
                tx = db.session.query(ChainTx).filter(ChainTx.id == self.id).first()
                tx.sent_at = datetime.now()
                db.session.commit()
            except Exception as e:
                db.session.rollback()
                print(f"An unknown error occurred while changing sent_at of chain_tx.id=={self.id}: {e}")

    def completed(self, receipt, cosmic):
        gas_price_gwei = cosmic.w3.from_wei(receipt['effectiveGasPrice'], 'gwei')
        with flask_app.app_context():
            try:
                tx = db.session.query(ChainTx).filter(ChainTx.id == self.id).first()
                tx.completed_at = datetime.now()
                if receipt['status'] == 1: # transaction accepted
                    tx.gas = receipt['gasUsed']
                    tx.gas_price = gas_price_gwei
                    fct, args = self.decode_tx()
                    try:
                        getattr(type(cosmic), f"event_{tx.tx_type}_{fct}")(args)
                    except AttributeError: # if this transaction does not have any event
                        pass
                else:
                    print(f'Something went wrong with the chain_tx.id=={tx.id}. Check the receipt for more: {receipt}')
                db.session.commit()
            except Exception as e:
                db.session.rollback()
                print(f"An unknown error occurred while changing completed status of chain_tx.id=={self.id}: {e}")

