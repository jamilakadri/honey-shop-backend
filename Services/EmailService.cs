using Resend;
using Microsoft.Extensions.Options;
using MielShop.API.Models;

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
        private readonly ILogger<EmailService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public EmailService(
            IOptions<EmailSettings> emailSettings,
            IOptions<AppSettings> appSettings,
            ILogger<EmailService> logger,
            IHttpClientFactory httpClientFactory)
        {
            _emailSettings = emailSettings.Value;
            _frontendUrl = appSettings.Value.FrontendUrl;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
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
            <h1>🍯 Bienvenue sur Miel De Aoussou!</h1>
        </div>
        <div class='content'>
            <h2>Félicitations {userName}!</h2>
            <p>Votre email a été vérifié avec succès.</p>
            <p>Vous pouvez maintenant profiter de tous nos produits de miel authentique.</p>
            <p>Commencez dès maintenant à explorer notre catalogue!</p>
        </div>
        <div class='footer'>
            <p>&copy; 2025 Miel De Aoussou. Tous droits réservés.</p>
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
                _logger.LogInformation($"📧 Attempting to send email to {toEmail}");
                _logger.LogInformation($"🔧 Resend Settings:");
                _logger.LogInformation($"   API Key: {(_emailSettings.ResendApiKey?.Length > 0 ? "SET (hidden)" : "NOT SET")}");
                _logger.LogInformation($"   From Email: {_emailSettings.SenderEmail}");
                _logger.LogInformation($"   From Name: {_emailSettings.SenderName}");

                if (string.IsNullOrEmpty(_emailSettings.ResendApiKey))
                {
                    _logger.LogError("❌ Resend API key is not configured");
                    throw new InvalidOperationException("Resend API key is missing. Please set EmailSettings:ResendApiKey");
                }

                // Create HttpClient
                var httpClient = _httpClientFactory.CreateClient();

                // Create Resend options
                var resendOptions = new ResendClientOptions
                {
                    ApiToken = _emailSettings.ResendApiKey
                };

                // Create options wrapper
                var optionsWrapper = new ResendOptionsWrapper(resendOptions);

                // Create Resend client
                var resend = new ResendClient(optionsWrapper, httpClient);

                // Create email message
                var message = new EmailMessage
                {
                    From = $"{_emailSettings.SenderName} <{_emailSettings.SenderEmail}>",
                    To = new[] { toEmail },
                    Subject = subject,
                    HtmlBody = htmlBody
                };

                _logger.LogInformation($"📤 Sending email via Resend...");

                // Send email
                var response = await resend.EmailSendAsync(message);

                if (response != null && response.Content != Guid.Empty)
                {
                    _logger.LogInformation($"✅ Email sent successfully to {toEmail}");
                    _logger.LogInformation($"   Message ID: {response.Content}");
                }
                else
                {
                    _logger.LogError($"❌ Failed to send email - no response or empty ID");
                    throw new Exception("Failed to send email via Resend");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error sending email to {toEmail}: {ex.Message}");

                if (ex.Message.Contains("API key") || ex.Message.Contains("Unauthorized"))
                {
                    _logger.LogError("💡 Make sure to:");
                    _logger.LogError("   1. Get API key from https://resend.com/api-keys");
                    _logger.LogError("   2. Set EmailSettings__ResendApiKey in environment variables");
                }

                throw;
            }
        }
    }

    // Classe wrapper pour IOptionsSnapshot<ResendClientOptions>
    internal class ResendOptionsWrapper : IOptionsSnapshot<ResendClientOptions>
    {
        private readonly ResendClientOptions _options;

        public ResendOptionsWrapper(ResendClientOptions options)
        {
            _options = options;
        }

        public ResendClientOptions Value => _options;

        public ResendClientOptions Get(string name) => _options;
    }
}