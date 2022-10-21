using Api.Authorization;
using Common.RevisionTrain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Authorize(Roles = Roles.Admin)]
    [ApiController]
    [Route("[controller]")]
    public class RevisionTrainController : ControllerBase
    {
        /// <summary>
        /// Puts an Engineering Numbering System (ENS) revision train. A revision train is a named individual holding information common between revisions.
        /// ENS-trains in particular is created to hold informaiton about a series of ENS-documents (for example Mel)
        /// </summary>
        /// <param name="server">specifies the server that turtle should be uploaded to. All future revisions will be uploaded as individual named graphs to this server</param>
        /// <param name="turtle">information about future revisioned data that is common between revisions, for example facilityId</param>
        /// TODO set correct content type of incoming turtle
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [RequestSizeLimit(int.MaxValue)]
        [HttpPost("{server}")]
        public Task<IActionResult> UploadRevisionTrain(string server, [FromBody] string turtle)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get an ens revision train based by id
        /// </summary>
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpGet("ensDocumentTrain/{id}")]
        public Task<EnsRevisionTrain> GetEnsRevisionTrain(string id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get all ens revision trains
        /// </summary>
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpGet("ensDocumentTrain/")]
        public Task<List<EnsRevisionTrain>> GetAllEnsRevisionTrain()
        {
            throw new NotImplementedException();
        }

    }
}
