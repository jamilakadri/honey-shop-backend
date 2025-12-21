using System.Text.Json;

namespace MielShop.API.Services
{
    public interface IEmailValidationService
    {
        Task<bool> IsEmailRealAsync(string email);
    }

    public class EmailValidationService : IEmailValidationService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<EmailValidationService> _logger;

        public EmailValidationService(
            IHttpClientFactory httpClientFactory,
            ILogger<EmailValidationService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<bool> IsEmailRealAsync(string email)
        {
            try
            {
                // Get EmailListVerify API key from environment
                var apiKey = Environment.GetEnvironmentVariable("EMAILLISTVERIFY_API_KEY");

                if (string.IsNullOrEmpty(apiKey))
                {
                    _logger.LogWarning("⚠️ EMAILLISTVERIFY_API_KEY not found - Allowing all registrations");
                    return true; // Allow if no API key
                }

                var httpClient = _httpClientFactory.CreateClient();
                // EmailListVerify API endpoint
                var url = $"https://apps.emaillistverify.com/api/verifyEmail?secret={apiKey}&email={email}";

                _logger.LogInformation($"🔍 Validating email with EmailListVerify: {email}");

                var response = await httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"📡 EmailListVerify Response Status: {response.StatusCode}");
                _logger.LogInformation($"📡 EmailListVerify Response: {content}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"❌ EmailListVerify API error: {content}");
                    _logger.LogWarning("⚠️ Allowing registration due to API error");
                    return true; // Allow if API fails
                }

                // EmailListVerify returns plain text status, not JSON
                var status = content.Trim().ToLower();

                _logger.LogInformation($"📊 EmailListVerify Status: {status}");

                // Accept only if status is "ok"
                // Possible statuses: "ok", "email_disabled", "invalid", "unknown", "disposable", "error"
                bool isValid = status == "ok";

                if (isValid)
                {
                    _logger.LogInformation($"✅ Email ACCEPTED: {email}");
                }
                else
                {
                    _logger.LogWarning($"❌ Email REJECTED: {email} - Status: {status}");
                }

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Exception validating email: {email}");
                _logger.LogWarning("⚠️ Allowing registration due to exception");
                return true; // Allow if exception
            }
        }
    }
}