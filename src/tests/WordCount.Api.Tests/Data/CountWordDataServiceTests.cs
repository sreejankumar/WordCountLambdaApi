using System.Threading.Tasks;
using AngleSharp;
using FluentAssertions;
using Logging.Interfaces;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using WordCount.Api.Core.Configuration;
using WordCount.Api.Core.Data.Service;

namespace WordCount.Api.Tests.Data
{
    [TestFixture]
    public class CountWordDataServiceTests
    {
        private Mock<IOptions<SiteScrapperConfiguration>> _optionsMock;
        private IBrowsingContext _browsingContext;
        private ICountWordDataService _countWordDataService;
        private Mock<ILogger<CountWordDataService>> _loggerMock;
        private MockRepository _repository;

        [SetUp]
        public void Setup()
        {
            _repository = new MockRepository(MockBehavior.Strict);
            _optionsMock = _repository.Create<IOptions<SiteScrapperConfiguration>>();
            _loggerMock = _repository.Create<ILogger<CountWordDataService>>();

            var config = Configuration.Default.WithDefaultLoader();
            _browsingContext = BrowsingContext.New(config);

            _countWordDataService = new CountWordDataService(_optionsMock.Object, _browsingContext, _loggerMock.Object);

            _optionsMock.Setup(x => x.Value).Returns(new SiteScrapperConfiguration()
            {
                SiteAddress = "https://archive.org/stream/songhiawathathe00longrich/songhiawathathe00longrich_djvu.txt",
                TextContentTag = "pre"
            });

            _loggerMock.Setup(x => x.LogDebug(It.IsAny<string>(), It.IsAny<string>()));
        }

        [TearDown]
        public void Teardown()
        {
            _browsingContext.Dispose();
            _countWordDataService = null;
        }

        [Test]
        public async Task FetchWordsWithCount_Success()
        {
            const string wordToText = "Ugudwash";

            var result = await _countWordDataService.FetchWordsWithCount();

            var count = result.TryGetValue(wordToText, out var countValue);
            count.Should().BeTrue();
            countValue.Should().Be(5);
        }
    }
}