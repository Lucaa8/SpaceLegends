import os
from datetime import timedelta
from flask_jwt_extended import JWTManager, create_access_token, create_refresh_token
from flask import jsonify, session

jwt = None


def load(app):
    app.config["JWT_SECRET_KEY"] = os.getenv("JWT_SECRET_KEY")
    app.config["JWT_TOKEN_LOCATION"] = ["headers"]
    app.config["JWT_ACCESS_TOKEN_EXPIRES"] = timedelta(hours=1)
    app.config["JWT_REFRESH_TOKEN_EXPIRES"] = timedelta(days=45)
    app.config['JWT_BLACKLIST_ENABLED'] = True
    app.config['JWT_BLACKLIST_TOKEN_CHECKS'] = ['access', 'refresh']

    app.before_request(check_if_user_banned)

    global jwt
    # https://flask-jwt-extended.readthedocs.io/en/stable/
    jwt = JWTManager(app)

    # Needs to register callback functions without the decorator because @jwt is not initialized when functions are declared
    jwt.user_identity_loader(user_identity_lookup)
    jwt.user_lookup_loader(user_lookup_callback)

    jwt.invalid_token_loader(jwt_generic_error)
    jwt.unauthorized_loader(jwt_generic_error)
    jwt.revoked_token_loader(jwt_token_revoked)
    jwt.needs_fresh_token_loader(jwt_route_needs_fresh_token) # fresh token wont be used but who knows (fresh tokens != refresh tokens
    jwt.expired_token_loader(jwt_token_expired)

    jwt.token_in_blocklist_loader(check_if_token_valid)


def user_identity_lookup(user_id):
    return user_id


def user_lookup_callback(_jwt_header, jwt_data):
    from models import User
    identity = jwt_data["sub"]
    return User.get_user_by_id(identity)


def jwt_generic_error(error):
    return jsonify({"msg": "Access Denied"}), 401


def jwt_route_needs_fresh_token(jwt_header, jwt_data):
    return jsonify({"msg": "This route needs a fresh token"}), 401


def jwt_token_expired(jwt_header, jwt_data):
    return jsonify({"msg": "This token has expired"}), 401


def jwt_token_revoked(jwt_header, jwt_data):
    return jsonify({"msg": "This token has expired"}), 401


def check_if_token_valid(jwt_header, jwt_payload: dict) -> bool:
    from models.TokenRevoked import TokenRevoked
    jti = jwt_payload.get("jti")
    token = TokenRevoked.get_by_jti(jti=jti)
    if token is None: # Token was revoked (logout)
        return True
    from models.User import User
    user_id = jwt_payload.get('sub') # A little bit of copy paste from the check_if_user_banned. But in some case user_id is not in the session and some /api call can be still done. We need to recheck here.
    user = User.get_user_by_id(user_id)
    valid: bool = user is not None and user.banned != 1
    if not valid and "user_id" in session:
        session.pop('user_id', None)
    # return true if user has been deleted or user is banned
    return not valid


def check_if_user_banned():
    from models.User import User
    if "user_id" in session:
        u = User.get_user_by_id(session.get("user_id"))
        if u is not None and u.banned == 1:
            session.pop('user_id', None)
            return jsonify(message="Your account has been banned."), 401


def create_refresh(user):
    return create_refresh_token(identity=user.id)


def create_access(user):
    return create_access_token(identity=user.id)
