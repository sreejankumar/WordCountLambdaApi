using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Logging.Extensions;
using WordCount.Api.Core.Data.Dtos;
using WordCount.Api.Core.Data.ExternalService;
using WordCount.Api.Core.Data.Models;
using WordCount.Api.Core.Data.Service;

namespace WordCount.Api.Core.Services
{
    public class WordCounterService : IWordCounterService
    {
        private readonly IDefinitionsApiService _definitionsApiService;
        private readonly IWordProcessorService _wordProcessorService;

        public WordCounterService(IDefinitionsApiService definitionsApiService,
            IWordProcessorService wordProcessorService)
        {
            _definitionsApiService = definitionsApiService;
            _wordProcessorService = wordProcessorService;
        }

        /// <summary>
        /// Get the words and definitions.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public async Task<List<WordCountApiResponse>> ProcessWordsWithDefinitionsAsync(string text, int limit)
        {
            var wordCountsWithDefinitions = new List<WordCountApiResponse>();
            var fetchWordsWithCounts = _wordProcessorService.FetchWordsWithCount(text);

            var sortedWords = fetchWordsWithCounts.OrderByDescending(x => x.Value).Take(limit)
                .ToDictionary(x => x.Key, x => x.Value);

            var tasks = sortedWords.Select(id => _definitionsApiService.FetchDefinitionsAsync(id.Key));
            var apiResponses = await Task.WhenAll(tasks);

            var definitions = apiResponses.Where(x => x.Word.HasValue())
                .ToDictionary(x => x.Word, x => x.Definitions ?? new List<Definitions>());

            foreach (var (word, count) in sortedWords)
            {
                var wordCount = new WordCountApiResponse
                {
                    Count = count,
                    Word = word,
                    Definitions = definitions.TryGetValue(word, out var value)
                        ? value
                        : new List<Definitions>()
                };
                wordCountsWithDefinitions.Add(wordCount);
            }

            return wordCountsWithDefinitions;
        }
    }
}