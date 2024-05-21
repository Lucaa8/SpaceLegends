from flask.blueprints import Blueprint
from flask import request, jsonify, url_for
from utils import USERNAME_REGEX, DISPLAYNAME_REGEX, PASSWORD_REGEX, EMAIL_REGEX, hash_password, verify_password, generate_confirmation_code
from authentification import create_refresh, create_access

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
                return jsonify(refresh_token=create_refresh(user), access_token=create_access(user)), 200
        except Exception as e:
            print(f"An unknown error occurred while checking password on login for user.id=={user.id}: {str(e)}")
            error_msg = 'Something went wrong while checking your password. Try again later.'

    return jsonify({'message': error_msg}), 401
