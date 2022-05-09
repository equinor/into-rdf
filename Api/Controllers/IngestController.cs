using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Doc2RdfService;

namespace Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class IngestController : ControllerBase
    {
        readonly IDoc2RdfService _doc2RdfService;

        public IngestController(IDoc2RdfService doc2RdfService)
        {
            _doc2RdfService = doc2RdfService;
        }

        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [RequestSizeLimit(Int32.MaxValue)]
        [HttpPost("convert", Name = "Convert to ttl")]
        public async Task<IActionResult> Post(IFormFile postedFile)
        {
            if (postedFile is null) return BadRequest("No file");
            using var stream = new MemoryStream();
            await postedFile.CopyToAsync(stream);
            if (stream.Length is 0) return BadRequest("No file stream");
            return Ok(_doc2RdfService.PostDoc2Rdf(stream, postedFile.FileName));
        }
    }
}
