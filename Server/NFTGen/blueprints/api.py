from typing import Tuple

import flask
from flask import jsonify, redirect, url_for, request, Response
from flask.blueprints import Blueprint
from flask_jwt_extended import jwt_required, current_user
from collection import Item, get_item, collections
import chain
from utils import save_profile_pic, delete_profile_pic
from threading import Thread

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


@api_bp.route('/open-relic/<int:collection>', methods=['POST'])
@jwt_required()
def open_relic(collection: int):
    from models.NFT import NFT
    from collection import get_item
    from models.CRELProbabilty import CRELPropability
    nft: NFT = NFT.get_first_unminted_nft(current_user.id, collection)
    if nft is not None:
        wallet = current_user.wallet_address
        username = current_user.username
        Thread(target=nft.mint, args=(nft.id, wallet, username,)).start()
        item = get_item(nft.type)
        return jsonify(name=f"{item.name} (Row {item.row} | Col {item.col})",
                       type=item.item_id,
                       # Send the level probabilities where the relic was dropped. If the relic was received by offer or whatever, no custom probabilities and the client handle the default ones
                       probabilities=(CRELPropability.get_probabilities(nft.dropped_by_level_id).as_json() if nft.dropped_by_level_id is not None else []),
                       rarity=item.rarity,
                       image=f"{item.collection.get_collection_id()}_{item.collection.name}_r{str(item.row).zfill(2)}c{str(item.col).zfill(2)}"), 200
    return jsonify(message="No valid relics to open"), 400


@api_bp.route('/lives', methods=['GET', 'POST'])
@jwt_required()
def user_lives():
    from models.User import User
    user: User = current_user
    # GET LIVES
    if request.method == 'GET':
        return jsonify(count=user.money_heart), 200
    # DECREASE LIVES (POST)
    if user.money_heart == 0:
        return jsonify(message="No money heart"), 403
    if not ("count" in request.json):
        return jsonify(message="You must provide the value"), 400
    try:
        count = int(request.json["count"])
        new_value: int = user.decrease_lives_count(count)
        if new_value >= 0:
            return '', 204
        return jsonify(message="An unknown error occurred."), 500
    except ValueError:
        return jsonify(message="The value must be integer"), 400


@api_bp.route('/start-level/<int:level_id>', methods=['PUT'])
@jwt_required()
def start_level(level_id: int):
    from models.LiveGame import LiveGame
    from models.UserProgress import UserProgress
    from models.GameLevel import GameLevel
    code: str = LiveGame.start(current_user, level_id)
    if code is None:
        return jsonify(message="Invalid level"), 400
    try:
        progress: UserProgress = UserProgress.get_progress(current_user.id, level_id, create=True)
        stars = progress.as_json()["stars"]
        progress.total_games += 1
        progress.update()
        level = GameLevel.get(level_id)
        if level is not None:
            level = f"Level {level.level}"
        return jsonify(code=code, lives=current_user.money_heart, stars=stars, level=level), 200
    except Exception as e:
        print(f"Something went wrong while getting level information for user.id=={current_user.id} and level_id=={level_id}: {str(e)}")
        return jsonify(message="Error"), 500


@api_bp.route('/stop-level', methods=['DELETE'])
@jwt_required()
def stop_level():
    if not ("code" in request.json) or not ("completed" in request.json):
        return jsonify(message="Invalid request"), 400

    from models.LiveGame import LiveGame
    game: LiveGame = LiveGame.get(request.json["code"])
    if game is None or game.has_ended():
        return jsonify(message="This game is invalid or has already ended"), 400

    if not game.finish(request.json["completed"]):
        return jsonify(message="Something went wrong while validating your session"), 500

    if not game.completed: # Used left the level without ending it
        return '', 204

    time_spent = (game.finished_at - game.started_at).total_seconds()

    from models.UserProgress import UserProgress
    progress: UserProgress = UserProgress.get_progress(current_user.id, game.level_id)
    progress.total_completions += 1

    # Update Stars only if level has been completed
    if "stars" in request.json:
        progress.star_1 = request.json["stars"]["star_1"]
        progress.star_2 = request.json["stars"]["star_2"]
        progress.star_3 = request.json["stars"]["star_3"]

    progress.update() # As I'm potentially inserting NFTs later on, I cant keep those unchanged fields to wait for relics_found to be changed and then update it.

    from models.GameLevel import GameLevel
    level: GameLevel = GameLevel.get(game.level_id)

    from models.User import User
    user = User.get_user_by_id(current_user.id)
    user.give_exp(level.get_exp())
    if "SDT" in request.json:
        user.set_sdt_money(user.money_sdt + float(request.json["SDT"]))

    reward = level.generate_reward()

    # Apply the reward to user
    if reward[0] == 'RELIC':
        from models.NFT import NFT
        NFT.create(reward[1], current_user.id, level.id)
        progress.relics_found += 1
        progress.update()
        return jsonify(reward={'type': 'RELIC', 'value': reward[1].collection.name}, time=time_spent), 200
    elif reward[0] == 'HEART' or reward[0] == 'SDT':
        user = User.get_user_by_id(current_user.id)
        if reward[0] == 'HEART':
            user.increase_lives_count(reward[1])
        else:
            user.set_sdt_money(user.money_sdt + reward[1])

    # Return reward info to client
    return jsonify(reward={'type': reward[0], 'value': reward[1]}, time=time_spent), 200


@api_bp.route('/kills', methods=['POST'])
@jwt_required()
def increment_kills():
    if not ("code" in request.json):
        return jsonify(message="Invalid request"), 400

    from models.LiveGame import LiveGame
    game: LiveGame = LiveGame.get(request.json["code"])
    if game is None or game.has_ended():
        return jsonify(message="This game is invalid or has already ended"), 400

    try:
        from models.UserProgress import UserProgress
        progress: UserProgress = UserProgress.get_progress(current_user.id, game.level_id)
        progress.kills += 1
        progress.update()
        return '', 204
    except Exception as e:
        print(f"Something went wrong while updating kills in user progress of user.id=={current_user.id} and level_id=={game.level_id}: {str(e)}")
        return jsonify(message="Failed to update kills"), 400


@api_bp.route('/deaths', methods=['POST'])
@jwt_required()
def increment_deaths():
    if not ("code" in request.json):
        return jsonify(message="Invalid request"), 400

    from models.LiveGame import LiveGame
    game: LiveGame = LiveGame.get(request.json["code"])
    if game is None or game.has_ended():
        return jsonify(message="This game is invalid or has already ended"), 400

    try:
        from models.UserProgress import UserProgress
        progress: UserProgress = UserProgress.get_progress(current_user.id, game.level_id)
        progress.deaths += 1
        progress.update()
        return '', 204
    except Exception as e:
        print(f"Something went wrong while updating deaths in user progress of user.id=={current_user.id} and level_id=={game.level_id}: {str(e)}")
        return jsonify(message="Failed to deaths kills"), 400


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


@api_bp.route('/list-nft/<int:nft_id>', methods=['POST'])
@jwt_required()
def list_nft(nft_id: int):
    if not ("price" in request.json):
        return jsonify(message="Missing price"), 400
    try:
        price = float(request.json["price"])
        if price < 0.1 or price >= 5000.0:
            raise ValueError("")
    except ValueError:
        return jsonify(message="Price must be a decimal number between 0.1 (inclusive) and 5000.0 (exclusive)"), 400
    from models.NFT import NFT
    if NFT.list_on_market(current_user.id, nft_id, price):
        return '', 204
    return jsonify(message="You must have a minimum of 2 *MINTED* and *UNLISTED* NFTs of the same type to be able to list one of them."), 400


@api_bp.route('/buy-nft/<int:nft_id>', methods=['POST'])
def buy_nft(nft_id: int):
    return '', 204
