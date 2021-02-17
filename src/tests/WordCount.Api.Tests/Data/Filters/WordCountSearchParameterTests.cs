using System;
using System.IO;
using Api.Core.Data.Constants;
using Api.Core.Exceptions;
using FluentAssertions;
using Logging.Extensions;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using WordCount.Api.Core.Data.Filters;
using WordCount.Api.Core.Data.Messages;

namespace WordCount.Api.Tests.Data.Filters
{
    [TestFixture]
    public class WordCountSearchParameterTests
    {
        [Test]
        public void Validate_Text_File_Upload_Success()
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

            wordCountSearchParameter.Validate();
            wordCountSearchParameter.Limit.Should().Be(WordCountSearchParameter.DefaultLimit);
            wordCountSearchParameter.Text.HasValue().Should().BeTrue();
        }

        [Test]
        public void Validate_Text_File_With_Limit_Upload_Success()
        {
            var path = Path.Combine(Environment.CurrentDirectory, "TestData/TestFile.txt");
            using var stream = File.OpenRead(path);
            var file = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name))
            {
                Headers = new HeaderDictionary(),
                ContentType = MimeTypesConstants.Text.Plain
            };
            const int limit = 100;
            var wordCountSearchParameter = new WordCountSearchParameter
            {
                File = file,
                Limit = limit
            };

            wordCountSearchParameter.Validate();
            wordCountSearchParameter.Limit.Should().Be(limit);
            wordCountSearchParameter.Text.HasValue().Should().BeTrue();
        }

        [Test]
        public void Validate_Docx_File_Upload_Success()
        {
            var path = Path.Combine(Environment.CurrentDirectory, "TestData/TestFile.docx");
            using var stream = File.OpenRead(path);
            var file = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name))
            {
                Headers = new HeaderDictionary(),
                ContentType = Core.Data.Constants.MimeTypesConstants.Application.WordDocx
            };

            var wordCountSearchParameter = new WordCountSearchParameter
            {
                File = file,
            };

            wordCountSearchParameter.Validate();
            wordCountSearchParameter.Limit.Should().Be(WordCountSearchParameter.DefaultLimit);
            wordCountSearchParameter.Text.HasValue().Should().BeTrue();
        }

        [Test]
        public void Validate_With_Empty_Data_Failure()
        {
            var wordCountSearchParameter = new WordCountSearchParameter
            {
                File = null
            };

            wordCountSearchParameter.Invoking(x => x.Validate()).Should().Throw<ValidationException>().WithMessage(
                ExceptionMessages.InvalidInput(nameof(wordCountSearchParameter.File)));
        }

        [Test]
        public void Validate_Text_With_Invalid_ContentType_Failure()
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

            wordCountSearchParameter.Invoking(x => x.Validate()).Should().Throw<ValidationException>().WithMessage(
                ExceptionMessages.AllowedInputContentTypes(value, WordCountSearchParameter.AllowedContextTypes));
        }

        [Test]
        public void Validate_Text_Property_Is_Empty_Failure()
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

            wordCountSearchParameter.Invoking(x => x.Validate()).Should().Throw<ValidationException>().WithMessage(
                ExceptionMessages.InvalidInput(nameof(wordCountSearchParameter.Text)));
        }
    }
}
