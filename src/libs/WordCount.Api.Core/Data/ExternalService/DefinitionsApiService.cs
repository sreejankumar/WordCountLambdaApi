using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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
        /// <returns></returns>
        /// <exception cref="ExternalServiceException"></exception>
        public async Task<ApiResponse> FetchDefinitionsAsync(string searchWord)
        {
            var apiResponse = new ApiResponse();
            try
            {
                var url = $"{_definitionApiConfiguration.Value.Address}{searchWord}";

               _logger.LogDebug($"Making API Request to {url}");
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Token", _definitionApiConfiguration.Value.Token);
                var response = await _httpClient.GetAsync(url);

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
            catch (HttpRequestException e)
            {
                _logger.LogError("HttpRequestException occurred", e);
            }
            catch (ExternalServiceException e)
            {
                _logger.LogError("ExternalServiceException occurred", e);
            }
            catch (Exception e)
            {
                _logger.LogError("Exception occurred", e);
            }
            return apiResponse;
        }
    }
}