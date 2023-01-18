using Common.Exceptions;
using Services.Utils;
using Microsoft.Extensions.Logging;
using Services.FusekiServices;
using VDS.RDF.Query;

namespace Repositories.RecordRepository;

public class RecordRepository : IRecordRepository
{
    private readonly IFusekiService _fusekiService;
    private readonly ILogger<RecordRepository> _log;

    public RecordRepository(IFusekiService fusekiService, ILogger<RecordRepository> log)
    {
        _fusekiService = fusekiService;
        _log = log;
    }

    public async Task Add(string server, string record, string contentType)
    {
        var response = await _fusekiService.AddData(server, record, contentType);

        await ValidateAndLogResponse(response, HttpVerbs.Post);
    }

    public async Task Delete(string server, Uri record)
    {
        var response = await _fusekiService.Update(server, GetDropRecordQuery(record.AbsoluteUri));

        await ValidateAndLogResponse(response, HttpVerbs.Delete, record.AbsoluteUri);
    }

    private string GetDropRecordQuery(string record)
    {
        var recordUri = new Uri(record);
        var queryString = new SparqlParameterizedString();

        queryString.CommandText = "DROP GRAPH @record ;";
        queryString.SetUri("record", recordUri);

        return queryString.ToString();
    }

    private async Task ValidateAndLogResponse(HttpResponseMessage response, HttpVerbs verb, string? identifier = null)
    {
        if (!response.IsSuccessStatusCode)
        {
            var customMessage = string.Empty;
            switch (verb)
            {
                case HttpVerbs.Post:
                    customMessage = $"Failed to add record {identifier}.";
                    break;
                case HttpVerbs.Delete:
                    customMessage = identifier != null ? $"Failed to delete record {identifier}." : "Failed to delete records.";
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
                    customMessage = $"Successfully added record {identifier}.";
                    break;
                case HttpVerbs.Delete:
                    customMessage = identifier != null ? $"Successfully deleted record {identifier}." : "Successfully deleted records.";
                    break;
            }

            _log.LogInformation(customMessage);
        }
    }
}