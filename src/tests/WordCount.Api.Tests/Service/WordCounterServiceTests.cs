using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Logging.Interfaces;
using Moq;
using NUnit.Framework;
using WordCount.Api.Core.Data.ExternalService;
using WordCount.Api.Core.Data.Models;
using WordCount.Api.Core.Data.Service;
using WordCount.Api.Core.Services;

namespace WordCount.Api.Tests.Service
{
    [TestFixture]
    public class WordCounterServiceTests
    {
        private MockRepository _mockRepository;
        private Mock<IDefinitionsApiService> _definitionsApiServiceMock;
        private Mock<IWordProcessorService> _wordProcessorServiceMock;
        private IWordCounterService _wordCounterService;
        private Mock<ILogger<WordCounterService>> _loggerMock;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _definitionsApiServiceMock = _mockRepository.Create<IDefinitionsApiService>();
            _loggerMock = _mockRepository.Create<ILogger<WordCounterService>>();
            _wordProcessorServiceMock = _mockRepository.Create<IWordProcessorService>();
            
            _wordCounterService = new WordCounterService(_definitionsApiServiceMock.Object,
                _wordProcessorServiceMock.Object, _loggerMock.Object);
        }

        [TearDown]
        public void Teardown()
        {
            _mockRepository.VerifyAll();
        }

        [Test]
        public async Task ProcessWordsWithDefinitionsAsync_Success()
        {

            var countedWords = GetRandomWordDictionary(11);

            _wordProcessorServiceMock.Setup(x => x.FetchWordsWithCount(It.IsAny<string>()))
                .Returns(countedWords);

            var expected = countedWords.OrderByDescending(x => x.Value).
                Take(10).ToDictionary(x => x.Key, x => x.Value);

            var responses = GetRandomApiResponsesFromWordDictionary(expected);

            _definitionsApiServiceMock.SetupSequence(x =>
                    x.FetchDefinitionsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(responses[0])
                .ReturnsAsync(responses[1])
                .ReturnsAsync(responses[2])
                .ReturnsAsync(responses[3])
                .ReturnsAsync(responses[4])
                .ReturnsAsync(responses[5])
                .ReturnsAsync(responses[6])
                .ReturnsAsync(responses[7])
                .ReturnsAsync(responses[8])
                .ReturnsAsync(responses[9]);

            var countApiResponses = await _wordCounterService.ProcessWordsWithDefinitions("text", 10);

            var results = countApiResponses.ToDictionary(x => x.Word, x => x.Count);

            expected.Should().BeEquivalentTo(results);
            countApiResponses.Any(x => x.Definitions.Any()).Should().BeTrue();
        }

        [Test]
        public async Task ProcessWordsWithDefinitionsAsync_With_Some_Definitions_Populated_Success()
        {
            var countedWords = GetRandomWordDictionary(11);

            _wordProcessorServiceMock.Setup(x => x.FetchWordsWithCount(It.IsAny<string>()))
                .Returns(countedWords);

            var expected = countedWords.OrderByDescending(x => x.Value).Take(10).ToDictionary(x => x.Key, x => x.Value);
            const string value = "test1";
            var response = new ApiResponse()
            {
                Definitions = new List<Definitions>()
                {
                    new Definitions()
                    {
                        Definition = value,
                        Emoji = value,
                        Example = value,
                        ImageUrl = value,
                        Type = value
                    }
                },
                HttpCode = 200,
                Pronunciation = value,
                Word = value
            };

            _definitionsApiServiceMock.SetupSequence(x =>
                    x.FetchDefinitionsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response)
                .ReturnsAsync(new ApiResponse())
                .ReturnsAsync(new ApiResponse())
                .ReturnsAsync(new ApiResponse())
                .ReturnsAsync(new ApiResponse())
                .ReturnsAsync(new ApiResponse())
                .ReturnsAsync(new ApiResponse())
                .ReturnsAsync(new ApiResponse())
                .ReturnsAsync(new ApiResponse())
                .ReturnsAsync(new ApiResponse());

            var countApiResponses = await _wordCounterService.ProcessWordsWithDefinitions("text", 10);

            var results = countApiResponses.ToDictionary(x => x.Word, x => x.Count);

            expected.Should().BeEquivalentTo(results);
            var populatedDefinitions = countApiResponses.First(x => x.Definitions.FirstOrDefault(x=>x.Definition ==value)!=null);
            populatedDefinitions.Should().NotBeNull();
        }


        private static Dictionary<string, int> GetRandomWordDictionary(int size)
        {
            var random = new Random();
            var result = new Dictionary<string, int>();

            for (var i = 1; i <= size; i++)
            {
                var word = $"test{i}";
                result.Add(word, word == "test1" ? 705 : random.Next(10, 700));
            }

            return result;
        }

        private static List<ApiResponse> GetRandomApiResponsesFromWordDictionary(Dictionary<string, int> dictionary)
        {
            return dictionary.Select(dic => dic.Key)
                .Select(value => new ApiResponse()
                {
                    Definitions = new List<Definitions>()
                    {
                        new Definitions()
                        {
                            Definition = value,
                            Emoji = value,
                            Example = value,
                            ImageUrl = value,
                            Type = value
                        }
                    },
                    HttpCode = 200,
                    Pronunciation = value,
                    Word = value
                })
                .ToList();
        }

    }
}
