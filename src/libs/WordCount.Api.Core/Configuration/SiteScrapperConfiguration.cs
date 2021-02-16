namespace WordCount.Api.Core.Configuration
{
    public class SiteScrapperConfiguration
    {
        public const string SiteScrapperConfigurationPrefix = "SiteScrapper";

        public string SiteAddress { get; set; } 
        public string TextContentTag { get; set; }
    }
}