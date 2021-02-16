using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp;
using Logging.Extensions;
using Logging.Interfaces;
using Microsoft.Extensions.Options;
using WordCount.Api.Core.Configuration;

namespace WordCount.Api.Core.Data.Service
{
    public class CountWordDataService : ICountWordDataService
    {
        private readonly IOptions<SiteScrapperConfiguration> _siteScrapperOptions;
        private readonly IBrowsingContext _browsingContext;
        private readonly ILogger<CountWordDataService> _logger;

        public CountWordDataService(IOptions<SiteScrapperConfiguration> siteScrapperOptions,
            IBrowsingContext browsingContext, ILogger<CountWordDataService> logger)
        {
            _siteScrapperOptions = siteScrapperOptions;
            _browsingContext = browsingContext;
            _logger = logger;
        }

        /// <summary>
        /// Fetch the words and count from the site in the config.
        /// </summary>
        /// <returns></returns>
        public async Task<IDictionary<string, int>> FetchWordsWithCount()
        {
            var wordDictionary = new Dictionary<string, int>();

            _logger.LogDebug($"Fetching data from the Site Address: {_siteScrapperOptions.Value.SiteAddress}");
            var document = await _browsingContext.OpenAsync(_siteScrapperOptions.Value.SiteAddress);
            _logger.LogDebug("Finished fetching the data");

            var elements = document.QuerySelectorAll(_siteScrapperOptions.Value.TextContentTag);

            foreach (var element in elements)
            {
                var innerHtml = element.InnerHtml;
                if (!innerHtml.HasValue()) continue;
                var matches = Regex.Replace(innerHtml, "[^A-Za-z ]", string.Empty);

                wordDictionary = matches.Split(' ')
                    .GroupBy(s => s)
                    .Select(g => new {Word = g.Key, Count = g.Count()}).ToDictionary(t => t.Word, t => t.Count);
            }

            return wordDictionary;
        }

        public void Dispose()
        {
            _browsingContext?.Dispose();
        }
    }
}