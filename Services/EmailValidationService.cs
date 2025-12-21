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
                var apiKey = Environment.GetEnvironmentVariable("ABSTRACT_API_KEY");

                if (string.IsNullOrEmpty(apiKey))
                {
                    _logger.LogWarning("⚠️ ABSTRACT_API_KEY not found");
                    return true; // Allow registration if API key missing
                }

                var httpClient = _httpClientFactory.CreateClient();
                var url = $"https://emailvalidation.abstractapi.com/v1/?api_key={apiKey}&email={email}";

                _logger.LogInformation($"🔍 Checking if email is real: {email}");

                var response = await httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"⚠️ API error: {content}");
                    return true; // Allow if API fails
                }

                var result = JsonSerializer.Deserialize<AbstractResponse>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // Check if email is deliverable
                bool isReal = result?.Deliverability == "DELIVERABLE" &&
                              result?.IsValidFormat?.Value == true;

                _logger.LogInformation($"✅ Email is real: {isReal} - {email}");

                return isReal;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error checking email: {email}");
                return true; // Allow if error
            }
        }

        private class AbstractResponse
        {
            public string Deliverability { get; set; } = "";
            public FormatInfo? IsValidFormat { get; set; }
        }

        private class FormatInfo
        {
            public bool Value { get; set; }
        }
    }
}