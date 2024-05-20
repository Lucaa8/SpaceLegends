from flask import request, jsonify, url_for
from flask.blueprints import Blueprint
from utils import USERNAME_REGEX, DISPLAYNAME_REGEX, PASSWORD_REGEX, EMAIL_REGEX, hash_password, verify_password, generate_confirmation_code
from collection import Item, get_item
import chain

api_bp = Blueprint('api', __name__, template_folder='templates')


@api_bp.route('/token/<int:token_id>')
def get_metadata(token_id):
    if not chain.cosmic.is_valid():
        return jsonify({'error': 'API not initialized.'}), 500
    try:
        item_id = chain.cosmic.get_token_type(token_id)
        ownership_history = chain.cosmic.get_ownership_history(token_id)
        creation_date = chain.cosmic.get_token_creation_timestamp(token_id)
    except Exception as e:
        return jsonify({'error': str(e)}), 500
    item: Item = get_item(item_id)
    if item is None:
        return jsonify({'error': 'Item not found'}), 404
    meta = item.to_metadata()
    meta['id'] = token_id
    meta['attributes'].append({"trait_type": "Creation", "display_type": "date", "value": creation_date})
    meta['ownership_history'] = ownership_history
    meta['rarity'] = item.format_rarity()
    return jsonify(meta), 200


@api_bp.route('register', methods=['POST'])
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
        if (not wallet_address) or (not chain.cosmic.check_address(wallet_address)):
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
        return '', 204
    except Exception as e:
        return jsonify({'message': str(e)}), 400


@api_bp.route('/login', methods=['POST'])
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
                return '', 204 # Redirect ?
        except Exception as e:
            print(f"An unknown error occurred while checking password on login for user.id=={user.id}: {e}")
            error_msg = 'Something went wrong while checking your password. Try again later.'

    return jsonify({'message': error_msg}), 401


@api_bp.route('/verification/<code>', methods=['POST'])
def verification(code: str):
    from models import User
    if User.validate_email(code):
        return '', 204
    return '', 400
