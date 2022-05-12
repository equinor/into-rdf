using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.RdfService;
using System.Text;

namespace Api.Controllers
{
    /// <summary>
    /// Controller for handling rdf uploads
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("[controller]/{server}")]
    public class IngestController : ControllerBase
    {
        readonly IRdfService _rdfService;

        public IngestController(IRdfService doc2RdfService)
        {
            _rdfService = doc2RdfService;
        }

        /// <summary>
        /// Posts excel to fuseki as rdf
        /// </summary>
        /// <param name="server">specifies which fuseki to post to</param>
        /// <param name="postedFile">Mel excel file with provo data</param>
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [RequestSizeLimit(Int32.MaxValue)]
        [HttpPost("upload/excel")]
        public async Task<IActionResult> UploadExcel(string server, IFormFile? postedFile)
        {
            if (postedFile is null) return BadRequest("No file");
            var turtle = await _rdfService.ConvertDocToRdf(postedFile);
            var result = await _rdfService.PostToFuseki(server, turtle);
            return result.IsSuccessStatusCode ? Ok(turtle) : BadRequest(result);
        }

        /// <summary>
        /// Post .ttl
        /// </summary>
        /// <param name="server">specifies which fuseki to post to</param>
        /// <param name="formFile">Turtle file</param>
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [RequestSizeLimit(Int32.MaxValue)]
        [HttpPost("upload/rdf")]
        public async Task<IActionResult> UploadRdf(string server, IFormFile? formFile)
        {
            if (formFile is null) return BadRequest("No file");
            using var streamReader = new StreamReader(formFile.OpenReadStream(), Encoding.UTF8);
            var content = streamReader.ReadToEnd();
            var result = await _rdfService.PostToFuseki(server, content ?? string.Empty);
            return result.IsSuccessStatusCode ? Ok(content) : BadRequest(result);
        }
    }
}
