using Common.Constants;
using Common.Exceptions;
using Common.GraphModels;
using Common.Utils;
using Microsoft.Extensions.Logging;
using Services.FusekiServices;
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

    public async Task Add(string revisionTrain)
    {
        var response = await _fusekiService.AddData(_server, new ResultGraph(GraphConstants.Default, revisionTrain), "text/turtle");

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var message = $"Failed to add revision train. Failed with status {response.StatusCode} and message {content}";
            _log.LogInformation(message);
            throw new BadGatewayException(message);
        }

        _log.LogInformation("Successfully uploaded revision train");
    }

    public async Task Restore(string train)
    {
        var response = await _fusekiService.AddData(_server, new ResultGraph(GraphConstants.Default, train), "text/turtle");
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new BadGatewayException(
                $"Failed to restore revision train. Failed with status {response.StatusCode} and message {content}");
        }
    }

    public async Task<string> GetByName(string name)
    {
        var trainExist = await _fusekiQueryService.Ask(_server, GraphSupportFunctions.GetAskQuery(TripleContent.Object, name));

        if (!trainExist)
        {
            _log.LogWarning($"Failed to get train with name {name} because it doesn't exist");
            throw new ObjectNotFoundException($"Failed to get train with name {name} because it doesn't exist");
        }

        var revisionTrainQuery = GetRevisionTrainByNameQuery(name);
        var response = await _fusekiService.Query(_server, revisionTrainQuery);

        var trainUri = GetTrainUriFromName(name, await response.Content.ReadAsStringAsync());

        return await Get(trainUri);
    }

    public async Task<string> GetByRecord(Uri record)
    {
        var recordExist = await _fusekiQueryService.Ask(_server, GraphSupportFunctions.GetAskQuery(TripleContent.Subject, record.AbsoluteUri));

        if (!recordExist)
        {
            _log.LogWarning($"Failed to get revision train with record {record} because the record doesn't exist");
            throw new FileNotFoundException($"Failed to get revision train with record {record} because the record doesn't exist");
        }

        var revisionTrainQuery = GetRevisionTrainByRecordQuery(record);
        var response = await _fusekiService.Query(_server, revisionTrainQuery);

        var trainUri = GetTrainUriFromRecord(record, await response.Content.ReadAsStringAsync());

        return await Get(trainUri);
    }

    public async Task<string> Get(Uri trainUri)
    {
        var trainExist = await _fusekiQueryService.Ask(_server, GraphSupportFunctions.GetAskQuery(TripleContent.Subject, trainUri.AbsoluteUri));

        if (!trainExist)
        {
            _log.LogWarning($"Failed to get revision train with record {trainUri} because the record doesn't exist");
            throw new FileNotFoundException($"Failed to get revision train with record {trainUri} because the record doesn't exist");
        }

        var revisionTrainQuery = GetRevisionTrainByUriQuery(trainUri);
        var response = await _fusekiService.Query(_server, revisionTrainQuery);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new BadGatewayException(
                $"Failed to get revision train {trainUri}. Failed with status {response.StatusCode} and message {content}");
        }

        return content;
    }

    public async Task<string> GetAll()
    {
        var revisionTrainQuery = GetAllRevisionTrainQuery();
        var response = await _fusekiService.Query(_server, revisionTrainQuery);

        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new BadGatewayException(
                $"Failed to get revision trains. Failed with status {response.StatusCode} and message {content}");
        }

        return content;
    }

    public async Task Delete(string name)
    {
        var response = await _fusekiService.Update(_server, GetDeleteRevisionTrainQuery(name));

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new BadGatewayException(
                $"Failed to delete revision train {name}. Failed with status {response.StatusCode} and message {content}");
        }
    }

    public async Task AddRecordContext(ResultGraph recordContext)
    {
        var response = await _fusekiService.AddData(_server, recordContext, "text/turtle");

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new BadGatewayException(
                $"Failed to add record context for record {recordContext.Name}. Failed with status {response.StatusCode} and message {content}");
        }
    }

    public async Task DeleteRecordContext(Uri record)
    {
        var response = await _fusekiService.Update(_server, GetDeleteRecordContextQuery(record));

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new BadGatewayException(
                $"Failed to delete record context for record {record.AbsoluteUri}. Failed with status {response.StatusCode} and message {content}");
        }
    }

    private Uri GetTrainUriFromName(string name, string train)
    {
        var graph = new Graph();
        graph.LoadFromString(train);

        var trainNameNode = graph.CreateLiteralNode(name);
        if (trainNameNode == null) { throw new ObjectNotFoundException($"Failed to get train with name {name}"); }

        var trainTriple = graph.GetTriples(trainNameNode).First();
        var trainNode = (UriNode)trainTriple.Subject;

        return trainNode.Uri;
    }

    private Uri GetTrainUriFromRecord(Uri record, string train)
    {
        var graph = new Graph();
        graph.LoadFromString(train);

        var recordUriNode = graph.CreateUriNode(record);
        var trainTriples = graph.GetTriples(recordUriNode);
        if (trainTriples.Count() != 1) { throw new ObjectNotFoundException($"Failed to get record {record}"); }

        var trainNode = (UriNode)trainTriples.First().Subject;

        return trainNode.Uri;
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

    private string GetDeleteRevisionTrainQuery(string name)
    {
        var query =
        @$"
        prefix commonlib: <https://rdf.equinor.com/commonlib/tie#>
        prefix splinter: <https://rdf.equinor.com/splinter#>
        prefix spreadsheet: <https://rdf.equinor.com/splinter/spreadsheet#>

        DELETE
        {{
            ?train ?trainProperty ?obj1 .
            ?context ?contextProperty ?obj2 .
        }}
        WHERE
        {{
            ?train splinter:name '{name}' ;
                (splinter:hasTieContext | splinter:hasSpreadsheetContext | splinter:hasRecord) ?context .

            ?train ?trainProperty ?obj1 .
            ?context ?contextProperty ?obj2 .
        }}
        ";

        return query;
    }

    private string GetRevisionTrainByNameQuery(string name)
    {
        var query =
        @$"
        prefix splinter: <https://rdf.equinor.com/splinter#>

        CONSTRUCT 
        {{
            ?train splinter:name '{name}' .
        }}
        WHERE
        {{
            ?train splinter:name '{name}' .
        }}
        ";

        return query;
    }

    private string GetRevisionTrainByUriQuery(Uri uri)
    {
        var query =
        @$"
        prefix splinter: <https://rdf.equinor.com/splinter#>

        CONSTRUCT 
        {{
            <{uri}> ?trainProp ?trainValue .
            ?trainValue ?contextProp ?contextValue . 
        }}
        WHERE
        {{
            <{uri}> a splinter:RevisionTrain ;
                ?trainProp ?trainValue .
            OPTIONAL {{?trainValue ?contextProp ?contextValue .}} 
        }}
        ";

        return query;
    }

    private string GetRevisionTrainByRecordQuery(Uri record)
    {
        var query =
        @$"
        prefix splinter: <https://rdf.equinor.com/splinter#>

        CONSTRUCT 
        {{
            ?train splinter:hasRecord <{record}> .  
        }}
        WHERE
        {{
            ?train splinter:hasRecord <{record}> . 
        }}
        ";

        return query;
    }

        private string GetAllRevisionTrainQuery()
    {
        var query =
        @$"
        prefix splinter: <https://rdf.equinor.com/splinter#>

        CONSTRUCT 
        {{
            ?train ?trainProp ?trainValue .
            ?trainValue ?contextProp ?contextValue . 
        }}
        WHERE
        {{
            ?train a splinter:RevisionTrain ;
                ?trainProp ?trainValue .
            OPTIONAL {{?trainValue ?contextProp ?contextValue .}} 
        }}
        ";

        return query;
    }
}