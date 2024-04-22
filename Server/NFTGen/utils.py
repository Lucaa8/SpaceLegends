import re


USERNAME_REGEX = re.compile(r'^[a-z0-9_]{3,16}$')
DISPLAYNAME_REGEX = re.compile(r'^[a-zA-ZÀ-Ÿ\-_. 0-9]{3,24}$')
EMAIL_REGEX = re.compile(r'^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$')
PASSWORD_REGEX = re.compile(r'^.{8,}$')

