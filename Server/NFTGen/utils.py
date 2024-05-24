import re
import os
import hashlib
import secrets
import string
import random
from flask import session, redirect, url_for
from functools import wraps


USERNAME_REGEX = re.compile(r'^[a-z0-9_]{3,16}$')
DISPLAYNAME_REGEX = re.compile(r'^[a-zA-ZÀ-Ÿ\-_. 0-9]{3,24}$')
EMAIL_REGEX = re.compile(r'^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$')
PASSWORD_REGEX = re.compile(r'^.{8,}$')


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

