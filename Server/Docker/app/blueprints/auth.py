from flask.blueprints import Blueprint
from flask import request, jsonify, url_for, session

from utils import USERNAME_REGEX, DISPLAYNAME_REGEX, PASSWORD_REGEX, EMAIL_REGEX, hash_password, verify_password, generate_confirmation_code, generate_password
from authentification import create_refresh, create_access
from flask_jwt_extended import jwt_required, current_user, get_jwt, decode_token

auth_bp = Blueprint('auth', __name__, template_folder='templates')


@auth_bp.route('register', methods=['POST'])
def register():
    username = request.form.get('username')
    display_name = request.form.get('displayname')
    email = request.form.get('email')
    password = request.form.get('pass1')
    confirm_password = request.form.get('pass2')
    wallet_address = request.form.get('walletAddress')

    errors = []
    try:
        if (not username) or (not USERNAME_REGEX.match(username)):
            errors.append('Invalid or missing username.')
        if display_name and (len(display_name) > 0) and (not DISPLAYNAME_REGEX.match(display_name)):
            errors.append('Invalid or missing display name.')
        if (not email) or (not EMAIL_REGEX.match(email)):
            errors.append('Invalid or missing email address.')
        if (not password) or (not PASSWORD_REGEX.match(password)):
            errors.append('Password must be at least 8 characters long.')
        if password != confirm_password:
            errors.append('Passwords do not match.')
        from chain import cosmic # cosmic still to None when auth.py is loaded into memory. Cannot import it at the start of this file.
        if (not wallet_address) or (not cosmic.check_address(wallet_address)):
            errors.append('Invalid or missing wallet address.')
    except Exception as e:
        errors.append(str(e))

    if errors:
        return jsonify({'message': errors}), 400

    if (not display_name) or (len(display_name) == 0): # If the user did not specify a display name, his username will be his display name
        display_name = username

    # DB register
    hashed_password, salt = hash_password(password)
    email_code: str = generate_confirmation_code()
    try:
        from models import User
        user = User.create_user(
            username=username,
            email=email,
            email_code=email_code,
            display_name=display_name,
            password=hashed_password,
            salt=salt,
            wallet=wallet_address
        ) # Tries to register the new user in the database
        print(f"Registered {user}")
        verification_url = f"https://space-legends.luca-dc.ch{url_for('api.verification', code=email_code)}"
        from smtp_service import smtp_service as smtp
        session['user_id'] = str(user.id)
        if not smtp.send_verification_email(username, email, verification_url):
            return jsonify({'message': 'Your account has been successfully created but we failed to send you the verification code. Please retry the confirmation later in your user profile.'}), 200
        return jsonify(refresh_token=create_refresh(user), access_token=create_access(user)), 200
    except Exception as e:
        return jsonify({'message': str(e)}), 400


@auth_bp.route('/login', methods=['POST'])
def login():
    username = request.form.get('username')
    password = request.form.get('password')

    errors = []
    try:
        if (not username) or (not USERNAME_REGEX.match(username)):
            errors.append('Invalid or missing username.')
        if (not password) or (not PASSWORD_REGEX.match(password)):
            errors.append('Invalid or missing password.')
    except Exception as e:
        errors.append(str(e))

    if errors:
        return jsonify({'message': errors}), 400

    from models import User
    user: User | None = User.get_user_by_creds(username=username, email=None)
    error_msg = 'Unknown user or invalid password.'
    if user:
        try:
            if verify_password(password, user.password, user.salt):
                session['user_id'] = str(user.id)
                return jsonify(refresh_token=create_refresh(user), access_token=create_access(user)), 200
        except Exception as e:
            print(f"An unknown error occurred while checking password on login for user.id=={user.id}: {str(e)}")
            error_msg = 'Something went wrong while checking your password. Try again later.'

    return jsonify({'message': error_msg}), 401


@auth_bp.route("/logout", methods=["DELETE"])
@jwt_required()
def logout():
    from models.TokenRevoked import TokenRevoked
    jti = get_jwt()["jti"]
    try:
        session.pop('user_id', None)
        TokenRevoked.revoke(jti, "access")
        if "refresh" in request.json:
            jti_refresh = decode_token(request.json["refresh"])["jti"]
            TokenRevoked.revoke(jti_refresh, "refresh")
        return '', 204
    except Exception as e:
        print(f"Failed to revoke one of the token for user.id=={current_user.id if current_user is not None else 'unknown'}: {str(e)}")
    return jsonify({'message': 'Failed to revoke token.'}), 400


@auth_bp.route("/refresh", methods=["POST"])
@jwt_required(refresh=True)
def refresh():
    access_token = create_access(user=current_user)
    return jsonify(access_token=access_token)


@auth_bp.route("/forgot-password", methods=["POST"])
def forgot_password():
    if "user_id" in session:
        return jsonify(message="You are already logged in."), 400

    if "username" in request.json:
        try:
            username = request.json["username"]
            if not USERNAME_REGEX.match(username):
                return jsonify(message="Invalid username"), 404
            from models.User import User
            user = User.get_user_by_creds(username=username, email=None)
            if user is None:
                return jsonify(message="Invalid username"), 404
            if not user.email_verified:
                return jsonify(message="Your email is not verified"), 404
            new_pass = generate_password()
            hashed_password, salt = hash_password(new_pass)
            if user.set_new_password(hashed_password, salt):
                from smtp_service import smtp_service
                smtp_service.send_new_password_email(user, new_pass)
                return jsonify(message="You received a new password in your email inbox! Please login with the new password and change it in your edit profile page!"), 200
            else:
                return jsonify(message="Something went wrong while updating your password. Please retry again later."), 500
        except Exception as e:
            print("An error occurred while trying to check username before a forgot-password procedure." + str(e))

    return jsonify(message="An unknown error occurred. Please retry later."), 400


@auth_bp.route('/change-password', methods=['POST'])
@jwt_required()
def change_password():

    if ("current" in request.json) and ("pass1" in request.json) and ("pass2" in request.json):
        current = request.json["current"]
        pass1 = request.json["pass1"]
        pass2 = request.json["pass2"]
        if not PASSWORD_REGEX.match(current) or not PASSWORD_REGEX.match(pass1):
            return jsonify(message="Invalid request. Password must be min. 8 characters."), 400
        if pass1 != pass2:
            return jsonify(message="Passwords dont match"), 400
        try:
            if not verify_password(current, current_user.password, current_user.salt):
                return jsonify(message="Your current password does not match"), 400
            hex_pass, hex_salt = hash_password(pass1)
            if current_user.set_new_password(hex_pass, hex_salt):
                return jsonify(message="Successfully changed your password!"), 200
            return jsonify(message="Something went wrong while updating your password. Please retry again later."), 500
        except Exception as e:
            print(f"An error occurred while trying to change password of user.id=={current_user.id}: " + str(e))

    return jsonify(message="An unknown error occurred. Please retry later."), 400

