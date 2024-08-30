using IntoRdf.Models;
using VDS.RDF;

namespace IntoRdf.Nuget.Tests;

internal class Utils
{
    private readonly Graph _graph;

    public Utils(string testFile, SpreadsheetDetails spreadsheetDetails, TransformationDetails transformationDetails)
    {
        using var stream = File.Open(testFile, FileMode.Open, FileAccess.Read);
        var turtle = new TransformerService().TransformSpreadsheet(spreadsheetDetails, transformationDetails, stream);
        _graph = new Graph();
        _graph.LoadFromString(turtle);
    }

    public List<Triple> GetTriples()
    {
        return _graph.Triples.ToList();
    }
}