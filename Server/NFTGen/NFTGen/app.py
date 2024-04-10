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

@app.route('/token/<int:tokenid>')
def generate_token(tokenid):
    metadata = {
        "name": f"Earth Piece 1",
        "description": f"This is piece 1 of the Earth collection.",
        "image": f"https://bafybeie4n7ygwhczxutgdtkjvr4mmdbbwdgejf6jrwz3d47xonhir6lnja.ipfs.w3s.link/00_Earth_r1c1.png",
        "attributes": [
            {"trait_type": "Collection", "value": "test"},
            {"trait_type": "Piece Index", "value": "test"}
        ]
    }
    return jsonify(metadata)


if __name__ == '__main__':
    app.run(host="0.0.0.0", port=80)
