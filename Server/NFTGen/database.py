from flask_sqlalchemy import SQLAlchemy
import os

db = None
flask_app = None # Needed to get back the app context in some async cases of db modifications


def load(app):
    DB_HOST = os.getenv('DATABASE_HOST', 'localhost:3306') # 2nd arg is a default value
    DB_NAME = os.getenv('DATABASE_NAME', 'spacelegends')
    DB_USER = os.getenv('DATABASE_USER', 'root')
    DB_PASS = os.getenv('DATABASE_PASS')
    app.config['SQLALCHEMY_DATABASE_URI'] = f"mysql+pymysql://{DB_USER}:{DB_PASS}@{DB_HOST}/{DB_NAME}?charset=utf8mb4"
    app.config["SQLALCHEMY_TRACK_MODIFICATIONS"] = False # TBD
    global db
    db = SQLAlchemy(app)
    global flask_app
    flask_app = app
