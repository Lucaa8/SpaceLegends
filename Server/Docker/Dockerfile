FROM python:3.12-bookworm

RUN apt-get update && apt-get install -y \
    build-essential \
    libssl-dev \
    libffi-dev \
    mariadb-client \
    && apt-get clean

WORKDIR /app

COPY ./app /app
RUN pip install --upgrade pip
RUN pip install -r requirements.txt

CMD ["python", "app.py"]

EXPOSE 8081
