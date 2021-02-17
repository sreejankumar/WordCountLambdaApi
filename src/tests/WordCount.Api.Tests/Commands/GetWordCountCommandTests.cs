using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Api.Core.Data.Constants;
using Api.Core.Exceptions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using WordCount.Api.Core.Commands;
using WordCount.Api.Core.Data.Dtos;
using WordCount.Api.Core.Data.Filters;
using WordCount.Api.Core.Data.Messages;
using WordCount.Api.Core.Data.Models;
using WordCount.Api.Core.Services;

namespace WordCount.Api.Tests.Commands
{
    public class GetWordCountCommandTests
    {
        private MockRepository _repository;
        private GetWordCountCommand _wordCountCommand;
        private Mock<IWordCounterService> _wordCounterServiceMock;

        [OneTimeSetUp]
        public void Setup()
        {
            _repository = new MockRepository(MockBehavior.Strict);
            _wordCounterServiceMock = _repository.Create<IWordCounterService>();
            _wordCountCommand = new GetWordCountCommand(_wordCounterServiceMock.Object);
        }

        [TearDown]
        public void Teardown()
        {
            _repository.VerifyAll();
        }

        [Test]
        public void Validate_SearchParameters_Success()
        {
            var path = Path.Combine(Environment.CurrentDirectory, "TestData/TestFile.txt");
            using var stream = File.OpenRead(path);
            var file = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name))
            {
                Headers = new HeaderDictionary(),
                ContentType = MimeTypesConstants.Text.Plain
            };

            var wordCountSearchParameter = new WordCountSearchParameter
            {
                File = file,
            };
            var result = _wordCountCommand.Validate(wordCountSearchParameter);
            result.Should().Be(Task.CompletedTask);
        }

        [Test]
        public void Validate_SearchParameters_File_Is_Empty_Failure()
        {
            var wordCountSearchParameter = new WordCountSearchParameter
            {
                File = null
            };
            _wordCountCommand.Invoking(x => x.Validate(wordCountSearchParameter)).Should().Throw<ValidationException>()
                .WithMessage(
                    ExceptionMessages.InvalidInput(nameof(wordCountSearchParameter.File)));
        }

        [Test]
        public void Validate_SearchParameters_With_Invalid_ContentType_Failure()
        {
            const string value = "MimeTypesConstants.Application.AtomcatXml";
            var path = Path.Combine(Environment.CurrentDirectory, "TestData/TestFile.docx");
            using var stream = File.OpenRead(path);
            var file = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name))
            {
                Headers = new HeaderDictionary(),
                ContentType = value
            };

            var wordCountSearchParameter = new WordCountSearchParameter
            {
                File = file
            };

            _wordCountCommand.Invoking(x => x.Validate(wordCountSearchParameter)).Should().Throw<ValidationException>()
                .WithMessage(
                    ExceptionMessages.AllowedInputContentTypes(value, WordCountSearchParameter.AllowedContextTypes));
        }

        [Test]
        public void Validate_SearchParameters_Property_Is_Empty_Failure()
        {
            var path = Path.Combine(Environment.CurrentDirectory, "TestData/TestFileWithNoData.txt");
            using var stream = File.OpenRead(path);
            var file = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name))
            {
                Headers = new HeaderDictionary(),
                ContentType = MimeTypesConstants.Text.Plain
            };

            var wordCountSearchParameter = new WordCountSearchParameter
            {
                File = file,
            };

            _wordCountCommand.Invoking(x => x.Validate(wordCountSearchParameter)).Should().Throw<ValidationException>()
                .WithMessage(
                    ExceptionMessages.InvalidInput(nameof(wordCountSearchParameter.Text)));
        }


        [Test]
        public async Task Run_Success()
        {
            var path = Path.Combine(Environment.CurrentDirectory, "TestData/TestFile.txt");
            await using var stream = File.OpenRead(path);
            var file = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name))
            {
                Headers = new HeaderDictionary(),
                ContentType = MimeTypesConstants.Text.Plain
            };

            var wordCountSearchParameter = new WordCountSearchParameter
            {
                File = file,
            };
            wordCountSearchParameter.Validate();

            _wordCounterServiceMock.Setup(x => x.ProcessWordsWithDefinitionsAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(new List<WordCountApiResponse>
                {
                    new WordCountApiResponse
                    {
                        Count = 10,
                        Definitions = new List<Definitions>(),
                        Word = "Test"
                    }
                });

            var result = await _wordCountCommand.Run(wordCountSearchParameter);

            result.Any().Should().BeTrue();
        }
    }
}
