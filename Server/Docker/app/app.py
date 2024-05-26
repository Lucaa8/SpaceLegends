from flask import Flask, render_template

import utils
from flask_session import Session
import os
from dotenv import load_dotenv
from chain import load as load_chain
from smtp_service import load as load_smtp_service
from collection import load as load_items
from database import load as load_database
from authentification import load as load_auth
from blueprints.views import views_bp
from blueprints.api import api_bp
from blueprints.auth import auth_bp

load_dotenv()

app = Flask(__name__)
app.config["SESSION_PERMANENT"] = True
app.config["SESSION_TYPE"] = "filesystem"
Session(app)

app.register_blueprint(views_bp)
app.register_blueprint(api_bp, url_prefix='/api')
app.register_blueprint(auth_bp, url_prefix='/auth')

utils.PROFILE_PIC_FOLDER = os.getenv('PROFILE_PICTURE_FOLDER')
utils.PROFILE_PIC_MAX_MB = float(os.getenv('MAX_PROFILE_PICTURE_SIZE_MB'))
if not os.path.isdir(f"static/{utils.PROFILE_PIC_FOLDER}"):
    os.mkdir(f"static/{utils.PROFILE_PIC_FOLDER}")


# Inject the function format_number in the jinja2 environment. Now this function can be called inside templates (if used with render_template)
app.jinja_env.filters['format_number'] = lambda value: f"{value:,}".replace(",", "'")


@app.errorhandler(404)
def page_not_found(e):
    return render_template('404.html'), 404


if __name__ == '__main__':
    load_smtp_service()
    load_chain()
    for file in os.listdir("data"):
        load_items(f"data/{file}")
    load_database(app)
    load_auth(app)

    with app.app_context():
        # Keep this import otherwise the create_all() method wont know about all the models (they are imported inside models/__init__.py)
        from models import __init__
        from database import db
        db.create_all()

    app.run(host="0.0.0.0", port=int(os.getenv("API_PORT")))
