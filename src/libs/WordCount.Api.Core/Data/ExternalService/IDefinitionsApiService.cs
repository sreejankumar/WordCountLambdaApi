using System.Threading;
using System.Threading.Tasks;
using WordCount.Api.Core.Data.Models;

namespace WordCount.Api.Core.Data.ExternalService
{
    public interface IDefinitionsApiService
    {
        Task<ApiResponse> FetchDefinitionsAsync(string searchString, CancellationToken cancellationSourceToken = default);
    }
}