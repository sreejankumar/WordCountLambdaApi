using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Core.Commands;
using WordCount.Api.Core.Data.Dtos;
using WordCount.Api.Core.Data.Filters;
using WordCount.Api.Core.Services;

namespace WordCount.Api.Core.Commands
{
    public class GetWordCountCommand : Command<WordCountSearchParameter, List<WordCountApiResponse>>
    {
        private readonly IWordCounterService _wordCounterService;

        public GetWordCountCommand(IWordCounterService counterService)
        {
            _wordCounterService = counterService;
        }

        public override Task Validate(WordCountSearchParameter input)
        {
            input.Validate();
            return Task.CompletedTask;
        }

        public override Task<List<WordCountApiResponse>> Run(WordCountSearchParameter input)
        {
            return _wordCounterService.ProcessWordsWithDefinitions(input.Text, input.Limit);
        }
    }
}