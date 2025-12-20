// Create this file at: Helpers/UrlHelper.cs

namespace MielShop.API.Helpers
{
    public static class UrlHelper
    {
        public static string GetBackendBaseUrl(IConfiguration configuration)
        {
            // Check for Railway public domain first (production)
            var railwayDomain = Environment.GetEnvironmentVariable("RAILWAY_PUBLIC_DOMAIN");
            if (!string.IsNullOrEmpty(railwayDomain))
            {
                return $"https://{railwayDomain}";
            }

            // Check for custom backend URL environment variable
            var backendUrl = Environment.GetEnvironmentVariable("BACKEND_URL");
            if (!string.IsNullOrEmpty(backendUrl))
            {
                return backendUrl;
            }

            // Fall back to configuration
            return configuration["AppSettings:BackendUrl"] ?? "https://honey-shop-backend-production.up.railway.app";
        }

        public static string GetFullImageUrl(IConfiguration configuration, string relativeUrl)
        {
            if (string.IsNullOrEmpty(relativeUrl))
            {
                return string.Empty;
            }

            // If it's already a full URL, return as-is
            if (relativeUrl.StartsWith("http://") || relativeUrl.StartsWith("https://"))
            {
                return relativeUrl;
            }

            // Build full URL
            var baseUrl = GetBackendBaseUrl(configuration);
            return $"{baseUrl.TrimEnd('/')}/{relativeUrl.TrimStart('/')}";
        }
    }
}