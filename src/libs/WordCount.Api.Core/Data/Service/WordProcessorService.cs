using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Logging.Interfaces;

namespace WordCount.Api.Core.Data.Service
{
    public class WordProcessorService : IWordProcessorService
    {
        private readonly ILogger<WordProcessorService> _logger;

        public WordProcessorService(ILogger<WordProcessorService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Fetch the words and count from the site in the config.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, int> FetchWordsWithCount(string text)
        {
            _logger.LogDebug($"Processing the word Count");
            var wordRegex = new Regex(@"\p{L}+");
            var wordsSplits = wordRegex.Matches(text).Select(c => c.Value.ToLower());
            var countOccurrences = CountOccurrences(wordsSplits, StringComparer.CurrentCultureIgnoreCase);
            _logger.LogDebug($"Finished processing");
            return countOccurrences;
        }

        public static IDictionary<string, int> CountOccurrences(IEnumerable<string> items, IEqualityComparer<string> comparer)
        {
            var counts = new Dictionary<string, int>(comparer);

            foreach (var item in items)
            {
                if (!counts.TryGetValue(item, out var count))
                {
                    count = 0;
                }
                counts[item] = count + 1;
            }
            return counts;
        }
    }
}