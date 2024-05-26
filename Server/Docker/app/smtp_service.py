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

    def send_email(self, to: str, subject: str, html_body: str) -> bool:
        message = MIMEMultipart()
        message["From"] = self.address
        message["To"] = to
        message["Subject"] = subject
        message.attach(MIMEText(html_body, "html"))
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
        <html>
            <body>
                <h3>Hello {username},</h3>
                <p>Thank you for registering. Please click the button below to verify your email address.</p>
                <a href="{verification_url}" style="display: inline-block; padding: 10px 20px; font-size: 16px; color: white; background-color: #007BFF; text-align: center; text-decoration: none; border-radius: 5px;">Verify</a>
                <p>Best regards,<br>The Space Legends Team</p>
            </body>
        </html>
        """
        return self.send_email(to, sub, body)

    def send_new_password_email(self, user, new_password) -> bool:
        sub = f"Your new password for account {user.username}"
        body = f"""
        <html>
            <body>
                <h3>Hello {user.display_name},</h3>
                <p>You made a request about a lost password. We'll send you a new one so you can login and change it in the edit profile page.</p>
                <p>Your new password is: {new_password}</p>
                <a href="https://space-legends.luca-dc.ch/login" style="display: inline-block; padding: 10px 20px; font-size: 16px; color: white; background-color: #007BFF; text-align: center; text-decoration: none; border-radius: 5px;">Login</a>
                <p>Best regards,<br>The Space Legends Team</p>
            </body>
        </html>
        """
        return self.send_email(user.email, sub, body)


smtp_service: SMTPServer | None = None


def load():
    global smtp_service
    smtp_service = SMTPServer(os.getenv("SMTP_ADDRESS"),
                              os.getenv("SMTP_PASSWORD"),
                              os.getenv("SMTP_SERVER"),
                              int(os.getenv("SMTP_PORT")))
