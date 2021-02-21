using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Api.Core.Exceptions;
using Logging.Interfaces;
using Microsoft.Extensions.Options;
using WordCount.Api.Core.Configuration;
using WordCount.Api.Core.Data.Models;
using WordCount.Api.Core.Utility;

namespace WordCount.Api.Core.Data.ExternalService
{
    public class DefinitionsApiService : IDefinitionsApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IOptions<DefinitionApiConfiguration> _definitionApiConfiguration;
        private readonly ILogger<DefinitionsApiService> _logger;

        public DefinitionsApiService(IOptions<DefinitionApiConfiguration> definitionApiConfiguration,
            IHttpClientFactory httpClientFactory, ILogger<DefinitionsApiService> logger)
        {
            _definitionApiConfiguration = definitionApiConfiguration;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
        }

        /// <summary>
        /// Fetch the Fetch Definitions Async from searchWord
        /// </summary>
        /// <param name="searchWord"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ExternalServiceException"></exception>
        public async Task<ApiResponse> FetchDefinitionsAsync(string searchWord,
            CancellationToken cancellationToken = default)
        {
            var apiResponse = new ApiResponse();
            try
            {
                CancelTask($"Task with {searchWord} is cancelled before it got started", cancellationToken);

                var url = $"{_definitionApiConfiguration.Value.Address}{searchWord}";

                _logger.LogDebug($"Making API Request to {url}");
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Token", _definitionApiConfiguration.Value.Token);
                var response = await _httpClient.GetAsync(url, cancellationToken);

                var statusCode = (int) response.StatusCode;
                _logger.LogDebug($"Finished the request and got a StatusCode {statusCode} ");
               
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        apiResponse = await response.EnsureSuccessStatusCode().ReadResponseAsync<ApiResponse>();
                        break;
                    case HttpStatusCode.NotFound:
                    {
                        var messageResponse = await response.ReadResponseAsync<List<BaseResponse>>();
                        apiResponse.Message = messageResponse[0].Message;
                        break;
                    }
                    default:
                        throw new ExternalServiceException($"HttpStatusCode({statusCode}) was not reconsigned");
                }
                apiResponse.HttpCode = statusCode;
            }
            catch (Exception e)
            {
                CancelTask($"Task with { searchWord } is cancelled", cancellationToken);
                _logger.LogError("Exception occurred", e);
                throw new ExternalServiceException($"HttpStatusCode({apiResponse.HttpCode}) was not reconsigned");
            }

            return apiResponse;
        }

        private void CancelTask(string text, CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested) return;
            _logger.LogDebug(text);
            cancellationToken.ThrowIfCancellationRequested();
        }
    }
}