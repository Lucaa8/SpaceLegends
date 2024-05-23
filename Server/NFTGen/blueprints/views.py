from flask import render_template, session, redirect, url_for
from flask.blueprints import Blueprint
from collection import collections

views_bp = Blueprint('views', __name__, template_folder='templates')


@views_bp.context_processor
def inject_user():
    if "user_id" in session:
        from models.User import User
        user = User.get_user_by_id(session["user_id"])
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
    return render_template('market.html')


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


@views_bp.route('/profile', methods=['GET'])
def own_profile():
    if "user_id" in session:
        from models.User import User
        user = User.get_user_by_id(session.get("user_id"))
        if user:
            return render_template('profile.html', displayed_user=user, can_edit=True)
    return redirect(url_for('views.login'))


@views_bp.route('/profile/<username>', methods=['GET'])
def user_profile(username: str):
    from models.User import User
    user = User.get_user_by_creds(username=username, email=None)
    if user and user.email_verified:
        return render_template('profile.html', displayed_user=user, can_edit=False)
    return render_template('404.html'), 404
