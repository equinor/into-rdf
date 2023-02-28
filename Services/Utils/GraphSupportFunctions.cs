using System.Text;
using VDS.RDF;
using VDS.RDF.Writing;
using VDS.RDF.Query;
using IntoRdf.Public.Models;

namespace IntoRdf.Utils;

internal static class GraphSupportFunctions
{

    internal static string WriteGraphToString(Graph graph, RdfFormat writerType )
    {
        using MemoryStream outputStream = new MemoryStream();
        switch (writerType)
        {
            case RdfFormat.Trig:
                graph.SaveToStream(new StreamWriter(outputStream, Encoding.UTF8), new TriGWriter());
                break;
            case RdfFormat.Jsonld:
                graph.SaveToStream(new StreamWriter(outputStream, Encoding.UTF8), new JsonLdWriter());
                break;
            case RdfFormat.Turtle:
                graph.SaveToStream(new StreamWriter(outputStream, new UTF8Encoding(false)), new CompressingTurtleWriter());
                break;
            default:
                throw new InvalidOperationException($"Unknown RDF writer type {writerType}");   
        }
        return Encoding.UTF8.GetString(outputStream.ToArray());
    }

    internal static Graph LoadGraphFromString(string graphContent)
    {
        var graph = new Graph();
        graph.LoadFromString(graphContent);
        
        return graph;
    }

    internal static string GetAskQuery(TripleContent tripleContent, string name)
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

