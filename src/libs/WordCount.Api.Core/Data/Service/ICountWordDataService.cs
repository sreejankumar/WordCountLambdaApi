using System.Collections.Generic;
using System.Threading.Tasks;

namespace WordCount.Api.Core.Data.Service
{
    public interface ICountWordDataService
    {
        Task<IDictionary<string, int>> FetchWordsWithCount();
    }
}