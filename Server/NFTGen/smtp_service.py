import smtplib
from email.mime.multipart import MIMEMultipart
from email.mime.text import MIMEText
import os


class SMTPServer:
    def __init__(self, address: str, password: str, outgoing_server_host: str, outgoing_server_port: int):
        self.address = address
        self._password = password
        self._hostname = outgoing_server_host
        self._port = outgoing_server_port

    def _connect(self) -> smtplib.SMTP_SSL:
        server = smtplib.SMTP_SSL(self._hostname, self._port, timeout=3)
        server.login(self.address, self._password)
        return server

    def send_email(self, to: str, subject: str, body: str) -> bool:
        message = MIMEMultipart()
        message["From"] = self.address
        message["To"] = to
        message["Subject"] = subject
        message.attach(MIMEText(body, "plain"))
        try:
            server = self._connect()
            text = message.as_string()
            server.sendmail(self.address, to, text)
            server.quit()
            return True
        except Exception as e:
            print(f"Failed to send email: {e}")
            return False

    def send_verification_email(self, username: str, to: str, verification_url: str) -> bool:
        sub = "Please Verify Your Email Address"
        body = f"""
        Hello {username},

        Thank you for registering. Please click the link below to verify your email address:
        {verification_url}

        Best regards,
        The Space Legends Team
        """
        return self.send_email(to, sub, body)


smtp_service: SMTPServer | None = None


def load():
    global smtp_service
    smtp_service = SMTPServer(os.getenv("SMTP_ADDRESS"),
                              os.getenv("SMTP_PASSWORD"),
                              os.getenv("SMTP_SERVER"),
                              int(os.getenv("SMTP_PORT")))
