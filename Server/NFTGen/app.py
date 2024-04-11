from flask import Flask
from flask import jsonify
from web3 import Web3
import os
from dotenv import load_dotenv
from Collection import *

load_dotenv()

app = Flask(__name__)
w3 = Web3(Web3.HTTPProvider(f'https://eth-sepolia.g.alchemy.com/v2/{os.getenv("ALCHEMY_API_KEY")}'))
baseURL = "http://localhost/"

assert w3.is_connected()


@app.route('/token/<int:token_id>')
def generate_token(token_id):
    c: Collection = collections[0]
    i: Item = items[2]
    # rechercher le token type dans le smartcontract du tokenId passé en query, utiliser Collections#get_item puis retourner Item#to_metadata
    return jsonify(i.to_metadata()), 200


if __name__ == '__main__':
    for file in os.listdir("data"):
        load(f"data/{file}")
    app.run(host="0.0.0.0", port=80)
