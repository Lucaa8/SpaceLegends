import re
import os
import hashlib
import secrets
import string


USERNAME_REGEX = re.compile(r'^[a-z0-9_]{3,16}$')
DISPLAYNAME_REGEX = re.compile(r'^[a-zA-ZÀ-Ÿ\-_. 0-9]{3,24}$')
EMAIL_REGEX = re.compile(r'^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$')
PASSWORD_REGEX = re.compile(r'^.{8,}$')


def hash_password(password: str) -> tuple[str, str]:
    salt = os.urandom(16)
    return hashlib.scrypt(
        password.encode(),
        salt=salt,
        n=16384,  # CPU Cost (N)
        r=8,  # Memory Cost (r)
        p=1,  # Parallel Setting (p)
        dklen=64  # Desired hash length (bytes)
    ).hex(), salt.hex()


def verify_password(password: str, hex_hashed_password: str, hex_salt: str) -> bool:
    salt = bytes.fromhex(hex_salt)
    hashed_password = hashlib.scrypt(password.encode(), salt=salt, n=16384, r=8, p=1, dklen=64)
    return hashed_password.hex() == hex_hashed_password


def generate_confirmation_code(length: int = 12) -> str:
    """Generate a confirmation code for email addresses."""
    alphabet = string.ascii_letters + string.digits
    return ''.join(secrets.choice(alphabet) for _ in range(length))

