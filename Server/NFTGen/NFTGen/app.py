from flask import Flask
from web3 import Web3
import os
import json
from dotenv import load_dotenv

load_dotenv()

app = Flask(__name__)
w3 = Web3(Web3.HTTPProvider(f'https://eth-sepolia.g.alchemy.com/v2/{os.getenv("ALCHEMY_API_KEY")}'))

assert w3.is_connected()

# Create encode and decode function/routes

# encoded_id = (collec << 16) | (row << 8) | col

# collection_id_decoded = (encoded_id >> 16) & 0xFF
# row_decoded = (encoded_id >> 8) & 0xFF
# col_decoded = encoded_id & 0xFF

@app.route('/token/<int:tokenid>')
def generate_token(tokenid):
    return json.dumps({'tokenid': tokenid}, indent=4)


if __name__ == '__main__':
    app.run(host="0.0.0.0", port=80)
