import re
import os
import hashlib
import secrets
import string
import random
import base64
from flask import session, redirect, url_for
from functools import wraps
from cryptography.hazmat.primitives.ciphers import Cipher, algorithms, modes
from cryptography.hazmat.backends import default_backend
from eth_account import Account


USERNAME_REGEX = re.compile(r'^[a-z0-9_]{3,16}$')
DISPLAYNAME_REGEX = re.compile(r'^[a-zA-ZÀ-Ÿ\-_. 0-9]{3,24}$')
EMAIL_REGEX = re.compile(r'^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$')
PASSWORD_REGEX = re.compile(r'^.{8,}$')

PROFILE_PIC_FOLDER: str | None = None
PROFILE_PIC_MAX_MB: str | None = None


# Decorator @user_session to use on flask routes. It will check if a valid user is in the session and add it to the context. Otherwise, redirect on the login page
def user_session():
    def decorator(f):
        @wraps(f)
        def decorated_function(*args, **kwargs):
            user = get_user_from_session()
            if user is None:
                return redirect(url_for('views.login'))
            return f(user, *args, **kwargs)
        return decorated_function
    return decorator


def get_user_from_session():
    if "user_id" in session:
        from models.User import User
        return User.get_user_by_id(session.get("user_id"))


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


def generate_confirmation_code(length: int = 36) -> str:
    """Generate a confirmation code for email addresses."""
    alphabet = string.ascii_letters + string.digits
    return ''.join(secrets.choice(alphabet) for _ in range(length))


def generate_password(length=12, include_uppercase=True, include_numbers=True):
    lower = string.ascii_lowercase
    upper = string.ascii_uppercase if include_uppercase else ''
    digits = string.digits if include_numbers else ''

    all_chars = lower + upper + digits

    password = [
        random.choice(lower),
        random.choice(upper) if include_uppercase else '',
        random.choice(digits) if include_numbers else '',
    ]

    password += random.choices(all_chars, k=length - len(password))
    random.shuffle(password)
    return ''.join(password)


def save_profile_pic(file, user_id) -> str | None:
    delete_profile_pic(user_id)
    file_len = file.seek(0, 2) # Check until the eof (2 is os.SEEK_END) to get the size in bytes
    if file_len / (1024 * 1024) < PROFILE_PIC_MAX_MB:
        file.seek(0, 0) # Replace the cursor at the start (0 is os.SEEK_SET) otherwise the save won't work
        path = f"static/{PROFILE_PIC_FOLDER}/{user_id}.{'jpeg' if file.mimetype == 'image/jpeg' else 'png'}"
        file.save(path)
        return path
    return None


def get_profile_pic(user_id, default_on_fail=True) -> str | None:
    as_jpeg = f"static/{PROFILE_PIC_FOLDER}/{user_id}.jpeg"
    if os.path.isfile(as_jpeg):
        return as_jpeg
    as_png = f"static/{PROFILE_PIC_FOLDER}/{user_id}.png"
    if os.path.isfile(as_png):
        return as_png
    return url_for('static', filename='files/default_pp.png')[1:] if default_on_fail else None


def delete_profile_pic(user_id) -> bool:
    pic = get_profile_pic(user_id, default_on_fail=False)
    if pic is not None and os.path.exists(pic):
        os.remove(pic)
        return True
    return False


def create_new_wallet() -> tuple[str, str]:
    new_account = Account.create()
    private_key = new_account.key.hex()
    address = new_account.address
    return address, encrypt_wallet_key(private_key)


def encrypt_wallet_key(private_key: str) -> str:
    iv = os.urandom(16)
    cipher = Cipher(algorithms.AES(base64.b64decode(os.getenv('AES_ENCRYPTION_KEY'))), modes.CFB(iv), backend=default_backend())
    encryptor = cipher.encryptor()
    encrypted_key = encryptor.update(private_key.encode()) + encryptor.finalize()
    return base64.b64encode(iv + encrypted_key).decode()


def decrypt_wallet_key(encrypted_key: str) -> str:
    encrypted_key = base64.b64decode(encrypted_key)
    iv = encrypted_key[:16]
    encrypted_key = encrypted_key[16:]
    cipher = Cipher(algorithms.AES(base64.b64decode(os.getenv('AES_ENCRYPTION_KEY'))), modes.CFB(iv), backend=default_backend())
    decryptor = cipher.decryptor()
    private_key = decryptor.update(encrypted_key) + decryptor.finalize()
    return private_key.decode()
