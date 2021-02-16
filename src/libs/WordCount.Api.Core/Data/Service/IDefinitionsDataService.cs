using System.Threading.Tasks;
using WordCount.Api.Core.Data.Models;

namespace WordCount.Api.Core.Data.Service
{
    public interface IDefinitionsDataService
    {
        Task<ApiResponse> FetchDefinitionsAsync(string searchString);
    }
}