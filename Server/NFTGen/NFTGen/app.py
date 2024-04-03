from flask import Flask
from web3 import Web3
import os
from dotenv import load_dotenv

load_dotenv()

app = Flask(__name__)
w3 = Web3(Web3.HTTPProvider(f'https://eth-sepolia.g.alchemy.com/v2/{os.getenv("ALCHEMY_API_KEY")}'))

assert w3.is_connected()


@app.route('/token/<int:tokenid>')
def generate_token(tokenid):
    return f'Token {tokenid}'


if __name__ == '__main__':
    app.run(host="0.0.0.0", port=80)
