using System.Collections.Generic;

namespace WordCount.Api.Core.Data.Service
{
    public interface IWordProcessorService 
    {
        IDictionary<string, int> FetchWordsWithCount(string text);
    }
}