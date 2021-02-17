using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Core.Commands;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WordCount.Api.Core.Commands;
using WordCount.Api.Core.Data.Dtos;
using WordCount.Api.Core.Data.Filters;
using ControllerBase = Api.Core.Controller.ControllerBase;

namespace WordCount.Api.Controllers
{
    /// <summary>
    /// Word Count Controller
    /// </summary>
    [Route("api/[controller]")]
    public class WordCountController : ControllerBase
    {
        /// <summary>
        /// Word Count Controller
        /// </summary>
        /// <param name="commandService"></param>
        public WordCountController(ICommandService commandService) : base(commandService)
        {
        }

        /// <summary>
        /// Process the word from the text file uploaded.
        /// </summary>
        /// <param name="file">Upload a file</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(List<WordCountApiResponse>), 200)]
        [ProducesResponseType(typeof(ErrorMessage), 400)]
        [ProducesResponseType(typeof(ErrorMessage), 500)]
        public Task<ActionResult> Post(IFormFile file) =>
            Run<GetWordCountCommand, WordCountSearchParameter, List<WordCountApiResponse>>(
                new WordCountSearchParameter()
                {
                    File = file
                });
    }
}
