using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Logging.Extensions;
using Logging.Interfaces;
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
        private readonly ILogger<WordCounterService> _logger;

        public WordCounterService(IDefinitionsApiService definitionsApiService,
            IWordProcessorService wordProcessorService, ILogger<WordCounterService> logger)
        {
            _definitionsApiService = definitionsApiService;
            _wordProcessorService = wordProcessorService;
            _logger = logger;
        }

        /// <summary>
        /// Get the words and definitions.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public Task<List<WordCountApiResponse>> ProcessWordsWithDefinitions(string text, int limit)
        {
            var wordCountsWithDefinitions = new List<WordCountApiResponse>();
            var fetchWordsWithCounts = _wordProcessorService.FetchWordsWithCount(text);

            var sortedWords = fetchWordsWithCounts.OrderByDescending(x => x.Value).Take(limit)
                .ToDictionary(x => x.Key, x => x.Value);

            var responses = GetDefinitions(sortedWords);

            var definitions = responses.Where(x => x.Word.HasValue())
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

            return Task.FromResult(wordCountsWithDefinitions);
        }

        private IEnumerable<ApiResponse> GetDefinitions(Dictionary<string, int> sortedWords)
        {
            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            var apiResponses = new List<ApiResponse>();
            var task = Task.Run(async () =>
            {
                foreach (var sortedWord in sortedWords)
                {
                    apiResponses.Add(await _definitionsApiService.FetchDefinitionsAsync(sortedWord.Key, token));
                }
            }, token);

            try
            {
                task.Wait(token);

            }
            catch (AggregateException ex)
            {
                tokenSource.Cancel();
                ex.Handle((exception) =>
                {
                    _logger.LogDebug("Cancel all others child tasks  requested ");
                    return true;
                });
                _logger.LogDebug($"\n{nameof(AggregateException)} thrown\n");
            }
            finally
            {
                tokenSource.Dispose();
            }

            return apiResponses;
        }
    }
}