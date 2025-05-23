namespace MerchStore.Application.Configuration
{
    public class ExternalApiSettings
    {
        public const string SectionName = "ExternalProductApi";

        public string BaseUrl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}