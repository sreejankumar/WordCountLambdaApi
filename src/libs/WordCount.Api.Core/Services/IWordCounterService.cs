using System.Collections.Generic;
using System.Threading.Tasks;
using WordCount.Api.Core.Data.Dtos;

namespace WordCount.Api.Core.Services
{
    public interface IWordCounterService
    {
        Task<List<WordCountApiResponse>> ProcessWordsWithDefinitions(string text, int limit);
    }
}