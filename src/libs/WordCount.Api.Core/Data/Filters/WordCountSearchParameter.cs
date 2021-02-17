using System.IO;
using System.Linq;
using System.Text;
using Api.Core.Data.Constants;
using Api.Core.Exceptions;
using Logging.Extensions;
using Microsoft.AspNetCore.Http;
using WordCount.Api.Core.Data.Messages;

namespace WordCount.Api.Core.Data.Filters
{
    public class WordCountSearchParameter
    {
        public const int DefaultLimit = 10;

        public static readonly string[] AllowedContextTypes = { MimeTypesConstants.Text.Plain, Constants.MimeTypesConstants.Application.WordDocx };
        public string Text { get; set; }
        public IFormFile File { get; set; }

        public int Limit { get; set; } = DefaultLimit;

        public void Validate()
        {
            if (File == null)
            {
                throw new ValidationException(ExceptionMessages.InvalidInput(nameof(File)));
            }

            var contentType = File.ContentType;
            if (contentType.HasValue() && !AllowedContextTypes.Contains(contentType))
            {
                throw new ValidationException(ExceptionMessages.AllowedInputContentTypes(contentType, AllowedContextTypes));
            }

            Text = ReadFileStream();

            if (!Text.HasValue())
            {
                throw new ValidationException(ExceptionMessages.InvalidInput(nameof(Text)));
            }
        }

        private string ReadFileStream()
        {
            var result = new StringBuilder();

            using var reader = new StreamReader(File.OpenReadStream());
            while (reader.Peek() >= 0)
                result.AppendLine(reader.ReadLine());

            return result.ToString();
        }
    }
}
