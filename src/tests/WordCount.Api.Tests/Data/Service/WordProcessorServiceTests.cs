using System.IO;
using System.Text;
using FluentAssertions;
using Logging.Interfaces;
using Moq;
using NUnit.Framework;
using WordCount.Api.Core.Data.Service;

namespace WordCount.Api.Tests.Data.Service
{
    [TestFixture]
    public class WordProcessorServiceTests
    {

        private IWordProcessorService _wordProcessorService;
        private Mock<ILogger<WordProcessorService>> _loggerMock;
        private MockRepository _repository;
        private string _testString = string.Empty;

        [OneTimeSetUp]
        public void Setup()
        {
            _repository = new MockRepository(MockBehavior.Strict);
            _loggerMock = _repository.Create<ILogger<WordProcessorService>>();

            _loggerMock.Setup(x => x.LogDebug(It.IsAny<string>(), It.IsAny<string>()));

            _wordProcessorService = new WordProcessorService(_loggerMock.Object);

            var path = Path.Combine(System.Environment.CurrentDirectory, "TestData/TestFile.txt");
            using var streamReader = new StreamReader(path, Encoding.UTF8);
            _testString = streamReader.ReadToEnd();
        }


        [TestCase("Ugudwash,Ugudwash,Ugudwash,Ugudwash,Ugudwash, hello, test 1", "Ugudwash", 5, true)]
        [TestCase("HIAWATHA", "HIAWATHA", 1, true)]
        [TestCase("HIAWATHA", "Leicester", 0, false)]
        public void FetchWordsWithCount_Success(string text, string wordToTest, int count, bool success)
        {
            var result = _wordProcessorService.FetchWordsWithCount(text);
            var exists = result.TryGetValue(wordToTest.ToLower(), out var countValue);
            exists.Should().Be(success);
            countValue.Should().Be(count);
        }

        [TestCase("Ugudwash", 5)]
        [TestCase("HIAWATHA", 446)]
        [TestCase("Leicester", 2)]
        public void FetchWordsFromTextFile_Success(string wordToTest, int count)
        {
            var result = _wordProcessorService.FetchWordsWithCount(_testString);
            var exists = result.TryGetValue(wordToTest.ToLower(), out var countValue);
            exists.Should().BeTrue();
            countValue.Should().Be(count);
        }
    }
}