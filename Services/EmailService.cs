using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MielShop.API.Models;
using MimeKit;

namespace MielShop.API.Services
{
    public interface IEmailService
    {
        Task SendEmailVerificationAsync(string toEmail, string userName, string verificationToken);
        Task SendPasswordResetAsync(string toEmail, string userName, string resetToken);
        Task SendWelcomeEmailAsync(string toEmail, string userName);
    }

    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly string _frontendUrl;

        public EmailService(IOptions<EmailSettings> emailSettings, IOptions<AppSettings> appSettings)
        {
            _emailSettings = emailSettings.Value;
            _frontendUrl = appSettings.Value.FrontendUrl;
        }

        public async Task SendEmailVerificationAsync(string toEmail, string userName, string verificationToken)
        {
            var verificationUrl = $"{_frontendUrl}/verify-email?token={verificationToken}";

            var subject = "Vérifiez votre adresse email - MielShop";
            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #f8b500; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f9f9f9; padding: 30px; }}
        .button {{ 
            display: inline-block; 
            padding: 12px 30px; 
            background-color: #f8b500; 
            color: white; 
            text-decoration: none; 
            border-radius: 5px; 
            margin: 20px 0;
        }}
        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🍯 Bienvenue sur MielShop!</h1>
        </div>
        <div class='content'>
            <h2>Bonjour {userName},</h2>
            <p>Merci de vous être inscrit sur MielShop!</p>
            <p>Pour finaliser votre inscription, veuillez vérifier votre adresse email en cliquant sur le bouton ci-dessous:</p>
            <p style='text-align: center;'>
                <a href='{verificationUrl}' class='button'>Vérifier mon email</a>
            </p>
            <p>Si le bouton ne fonctionne pas, copiez et collez ce lien dans votre navigateur:</p>
            <p style='word-break: break-all; color: #666;'>{verificationUrl}</p>
            <p><strong>Ce lien expirera dans 24 heures.</strong></p>
            <p>Si vous n'avez pas créé de compte, ignorez cet email.</p>
        </div>
        <div class='footer'>
            <p>&copy; 2024 MielShop. Tous droits réservés.</p>
        </div>
    </div>
</body>
</html>";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendPasswordResetAsync(string toEmail, string userName, string resetToken)
        {
            var resetUrl = $"{_frontendUrl}/reset-password?token={resetToken}";

            var subject = "Réinitialisation de votre mot de passe - MielShop";
            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #f8b500; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f9f9f9; padding: 30px; }}
        .button {{ 
            display: inline-block; 
            padding: 12px 30px; 
            background-color: #f8b500; 
            color: white; 
            text-decoration: none; 
            border-radius: 5px; 
            margin: 20px 0;
        }}
        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔐 Réinitialisation du mot de passe</h1>
        </div>
        <div class='content'>
            <h2>Bonjour {userName},</h2>
            <p>Vous avez demandé à réinitialiser votre mot de passe.</p>
            <p>Cliquez sur le bouton ci-dessous pour créer un nouveau mot de passe:</p>
            <p style='text-align: center;'>
                <a href='{resetUrl}' class='button'>Réinitialiser mon mot de passe</a>
            </p>
            <p>Si le bouton ne fonctionne pas, copiez et collez ce lien dans votre navigateur:</p>
            <p style='word-break: break-all; color: #666;'>{resetUrl}</p>
            <p><strong>Ce lien expirera dans 1 heure.</strong></p>
            <p>Si vous n'avez pas demandé cette réinitialisation, ignorez cet email.</p>
        </div>
        <div class='footer'>
            <p>&copy; 2024 MielShop. Tous droits réservés.</p>
        </div>
    </div>
</body>
</html>";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendWelcomeEmailAsync(string toEmail, string userName)
        {
            var subject = "Bienvenue sur MielShop! 🍯";
            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #f8b500; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f9f9f9; padding: 30px; }}
        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🍯 Bienvenue sur MielShop!</h1>
        </div>
        <div class='content'>
            <h2>Félicitations {userName}!</h2>
            <p>Votre email a été vérifié avec succès.</p>
            <p>Vous pouvez maintenant profiter de tous nos produits de miel authentique.</p>
            <p>Commencez dès maintenant à explorer notre catalogue!</p>
        </div>
        <div class='footer'>
            <p>&copy; 2024 MielShop. Tous droits réservés.</p>
        </div>
    </div>
</body>
</html>";

            await SendEmailAsync(toEmail, subject, body);
        }

        private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            try
            {
                Console.WriteLine($"📧 Attempting to send email to {toEmail}");
                Console.WriteLine($"🔧 SMTP Settings:");
                Console.WriteLine($"   Server: {_emailSettings.SmtpServer}");
                Console.WriteLine($"   Port: {_emailSettings.SmtpPort}");
                Console.WriteLine($"   EnableSsl: {_emailSettings.EnableSsl}");
                Console.WriteLine($"   Username: {_emailSettings.Username}");
                Console.WriteLine($"   Password: {(_emailSettings.Password?.Length > 0 ? "****" : "NOT SET")}");
                Console.WriteLine($"   Frontend URL: {_frontendUrl}");

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = htmlBody
                };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();

                // Set timeout to 30 seconds
                client.Timeout = 30000;

                Console.WriteLine($"🔌 Connecting to SMTP server...");

                // Determine the correct SecureSocketOptions based on port
                // Port 587 = StartTls (TLS upgrade after connection)
                // Port 465 = SslOnConnect (SSL from the start)
                // Port 25 = None (usually blocked by cloud providers)
                SecureSocketOptions secureSocketOption;

                if (_emailSettings.SmtpPort == 465)
                {
                    secureSocketOption = SecureSocketOptions.SslOnConnect;
                    Console.WriteLine($"   Using SSL on Connect (Port 465)");
                }
                else if (_emailSettings.SmtpPort == 587)
                {
                    secureSocketOption = SecureSocketOptions.StartTls;
                    Console.WriteLine($"   Using StartTLS (Port 587)");
                }
                else
                {
                    secureSocketOption = _emailSettings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
                    Console.WriteLine($"   Using {secureSocketOption}");
                }

                await client.ConnectAsync(
                    _emailSettings.SmtpServer,
                    _emailSettings.SmtpPort,
                    secureSocketOption
                );

                Console.WriteLine($"✅ Connected to SMTP server");
                Console.WriteLine($"🔐 Authenticating...");

                await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);

                Console.WriteLine($"✅ Authenticated successfully");
                Console.WriteLine($"📤 Sending email...");

                await client.SendAsync(message);

                Console.WriteLine($"✅ Email sent successfully to {toEmail}");

                await client.DisconnectAsync(true);
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                Console.WriteLine($"❌ Socket Error: {ex.Message}");
                Console.WriteLine($"   This usually means network/firewall issues or wrong port");
                Console.WriteLine($"   Error Code: {ex.SocketErrorCode}");
                Console.WriteLine($"   Stack Trace: {ex.StackTrace}");
                throw new Exception($"Network error connecting to SMTP server: {ex.Message}", ex);
            }
            catch (System.TimeoutException ex)
            {
                Console.WriteLine($"❌ Timeout Error: {ex.Message}");
                Console.WriteLine($"   SMTP server didn't respond in time");
                Console.WriteLine($"   Check if port {_emailSettings.SmtpPort} is accessible from Render");
                Console.WriteLine($"   Stack Trace: {ex.StackTrace}");
                throw new Exception($"Timeout connecting to SMTP server on port {_emailSettings.SmtpPort}", ex);
            }
            catch (MailKit.Security.AuthenticationException ex)
            {
                Console.WriteLine($"❌ Authentication Error: {ex.Message}");
                Console.WriteLine($"   Gmail App Password might be invalid or expired");
                Console.WriteLine($"   Generate new App Password at: https://myaccount.google.com/apppasswords");
                Console.WriteLine($"   Stack Trace: {ex.StackTrace}");
                throw new Exception($"Authentication failed. Check Gmail App Password.", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error sending email to {toEmail}: {ex.Message}");
                Console.WriteLine($"   Exception Type: {ex.GetType().Name}");
                Console.WriteLine($"   Stack Trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"   Inner Exception: {ex.InnerException.Message}");
                    Console.WriteLine($"   Inner Stack Trace: {ex.InnerException.StackTrace}");
                }
                throw;
            }
        }
    }
}