using Api.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize(Roles = Roles.Admin)]
[ApiController]
[Route("[controller]")]
public class DataController : ControllerBase
{
    public DataController()
    {
    }

    /// <summary>
    /// Delete a complete named graph including referencing data from main fuseki
    /// </summary>
    /// <param name="server">specifies the server to delete from.</param>
    /// <param name="namedGraph">specifies the namedGraph to delete.</param>
    [ProducesResponseType(typeof(string), StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpDelete("{server}/{namedGraph}")]
    public Task<IActionResult> DeleteGraph(string server, string namedGraph)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Update data. Should mostly be used for debugging and experimentation. Actual data changes should be uploaded as a seperate revision.
    /// </summary>
    /// <param name="server">specifies the server to update.</param>
    /// <param name="notes">this goes in main fuseki to describe update, together with name of uses / app, date and the sparqlQuery</param>
    /// <param name="sparqlQuery">the sparql query used for updating.see https://docs.marklogic.com/guide/semantics/sparql-update</param>
    /// <note> sparql update syntax let us specify named graphs to update</note>
    /// TODO set correct content type of incoming sparqlQuery
    ///
    [ProducesResponseType(typeof(string), StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpPost("{server}/{namedGraph}/update")]
    public Task<IActionResult> UpdateGraph(string server, string notes, [FromBody] string sparqlQuery)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Posts excel
    /// </summary>
    /// <param name="revisionTrain">specifies the id of the revision train this data belongs to.</param>
    /// <param name="revision">specifies the name of this revision.</param>
    /// <param name="date">specifies the date of this revision. Should be in the form YYYY-MM-DD</param>
    /// <param name="formFile">Mel excel file with provo data</param>
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [RequestSizeLimit(int.MaxValue)]
    [HttpPost("upload/excel")]
    public Task<IActionResult> UploadExcel([FromQuery] string revisionTrain, [FromQuery] string revision, [FromQuery] string date, IFormFile formFile)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Post .ttl
    /// </summary>
    /// <param name="revisionTrain">specifies the id of the revision train this data belongs to.</param>
    /// <param name="revision">specifies the name of this revision.</param>
    /// <param name="date">specifies the date of this revision. Should be in the form YYYY-MM-DD</param>
    /// <param name="formFile">Turtle file</param>
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [RequestSizeLimit(int.MaxValue)]
    [HttpPost("upload/rdf")]
    public Task<IActionResult> UploadRdf([FromQuery] string revisionTrain, [FromQuery] string revision, [FromQuery] string date, IFormFile formFile)
    {
        throw new NotImplementedException();
        }


    /// <summary>
    /// Post .AML File
    /// </summary>
    /// <param name="revisionTrain">specifies the id of the revision train this data belongs to.</param>
    /// <param name="revision">specifies the name of this revision.</param>
    /// <param name="date">specifies the date of this revision. Should be in the form YYYY-MM-DD</param>
    /// <param name="formFile">AML Serialized as XML</param>
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [RequestSizeLimit(int.MaxValue)]
    [HttpPost("upload/aml")]
    public Task<IActionResult> UploadAMLFile([FromQuery] string revisionTrain, [FromQuery] string revision, [FromQuery] string date, IFormFile formFile)
    {
        throw new NotImplementedException();
    }
}
