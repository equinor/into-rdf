using Common.Exceptions;
using Common.GraphModels;
using Common.Utils;
using Microsoft.Extensions.Logging;
using Services.FusekiServices;
using Services.Utils;
using VDS.RDF;

namespace Repositories.RevisionTrainRepository;

public class RevisionTrainRepository : IRevisionTrainRepository
{
    private readonly IFusekiService _fusekiService;
    private readonly IFusekiQueryService _fusekiQueryService;
    private readonly ILogger<RevisionTrainRepository> _log;
    private readonly string _server = ServerKeys.Main;

    public RevisionTrainRepository(IFusekiService fusekiService, IFusekiQueryService fusekiQueryService, ILogger<RevisionTrainRepository> log)
    {
        _fusekiService = fusekiService;
        _fusekiQueryService = fusekiQueryService;
        _log = log;
    }

    public Task Add(Stream train)
    {
        throw new NotImplementedException();
    }

    public Task Restore(string train)
        {
        throw new NotImplementedException();
    }
    public Task<Graph> GetByName(string name)
        {
        throw new NotImplementedException();
    }
    public Task<Graph> GetByRecord(Uri record)
        {
        throw new NotImplementedException();
    }
    public Task<Graph> Get(Uri id)
        {
        throw new NotImplementedException();
    }
    public Task<Graph> GetAll()
        {
        throw new NotImplementedException();
    }
    public Task Delete()
        {
        throw new NotImplementedException();
    }
    public async Task AddRecordContext(ResultGraph recordContext)
    {
        var response = await _fusekiService.AddData(_server, recordContext, "text/turtle");

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Failed to add record context for record {recordContext.Name}");
        }
    }

    public async Task DeleteRecordContext(Uri record)
    {
        var recordExist = await _fusekiQueryService.Ask(_server, GraphSupportFunctions.GetAskQuery(TripleContent.Subject, record.AbsoluteUri));

        if (!recordExist)
        {
            var message = $"Failed to delete record context because record {record} doesn't exist";
            _log.LogWarning(message);
            throw new ObjectNotFoundException(message);
        }

        var response = await _fusekiService.Update(_server, GetDeleteRecordContextQuery(record));

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Failed to delete record context for record {record}");
        }
    }

        private string GetDeleteRecordContextQuery(Uri recordContextUri)
    {
        var query = 
        @$"
        prefix splinter: <https://rdf.equinor.com/splinter#>
        DELETE 
        {{
            ?train splinter:hasRecord <{recordContextUri}> .
            <{recordContextUri}> ?p ?o .
        }}
        WHERE 
        {{
            ?train splinter:hasRecord <{recordContextUri}> .
            <{recordContextUri}> ?p ?o .
        }}";

        return query;
    }
}