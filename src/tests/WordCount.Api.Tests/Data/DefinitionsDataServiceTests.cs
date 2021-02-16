using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Logging.Interfaces;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using NUnit.Framework;
using WordCount.Api.Core.Configuration;
using WordCount.Api.Core.Data.Models;
using WordCount.Api.Core.Data.Service;

namespace WordCount.Api.Tests.Data
{
    [TestFixture]
    public class DefinitionsDataServiceTests
    {
        private Mock<IOptions<DefinitionApiConfiguration>> _optionsMock;
        private Mock<IHttpClientFactory> _clientFactoryMock;
        private IDefinitionsDataService _definitionsDataService;
        private Mock<ILogger<DefinitionsDataService>> _loggerMock;
        private Mock<HttpMessageHandler> _messageHandlerMock;
        private MockRepository _repository;

        [SetUp]
        public void Setup()
        {
            // Initialize the mocks
            _repository = new MockRepository(MockBehavior.Strict);
            _optionsMock = _repository.Create<IOptions<DefinitionApiConfiguration>>();
            _loggerMock = _repository.Create<ILogger<DefinitionsDataService>>();
            _messageHandlerMock = _repository.Create<HttpMessageHandler>();
            _clientFactoryMock = _repository.Create<IHttpClientFactory>();

            //Setup the common methods.
            _optionsMock.Setup(x => x.Value).Returns(new DefinitionApiConfiguration());
            _loggerMock.Setup(x => x.LogDebug(It.IsAny<string>(), It.IsAny<string>()));

            var httpClient = new HttpClient(_messageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://someurl.com")
            };

            _clientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

            _definitionsDataService =
                new DefinitionsDataService(_optionsMock.Object, _clientFactoryMock.Object, _loggerMock.Object);
        }

        [TearDown]
        public void Teardown()
        {
            _repository.VerifyAll();
        }

        [Test]
        public async Task FetchDefinitionsAsync_200_Success()
        {
            const string value = "test";
            var definitions = new Definitions
            {
                Definition = value,
                Emoji = value,
                Example = value,
                Type = value,
                ImageUrl = value
            };

            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(new ApiResponse
                {
                    Definitions = new List<Definitions>
                    {
                        definitions
                    },
                    Word = value,
                    Pronunciation = value
                }))
            };

            _messageHandlerMock.Protected().Setup<Task<HttpResponseMessage>>(
                "SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>()).ReturnsAsync(
                response);

            var results = await _definitionsDataService.FetchDefinitionsAsync(value);

            results.Should().NotBeNull();
            results.Word.Should().Be(value);
            results.Pronunciation.Should().Be(value);
            results.HttpCode.Should().Be((int) HttpStatusCode.OK);
            results.Message.Should().BeNullOrEmpty();

            var result = results.Definitions.FirstOrDefault();
            result.Should().NotBeNull();
            result?.Definition.Should().Be(value);
            result?.Emoji.Should().Be(value);
            result?.Example.Should().Be(value);
            result?.Type.Should().Be(value);
            result?.Type.Should().Be(value);
        }

        [Test]
        public async Task FetchDefinitionsAsync_404_Success()
        {
            const string value = "Not Found :(";

            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent(JsonConvert.SerializeObject(new List<BaseResponse>
                {
                    new BaseResponse()
                    {
                        Message = value
                    }
                }))
            };

            _messageHandlerMock.Protected().Setup<Task<HttpResponseMessage>>(
                "SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>()).ReturnsAsync(
                response);

            var results = await _definitionsDataService.FetchDefinitionsAsync("hello");

            results.Should().NotBeNull();
            results.Word.Should().BeNullOrEmpty();
            results.Pronunciation.Should().BeNullOrEmpty();
            results.HttpCode.Should().Be((int) HttpStatusCode.NotFound);
            results.Message.Should().Be(value);
            results.Definitions.Should().BeNullOrEmpty();
        }

        [Test]
        public async Task FetchDefinitionsAsync_400_Failure()
        {
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = null
            };

            _loggerMock.Setup(x => x.LogError(It.IsAny<string>(), It.IsAny<Exception>(), It.IsAny<string>()));
            _messageHandlerMock.Protected().Setup<Task<HttpResponseMessage>>(
                "SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>()).ReturnsAsync(
                response);

            var results = await _definitionsDataService.FetchDefinitionsAsync("hello");

            results.Should().NotBeNull();
            results.Word.Should().BeNullOrEmpty();
            results.Pronunciation.Should().BeNullOrEmpty();
            results.HttpCode.Should().Be(0);
            results.Message.Should().BeNullOrEmpty();
            results.Definitions.Should().BeNullOrEmpty();
        }

        [Test]
        public async Task FetchDefinitionsAsync_Throws_An_HttpRequestException_Failure()
        {
            _loggerMock.Setup(x => x.LogError(It.IsAny<string>(), It.IsAny<Exception>(), It.IsAny<string>()));
            _messageHandlerMock.Protected().Setup<Task<HttpResponseMessage>>(
                    "SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException());

            var results = await _definitionsDataService.FetchDefinitionsAsync("hello");

            results.Should().NotBeNull();
            results.Word.Should().BeNullOrEmpty();
            results.Pronunciation.Should().BeNullOrEmpty();
            results.HttpCode.Should().Be(0);
            results.Message.Should().BeNullOrEmpty();
            results.Definitions.Should().BeNullOrEmpty();
        }

        [Test]
        public async Task FetchDefinitionsAsync_Throws_An_Exception_Failure()
        {
            _loggerMock.Setup(x => x.LogError(It.IsAny<string>(), It.IsAny<Exception>(), It.IsAny<string>()));
            _messageHandlerMock.Protected().Setup<Task<HttpResponseMessage>>(
                    "SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exception());

            var results = await _definitionsDataService.FetchDefinitionsAsync("hello");

            results.Should().NotBeNull();
            results.Word.Should().BeNullOrEmpty();
            results.Pronunciation.Should().BeNullOrEmpty();
            results.HttpCode.Should().Be(0);
            results.Message.Should().BeNullOrEmpty();
            results.Definitions.Should().BeNullOrEmpty();
        }
    }
}