using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using Logging.Extensions;
using Microsoft.Extensions.Options;
using WordCount.Api.Core.Configuration;

namespace WordCount.Api.Core.Data.Service
{
    public class CountWordDataService : ICountWordDataService
    {
        private readonly IOptions<SiteScrapperConfiguration> _siteScrapperOptions;
        private readonly IBrowsingContext _browsingContext;

        public static IHtmlCollection<IElement> Collection { get; set; }

        //private readonly ILogger<CountWordDataService> _logger;

        //public CountWordDataService(IOptions<SiteScrapperConfiguration> siteScrapperOptions,
        //    IBrowsingContext browsingContext, ILogger<CountWordDataService> logger)
        //{
        //    _siteScrapperOptions = siteScrapperOptions;
        //    _browsingContext = browsingContext;
        //    _logger = logger;
        //}

        public CountWordDataService(IOptions<SiteScrapperConfiguration> siteScrapperOptions,
            IBrowsingContext browsingContext)
        {
            _siteScrapperOptions = siteScrapperOptions;
            _browsingContext = browsingContext;
            }

        /// <summary>
        /// Fetch the words and count from the site in the config.
        /// </summary>
        /// <returns></returns>
        public async Task<IDictionary<string, int>> FetchWordsWithCount()
        {
            if (Collection == null)
            {
                //_logger.LogDebug($"Fetching data from the Site Address: {_siteScrapperOptions.Value.SiteAddress}");
                var document = await _browsingContext.OpenAsync(_siteScrapperOptions.Value.SiteAddress);
                //  _logger.LogDebug("Finished fetching the data");

                Collection = document.QuerySelectorAll(_siteScrapperOptions.Value.TextContentTag);
            }

            return (from element in Collection
                    select element.InnerHtml
                into innerHtml
                where innerHtml.HasValue()
                let wordRegex = new Regex(@"\p{L}+")
                select wordRegex.Matches(innerHtml).Select(c => c.Value.ToLower())
                into wordsSplits
                select CountOccurrences(wordsSplits, StringComparer.CurrentCultureIgnoreCase)).FirstOrDefault();
        }

        public static IDictionary<string, int> CountOccurrences(IEnumerable<string> items, IEqualityComparer<string> comparer)
        {
            var counts = new Dictionary<string, int>(comparer);

            foreach (var t in items)
            {
                if (!counts.TryGetValue(t, out var count))
                {
                    count = 0;
                }
                counts[t] = count + 1;
            }

            return counts;
        }

        public void Dispose()
        {
            _browsingContext?.Dispose();
        }
    }
}