using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WordCount.Api.Core.Utility
{
    internal static class HttpResponseMessageExtensions
    {
        public static async Task<T> ReadResponseAsync<T>(this HttpResponseMessage response)
        {
            var jsonString = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(jsonString);
        }
    }
}