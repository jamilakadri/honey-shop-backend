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
                    _logger.LogError("⚠️ ABSTRACT_API_KEY not found in Railway environment variables!");
                    return false; // ❌ BLOCK if API key missing
                }

                var httpClient = _httpClientFactory.CreateClient();
                var url = $"https://emailvalidation.abstractapi.com/v1/?api_key={apiKey}&email={email}";

                _logger.LogInformation($"🔍 Validating email with AbstractAPI: {email}");

                var response = await httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"❌ AbstractAPI error (Status {response.StatusCode}): {content}");
                    return false; // ❌ BLOCK if API fails
                }

                var result = JsonSerializer.Deserialize<AbstractResponse>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                _logger.LogInformation($"📊 AbstractAPI Response for {email}:");
                _logger.LogInformation($"   - Deliverability: {result?.Deliverability}");
                _logger.LogInformation($"   - Is Valid Format: {result?.IsValidFormat?.Value}");
                _logger.LogInformation($"   - Is Free Email: {result?.IsFreeEmail?.Value}");
                _logger.LogInformation($"   - Is Disposable: {result?.IsDisposableEmail?.Value}");

                // ✅ STRICT VALIDATION - Email must be DELIVERABLE and VALID FORMAT
                bool isReal = result?.Deliverability == "DELIVERABLE" &&
                              result?.IsValidFormat?.Value == true;

                if (isReal)
                {
                    _logger.LogInformation($"✅ Email ACCEPTED: {email}");
                }
                else
                {
                    _logger.LogWarning($"❌ Email REJECTED: {email} - Deliverability: {result?.Deliverability}, Valid Format: {result?.IsValidFormat?.Value}");
                }

                return isReal;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error validating email: {email}");
                return false; // ❌ BLOCK if error occurs
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