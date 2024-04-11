from flask import Flask
from flask import jsonify
from web3 import Web3
import os
from dotenv import load_dotenv

load_dotenv()

app = Flask(__name__)
w3 = Web3(Web3.HTTPProvider(f'https://eth-sepolia.g.alchemy.com/v2/{os.getenv("ALCHEMY_API_KEY")}'))
baseURL = "http://localhost/"

assert w3.is_connected()


@app.route('/token/<int:token_id>')
def generate_token(token_id):
    coll = 1256
    row = 254
    col = 12
    rarity = 7
    encode = (coll << 27) | (row << 11) | (col << 3) | rarity
    print((encode >> 27) & 0xFFF, (encode >> 11) & 0xFF, (encode >> 3) & 0xFF, encode & 0x7)
    return "Token ID {}".format(token_id)


if __name__ == '__main__':
    app.run(host="0.0.0.0", port=80)
