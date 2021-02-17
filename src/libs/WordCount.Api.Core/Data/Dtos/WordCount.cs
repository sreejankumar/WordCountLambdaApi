using System.Collections.Generic;
using WordCount.Api.Core.Data.Models;

namespace WordCount.Api.Core.Data.Dtos
{
    public class WordCount
    {
        public string Word { get; set; }
        public int Count { get; set; }
        public List<Definitions> Definitions { get; set; }
    }
}