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
                    _logger.LogError("❌ ABSTRACT_API_KEY not found in environment variables!");
                    _logger.LogWarning("⚠️ Allowing registration since API key is missing");
                    return true; // ✅ ALLOW if API key missing (for now, until you add it to Railway)
                }

                var httpClient = _httpClientFactory.CreateClient();
                var url = $"https://emailvalidation.abstractapi.com/v1/?api_key={apiKey}&email={email}";

                _logger.LogInformation($"🔍 Validating email with AbstractAPI: {email}");

                var response = await httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"📡 AbstractAPI Response Status: {response.StatusCode}");
                _logger.LogInformation($"📡 AbstractAPI Response Body: {content}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"❌ AbstractAPI error (Status {response.StatusCode}): {content}");
                    _logger.LogWarning("⚠️ Allowing registration due to API error");
                    return true; // ✅ ALLOW if API fails
                }

                var result = JsonSerializer.Deserialize<AbstractResponse>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                _logger.LogInformation($"📊 AbstractAPI Result for {email}:");
                _logger.LogInformation($"   - Deliverability: {result?.Deliverability ?? "NULL"}");
                _logger.LogInformation($"   - Is Valid Format: {result?.IsValidFormat?.Value.ToString() ?? "NULL"}");
                _logger.LogInformation($"   - Is Free Email: {result?.IsFreeEmail?.Value.ToString() ?? "NULL"}");
                _logger.LogInformation($"   - Is Disposable: {result?.IsDisposableEmail?.Value.ToString() ?? "NULL"}");

                // ✅ Check if email is deliverable and has valid format
                bool isDeliverable = result?.Deliverability == "DELIVERABLE";
                bool isValidFormat = result?.IsValidFormat?.Value == true;

                _logger.LogInformation($"   - isDeliverable: {isDeliverable}");
                _logger.LogInformation($"   - isValidFormat: {isValidFormat}");

                // ✅ Accept email if BOTH conditions are true
                bool isReal = isDeliverable && isValidFormat;

                if (isReal)
                {
                    _logger.LogInformation($"✅ Email ACCEPTED: {email}");
                }
                else
                {
                    _logger.LogWarning($"❌ Email REJECTED: {email}");
                    _logger.LogWarning($"   Reason: Deliverability={result?.Deliverability}, ValidFormat={result?.IsValidFormat?.Value}");
                }

                return isReal;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Exception while validating email: {email}");
                _logger.LogWarning("⚠️ Allowing registration due to exception");
                return true; // ✅ ALLOW if exception occurs
            }
        }

        private class AbstractResponse
        {
            public string? Deliverability { get; set; }
            public FormatInfo? IsValidFormat { get; set; }
            public FreeEmailInfo? IsFreeEmail { get; set; }
            public DisposableEmailInfo? IsDisposableEmail { get; set; }
        }

        private class FormatInfo
        {
            public bool Value { get; set; }
        }

        private class FreeEmailInfo
        {
            public bool Value { get; set; }
        }

        private class DisposableEmailInfo
        {
            public bool Value { get; set; }
        }
    }
}