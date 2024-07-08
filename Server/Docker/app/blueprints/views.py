import base64
import json

from flask import render_template, session, redirect, url_for
from flask.blueprints import Blueprint
from collection import collections, items
from utils import user_session, get_user_from_session, get_profile_pic

views_bp = Blueprint('views', __name__, template_folder='templates')


@views_bp.context_processor # Needed for the header template (buttons on the right are different if a user is logged in)
def inject_user():
    user = get_user_from_session()
    if user:
        return dict(user=user)
    return dict(user=None)


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
    from models.MarketListing import MarketListing
    listings = []
    for listing in MarketListing.get_all_valid_listings():
        listing = listing.as_json()
        listing['nft']['created'] = listing['nft']['created'].isoformat()
        listings.append(listing)
    encoded = base64.b64encode(json.dumps(listings).encode())
    return render_template('market.html', listings=listings, json_listings=encoded.decode())


@views_bp.route('/register')
def register():
    if "user_id" in session:
        return redirect(url_for('views.own_profile'))
    return render_template('register.html')


@views_bp.route('/login')
def login():
    if "user_id" in session:
        return redirect(url_for('views.own_profile'))
    return render_template('login.html')


@views_bp.route('/forgot-password')
def forgot_password():
    return render_template('forgot_password.html')


@views_bp.route('/profile', methods=['GET'])
@user_session()
def own_profile(user):
    return render_template('profile.html', displayed_user=user, can_edit=True, max=len(items), current=len(user.nfts_discovered()), ppic=get_profile_pic(user.id))


@views_bp.route('/edit-profile', methods=['GET'])
@user_session()
def edit_profile(user):
    from chain import cosmic
    return render_template('edit_profile.html', current_sdt=cosmic.get_balance_sdt(user.wallet_address))


@views_bp.route('/profile/<username>', methods=['GET'])
def user_profile(username: str):
    from models.User import User
    user = User.get_user_by_creds(username=username, email=None)
    if user and user.email_verified:
        return render_template('profile.html', displayed_user=user, can_edit=False, max=len(items), current=len(user.nfts_discovered()), ppic=get_profile_pic(user.id))
    return render_template('404.html'), 404
