using System.Text;
using VDS.RDF;
using VDS.RDF.Writing;
using IntoRdf.Models;

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
}

