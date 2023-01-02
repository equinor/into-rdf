using Common.Exceptions;
using Common.GraphModels;
using Common.Utils;
using Services.Utils;
using Microsoft.Extensions.Logging;
using Services.FusekiServices;
using VDS.RDF.Query;

namespace Repositories.RecordContextRepository;

public class RecordContextRepository : IRecordContextRepository
{
    private readonly IFusekiService _fusekiService;
    private readonly ILogger<RecordContextRepository> _log;
    private readonly string _server = ServerKeys.Main;

    public RecordContextRepository(IFusekiService fusekiService, ILogger<RecordContextRepository> log)
    {
        _fusekiService = fusekiService;
        _log = log;
    }

    public async Task Add(ResultGraph recordContext)
    {
        var response = await _fusekiService.AddData(_server, recordContext, "text/turtle");

        await ValidateAndLogResponse(response, HttpVerbs.Post, recordContext.Name);
    }

    public async Task Delete(Uri record)
    {
        var response = await _fusekiService.Update(_server, GetDeleteRecordContextQuery(record));

        await ValidateAndLogResponse(response, HttpVerbs.Delete, record.AbsoluteUri);
    }

    private async Task ValidateAndLogResponse(HttpResponseMessage response, HttpVerbs verb, string? identifier = null)
    {
        if (!response.IsSuccessStatusCode)
        {
            var customMessage = string.Empty;
            switch (verb)
            {
                case HttpVerbs.Post:
                    customMessage = $"Failed to add record context {identifier}.";
                    break;
                case HttpVerbs.Delete:
                    customMessage = $"Failed to delete record context {identifier}.";
                    break;
            }

            var content = await response.Content.ReadAsStringAsync();
            var errorMessage = $"{customMessage} Failed with status {response.StatusCode} and message {content}";
            _log.LogWarning(errorMessage);
            throw new BadGatewayException(errorMessage);
        }

        if (response.IsSuccessStatusCode)
        {
            var customMessage = string.Empty;
            switch (verb)
            {
                case HttpVerbs.Post:
                    customMessage = $"Successfully added record context {identifier}.";
                    break;
                case HttpVerbs.Delete:
                    customMessage = $"Successfully deleted record context {identifier}.";
                    break;
            }

            _log.LogInformation(customMessage);
        }
    }

    private string GetDeleteRecordContextQuery(Uri recordContextUri)
    {
        var queryString = new SparqlParameterizedString();
        queryString.Namespaces.AddNamespace("splinter", new Uri("https://rdf.equinor.com/splinter#"));
        queryString.CommandText =
        @$"
        DELETE 
        {{
            ?train splinter:hasRecord @recordContextUri .
            @recordContextUri ?p ?o .
        }}
        WHERE 
        {{
            ?train splinter:hasRecord @recordContextUri .
            @recordContextUri ?p ?o .
        }}";

        queryString.SetUri("recordContextUri", recordContextUri);
        return queryString.ToString();
    }
}

