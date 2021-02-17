using System.Collections.Generic;
using System.Threading.Tasks;

namespace WordCount.Api.Core.Services
{
    public interface IWordCounterService
    {
        Task<List<Data.Dtos.WordCountApiResponse>> ProcessWordsWithDefinitionsAsync(string text, int limit);
    }
}