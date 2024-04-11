import json

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
    return "Token ID {}".format(token_id)


if __name__ == '__main__':
    app.run(host="0.0.0.0", port=80)
