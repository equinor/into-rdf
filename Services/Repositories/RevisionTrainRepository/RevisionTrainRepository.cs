using Common.Constants;
using Common.Exceptions;
using Common.GraphModels;
using Common.Utils;
using Microsoft.Extensions.Logging;
using Services.FusekiServices;
using Services.Utils;
using VDS.RDF;
using VDS.RDF.Query;

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
        await ValidateAndLogResponse(response, HttpVerbs.Post);
    }

    public async Task<string> GetByName(string name)
    {
        await VerifyExistence(TripleContent.Object, name, "name");

        var revisionTrainQuery = GetRevisionTrainByNameQuery(name);
        var response = await _fusekiService.Query(_server, revisionTrainQuery);

        var trainUri = GetTrainUriFromName(name, await response.Content.ReadAsStringAsync());

        return await Get(trainUri);
    }

    public async Task<string> GetByRecord(Uri record)
    {
        await VerifyExistence(TripleContent.Subject, record.AbsoluteUri, "record");

        var revisionTrainQuery = GetRevisionTrainByRecordQuery(record);
        var response = await _fusekiService.Query(_server, revisionTrainQuery);

        var trainUri = GetTrainUriFromRecord(record, await response.Content.ReadAsStringAsync());

        return await Get(trainUri);
    }

    private async Task<string> Get(Uri trainUri)
    {
        var revisionTrainQuery = GetRevisionTrainByUriQuery(trainUri);
        var response = await _fusekiService.Query(_server, revisionTrainQuery);

        await ValidateAndLogResponse(response, HttpVerbs.Get, trainUri.AbsoluteUri);

        return await response.Content.ReadAsStringAsync();
    }

    public async Task<string> GetAll()
    {
        var revisionTrainQuery = GetAllRevisionTrainQuery();
        var response = await _fusekiService.Query(_server, revisionTrainQuery);

        await ValidateAndLogResponse(response, HttpVerbs.Get);

        return await response.Content.ReadAsStringAsync();
    }

    public async Task Delete(string name)
    {
        var response = await _fusekiService.Update(_server, GetDeleteRevisionTrainQuery(name));

        await ValidateAndLogResponse(response, HttpVerbs.Delete, name);
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


    private async Task VerifyExistence(TripleContent tripleContent, string identifier, string? customMessage = null)
    {
        var trainExist = await _fusekiQueryService.Ask(_server, GraphSupportFunctions.GetAskQuery(tripleContent, identifier));

        if (!trainExist)
        {
            var message = "Failed to get revision train ";
            message += customMessage != null ? $"with {customMessage} " : "";
            message += "because it doesn't exist.";

            _log.LogWarning(message);
            throw new ObjectNotFoundException(message);
        }
    }


    private async Task ValidateAndLogResponse(HttpResponseMessage response, HttpVerbs verb, string? identifier = null)
    {
        if (!response.IsSuccessStatusCode)
        {
            var customMessage = string.Empty;
            switch (verb)
            {
                case HttpVerbs.Get:
                    customMessage = identifier != null ? $"Failed to get revision train {identifier}." : "Failed to get revision trains.";
                    break;
                case HttpVerbs.Post:
                    customMessage = $"Failed to add revision train.";
                    break;
                case HttpVerbs.Delete:
                    customMessage = $"Failed to delete revision train {identifier}.";
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
                case HttpVerbs.Get:
                    customMessage = identifier != null ? $"Successfully got revision train {identifier}." : "Successfully got revision trains.";
                    break;
                case HttpVerbs.Post:
                    customMessage = $"Successfully added revision train.";
                    break;
                case HttpVerbs.Delete:
                    customMessage = $"Successfully deleted revision train {identifier}.";
                    break;
            }

            _log.LogInformation(customMessage);
        }
    }

    private string GetDeleteRevisionTrainQuery(string name)
    {
        var queryString = new SparqlParameterizedString();
        queryString.Namespaces.AddNamespace("splinter", new Uri("https://rdf.equinor.com/splinter#"));
        queryString.CommandText =
        @$"
        DELETE
        {{
            ?train ?trainProperty ?obj1 .
            ?context ?contextProperty ?obj2 .
        }}
        WHERE
        {{
            ?train splinter:name @name ;
                (splinter:hasTieContext | splinter:hasSpreadsheetContext | splinter:hasRecord) ?context .

            ?train ?trainProperty ?obj1 .
            ?context ?contextProperty ?obj2 .
        }}
        ";

        queryString.SetLiteral("name", name);
        return queryString.ToString();
    }

    private string GetRevisionTrainByNameQuery(string name)
    {
        var queryString = new SparqlParameterizedString();
        queryString.Namespaces.AddNamespace("splinter", new Uri("https://rdf.equinor.com/splinter#"));
        queryString.CommandText =
        @$"
        CONSTRUCT 
        {{
            ?train splinter:name @name .
        }}
        WHERE
        {{
            ?train splinter:name @name .
        }}
        ";

        queryString.SetLiteral("name", name);
        return queryString.ToString();
    }

    private string GetRevisionTrainByUriQuery(Uri uri)
    {
        var queryString = new SparqlParameterizedString();
        queryString.Namespaces.AddNamespace("splinter", new Uri("https://rdf.equinor.com/splinter#"));
        queryString.CommandText =
        @$"
        CONSTRUCT 
        {{
            @uri ?trainProp ?trainValue .
            ?trainValue ?contextProp ?contextValue . 
        }}
        WHERE
        {{
            @uri a splinter:RevisionTrain ;
                ?trainProp ?trainValue .
            OPTIONAL {{?trainValue ?contextProp ?contextValue .}} 
        }}
        ";

        queryString.SetUri("uri", uri);
        return queryString.ToString();
    }

    private string GetRevisionTrainByRecordQuery(Uri record)
    {
        var queryString = new SparqlParameterizedString();
        queryString.Namespaces.AddNamespace("splinter", new Uri("https://rdf.equinor.com/splinter#"));
        queryString.CommandText =
        @$"
        CONSTRUCT 
        {{
            ?train splinter:hasRecord @record .  
        }}
        WHERE
        {{
            ?train splinter:hasRecord @record . 
        }}
        ";

        queryString.SetUri("record", record);
        return queryString.ToString();
    }

    private string GetAllRevisionTrainQuery()
    {
        var queryString = new SparqlParameterizedString();
        queryString.Namespaces.AddNamespace("splinter", new Uri("https://rdf.equinor.com/splinter#"));
        queryString.CommandText =
        @$"
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

        return queryString.ToString();
    }
}