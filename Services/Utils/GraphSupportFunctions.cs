using System.Text;
using VDS.RDF;
using VDS.RDF.Writing;
using Common.Utils;

namespace Services.Utils;

public static class GraphSupportFunctions
{
    public static string WriteGraphToString(Graph graph)
    {
        using MemoryStream outputStream = new MemoryStream();
        
        graph.SaveToStream(new StreamWriter(outputStream, Encoding.UTF8), new CompressingTurtleWriter());
        return Encoding.UTF8.GetString(outputStream.ToArray());
    }

    public static string GetAskQuery(TripleContent tripleContent, string name)
    {
        var pattern = string.Empty;
        switch (tripleContent)
        {
            case TripleContent.Subject:
                pattern = $"<{name}> ?p ?o .";
                break;
            case TripleContent.Predicate:
                pattern = $"?s <{name}> ?o .";
                break;
            case TripleContent.Object:
                pattern = $"?s ?p '{name}' .";
                break;
            default:
                throw new InvalidOperationException($"Failed to create ASK query for {name}");
        }

        var query = 
        @$"
        prefix splinter: <https://rdf.equinor.com/splinter#>
        ASK 
        {{
            {pattern}
        }}";

        return query;
    }
}

