namespace MielShop.API.Models
{
    public class EmailSettings
    {
        public string ResendApiKey { get; set; } = string.Empty; // Changed from SendGridApiKey
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
    }

    public class AppSettings
    {
        public string FrontendUrl { get; set; } = string.Empty;
    }
}