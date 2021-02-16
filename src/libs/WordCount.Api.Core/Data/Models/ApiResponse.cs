using System.Collections.Generic;

namespace WordCount.Api.Core.Data.Models
{
    public class ApiResponse : BaseResponse
    {
        public string Pronunciation { get; set; }
        public string Word { get; set; }
        public List<Definitions> Definitions { get; set; }
        public int HttpCode { get; set; }
    }
}