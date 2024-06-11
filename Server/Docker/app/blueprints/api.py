from flask import jsonify, redirect, url_for, request
from flask.blueprints import Blueprint
from flask_jwt_extended import jwt_required, current_user
from collection import Item, get_item, collections
import chain
from utils import save_profile_pic, delete_profile_pic

api_bp = Blueprint('api', __name__, template_folder='templates')


@api_bp.route('/token/<int:token_id>')
def get_metadata(token_id):
    if not chain.cosmic.is_valid():
        return jsonify({'error': 'API not initialized.'}), 500
    from models.NFT import NFT
    if not NFT.is_token_minted(token_id):
        return jsonify({'error': 'Item not found'}), 404
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


@api_bp.route('/change-displayname', methods=['POST'])
@jwt_required()
def change_displayname():
    if "display_name" in request.json:
        display_name = request.json["display_name"]
        from utils import DISPLAYNAME_REGEX
        if (len(display_name) > 0) and (DISPLAYNAME_REGEX.match(display_name)):
            if display_name == current_user.display_name:
                return jsonify(message="It's already your display name!"), 400
            if current_user.set_new_display_name(display_name):
                return jsonify(message="Successfully changed your display name!"), 200
            return jsonify(message="An unexpected error occurred while changing your display name. Please retry later."), 500
    return jsonify(message="Invalid display_name"), 400


@api_bp.route('/change-picture', methods=['POST'])
@jwt_required()
def change_picture():
    if "file" in request.files:
        file = request.files["file"]
        try:
            path = save_profile_pic(file, current_user.id)
            if path is None:
                return jsonify(message="File too big. Max. is 5 MB"), 400
            return jsonify(message="Profile picture updated successfully!", path=path), 200
        except Exception as e:
            print(f'An unknown error occurred while saving the new profile picture of user.id=={current_user.id}: {str(e)}')
    return jsonify(message="Something went wrong while updating your profile picture please retry later."), 400


@api_bp.route('/delete-picture', methods=['DELETE'])
@jwt_required()
def delete_picture():
    try:
        if delete_profile_pic(current_user.id):
            return jsonify(message="Profile picture deleted successfully.", path=url_for('static', filename='files/default_pp.png')), 200
        return jsonify(message="You have no profile picture"), 404
    except Exception as e:
        print(f'An unknown error occurred while deleting the profile picture of user.id=={current_user.id}: {str(e)}')
    return jsonify(message="Something went wrong while deleting your profile picture please retry later."), 400


@api_bp.route('/user', methods=['GET'])
@jwt_required()
def get_resources():
    from models.User import User
    from models.NFT import NFT
    from models.GameLevel import GameLevel
    from models.UserProgress import UserProgress
    user: User = current_user

    progression = {}
    for level in GameLevel.get_levels():
        level_json = level.as_json()
        progress = UserProgress.get_progress(user.id, level.id)
        if progress is not None:
            level_json['progress'] = progress.as_json()
        progression[level.id] = level_json

    # This name is misleading as the dict will contain how many piece this user got on the given collection
    # Just check if completed_collections[collection] == 9 to tell if is_collection_complete
    completed_collections = {}
    for collection in collections:
        completed_collections[collection.collection_id] = NFT.is_collection_complete(user.id, collection.collection_id)

    result = {
        'username': user.username,
        'display_name': user.display_name,
        'experience': user.get_level_info(),
        'nft': {nft_type: count for nft_type, count in NFT.get_nft_count_by_type(user.id)},
        'relics': NFT.get_unminted_nft_by_collections(user.id),
        'completed_collections': completed_collections,
        'resources': {
            'sdt': user.money_sdt,
            'heart': user.money_heart,
            'eth': chain.cosmic.get_balance_eth(user.wallet_address),
            'perks': user.get_active_perks()
        },
        'levels': progression
    }

    return jsonify(result), 200
