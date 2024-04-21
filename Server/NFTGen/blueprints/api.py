from flask import request, jsonify
from flask.blueprints import Blueprint
from utils import USERNAME_REGEX, DISPLAYNAME_REGEX, PASSWORD_REGEX, EMAIL_REGEX, check_address

api_bp = Blueprint('api', __name__, template_folder='templates')


@api_bp.route('register', methods=['POST'])
def register():
    username = request.form.get('username')
    display_name = request.form.get('displayname')
    email = request.form.get('email')
    password = request.form.get('pass1')
    confirm_password = request.form.get('pass2')
    wallet_address = request.form.get('walletAddress')

    errors = {}
    try:
        if (not username) or (not USERNAME_REGEX.match(username)):
            errors['username'] = 'Invalid or missing username.'
        if display_name and (len(display_name) > 0) and (not DISPLAYNAME_REGEX.match(display_name)):
            errors['display_name'] = 'Invalid or missing display name.'
        if (not email) or (not EMAIL_REGEX.match(email)):
            errors['email'] = 'Invalid or missing email address.'
        if (not password) or (not PASSWORD_REGEX.match(password)):
            errors['password'] = 'Password must be at least 8 characters long.'
        if password != confirm_password:
            errors['confirm_password'] = 'Passwords do not match.'
        if (not wallet_address) or (not check_address(wallet_address)):
            errors['wallet_address'] = 'Invalid or missing wallet address.'
    except Exception as e:
        errors['other'] = str(e)

    if errors:
        return jsonify({'message': errors}), 400

    if (not display_name) or (len(display_name) == 0): # If the user did not specify a display name, his username will be his display name
        display_name = username

    # DB register

    return jsonify({}), 204
