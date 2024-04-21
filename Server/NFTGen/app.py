from flask import Flask, jsonify, render_template, url_for
from web3 import Web3
from web3.exceptions import ContractLogicError
import os
from dotenv import load_dotenv
from Collection import *
from blueprints.views import views_bp
from blueprints.api import api_bp

load_dotenv()

app = Flask(__name__)
app.register_blueprint(views_bp)
app.register_blueprint(api_bp, url_prefix='/api')

w3 = Web3(Web3.HTTPProvider(f'https://eth-sepolia.g.alchemy.com/v2/{os.getenv("ALCHEMY_API_KEY")}'))

assert w3.is_connected()
contract_address = w3.to_checksum_address('0x4e4741F0274e9d32372d1E677258Ad1cE88eEA25')
with open('contracts/crel.json', 'r', encoding='utf-8') as f:
    crel_abi = json.load(f)

if crel_abi is None:
    print("Unable to load de CosmicRelic ABI. The token endpoint won't be able to deliver metadata.")
    crel = None
else:
    crel = w3.eth.contract(address=contract_address, abi=crel_abi)


@app.errorhandler(404)
def page_not_found(e):
    return render_template('404.html'), 404


# TODO moves inside the api blueprint
@app.route('/token/<int:token_id>')
def get_metadata(token_id):
    if crel is None:
        return jsonify({'error': 'API not initialized.'}), 500
    try:
        item_id = crel.functions.getTokenType(token_id).call()
        ownership_history = crel.functions.getOwnershipHistory(token_id).call()
        creation_date = crel.functions.getTokenCreationTimestamp(token_id).call()
    except ContractLogicError as logic:
        return jsonify({'error':  logic.message}), 400
    except Exception:
        return jsonify({'error': 'Oops, something went wrong on our side. Please retry later'}), 500
    item: Item = get_item(item_id)
    if item is None:
        return jsonify({'error': 'Item not found'}), 404
    meta = item.to_metadata()
    meta['id'] = token_id
    meta['attributes'].append({"trait_type": "Creation", "display_type": "date", "value": creation_date})
    meta['ownership_history'] = ownership_history
    meta['rarity'] = item.format_rarity()
    return jsonify(meta), 200


if __name__ == '__main__':
    for file in os.listdir("data"):
        load(f"data/{file}")
    app.run(host="0.0.0.0", port=8082) # 80, 8080 and 8081 are already used on my web server.
