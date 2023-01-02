using System.Text;
using VDS.RDF;
using VDS.RDF.Writing;
using VDS.RDF.Query;

namespace Common.Utils;

public static class GraphSupportFunctions
{
    public static string WriteGraphToString(Graph graph)
    {
        using MemoryStream outputStream = new MemoryStream();
        graph.SaveToStream(new StreamWriter(outputStream, Encoding.UTF8), new CompressingTurtleWriter());
        
        return Encoding.UTF8.GetString(outputStream.ToArray());
    }

    public static Graph LoadGraphFromString(string graphContent)
    {
        var graph = new Graph();
        graph.LoadFromString(graphContent);
        
        return graph;
    }

    public static string GetAskQuery(TripleContent tripleContent, string name)
    {
        var queryString = new SparqlParameterizedString();
        queryString.Namespaces.AddNamespace("splinter", new Uri("https://rdf.equinor.com/splinter#"));

        switch (tripleContent)
        {
            case TripleContent.Subject:
                queryString.CommandText = "ASK { @name ?p ?o .}";
                queryString.SetUri("name", new Uri(name));
                break;
            case TripleContent.Predicate:
                queryString.CommandText = "ASK { ?s @name ?o .}";
                queryString.SetUri("name", new Uri(name));
                break;
            case TripleContent.Object:
                queryString.CommandText = "ASK { ?s ?p @name .}";
                if (Uri.IsWellFormedUriString(name, UriKind.Absolute))
                {
                    queryString.SetUri("name", new Uri(name));
                }
                else 
                {
                    queryString.SetLiteral("name", name);
                }
                break;
            default:
                throw new InvalidOperationException($"Failed to create ASK query for {name}");
        }
        
        return queryString.ToString();
    }
}

