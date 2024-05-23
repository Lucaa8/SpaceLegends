from flask import jsonify, redirect, url_for
from flask.blueprints import Blueprint
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


@api_bp.route('/verification/<code>', methods=['GET'])
def verification(code: str):
    from models import User
    if User.validate_email(code):
        return redirect(url_for('views.own_profile'))
    return '', 400
