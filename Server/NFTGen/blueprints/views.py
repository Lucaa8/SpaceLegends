from flask import render_template
from flask.blueprints import Blueprint
from Collection import collections

views_bp = Blueprint('views', __name__, template_folder='templates')


@views_bp.route('/')
def index():
    return render_template('index.html')


@views_bp.route('/token-explorer')
def token_explorer():
    nfts = []
    for collec in collections:
        nfts.append({
            "collection": collec.to_json(),
            "items": collec.get_items()
        })
    return render_template('token_explorer.html', nfts=nfts)


@views_bp.route('/download')
def download():
    return render_template('download.html')


@views_bp.route('/market')
def market():
    return render_template('market.html')


@views_bp.route('/register')
def register():
    return render_template('register.html')


@views_bp.route('/login')
def login():
    return render_template('login.html')
