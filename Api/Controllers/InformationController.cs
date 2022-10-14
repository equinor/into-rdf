using Api.Authorization;
using Common.AppsettingsModels;
using Common.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;
/// <summary>
/// Useful information about the state of Splinter. Also includes states of the triple stores it is reverse proxying.
/// To be used for debugging / dashboard application / monitoring
/// </summary>
[Authorize(Roles = Roles.Admin)]
[ApiController]
public class InformationController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public InformationController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Get short information about splinter's state without querying downstream fusekis
    /// </summary>
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [HttpGet("info")]
    public IActionResult GetSplinterInformation()
    {
        var info = _configuration.GetSection(ApiKeys.Servers).Get<List<RdfServer>>();
        return Ok(info);
    }
}

