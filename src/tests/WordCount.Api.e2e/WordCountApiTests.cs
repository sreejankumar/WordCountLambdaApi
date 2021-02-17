using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Api.Core.Data.Constants;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using NUnit.Framework;
using WordCount.Api.Core.Data.Dtos;
using WordCount.Api.Core.Data.Messages;

namespace WordCount.Api.e2e
{
    public class TesWordCountApiTests
    {
        private const string RequestUrl = "/api/wordcount";
        private HttpClient _client;

        [OneTimeSetUp]
        public void Setup()
        {
            var server = new TestServer(new WebHostBuilder()
                .UseEnvironment("Development")
                .UseStartup<Startup>());

            _client = server.CreateClient();
        }

        [OneTimeTearDown]
        public void Teardown()
        {
            _client?.Dispose();
        }


        [Test]
        public async Task Request_Get_WordCount_Returns_200()
        {
            const string fileName = "TestFile.txt";
            var path = Path.Combine(Environment.CurrentDirectory, "TestData/TestFile.txt");
            using var formContent = new MultipartFormDataContent();
            formContent.Headers.ContentType.MediaType = "multipart/form-data";

            Stream fileStream = File.OpenRead(path);
            formContent.Add(new StreamContent(fileStream), "file", fileName);

            var response = await _client.PostAsync(RequestUrl, formContent);

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<List<WordCountApiResponse>>(content);

            var requestStr =
                await File.ReadAllTextAsync("./TestData/TestJson.json");
            var wordCountApiResponses = JsonConvert.DeserializeObject<List<WordCountApiResponse>>(requestStr);

            foreach (var resultedResponse in wordCountApiResponses.Select(apiResponse =>
                result.FirstOrDefault(x => x.Word == apiResponse.Word)))
            {
                resultedResponse.Should().NotBeNull();
            }
        }

        [TestCase("Text", "multipart/form-data")]
        [TestCase("File", MimeTypesConstants.Application.AtomcatXml)]
        public async Task Request_Get_WordCount_Returns_400(string name, string contentType)
        {
            const string fileName = "TestFileWithNoData.txt";
            var path = Path.Combine(Environment.CurrentDirectory, "TestData/TestFileWithNoData.txt");
            using var formContent = new MultipartFormDataContent();
            formContent.Headers.ContentType.MediaType = contentType;

            Stream fileStream = File.OpenRead(path);
            formContent.Add(new StreamContent(fileStream), "file", fileName);

            var response = await _client.PostAsync(RequestUrl, formContent);

            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var errorContent = JsonConvert.DeserializeObject<ErrorMessage>(content);
            errorContent.Error.Should()
                .Be(ExceptionMessages.InvalidInput(name));

        }

        protected string GetFormattedErrorMessage(string message)
        {
            return $"{{\"error\":\"{message}\"}}";
        }
    }
}