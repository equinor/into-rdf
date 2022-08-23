using System.Text;
using RDFSharp.Model;
namespace Services.TransformationServices.XMLTransformationServices.Serializers;

public static class RdfSharpNQuadSerializer
{
    public static string TripleToNQuad(this RDFGraph graph)
    {
        var sb = new StringBuilder();
        foreach (var triple in graph)
        {
            if (triple is not null)
            {
                sb.AppendLine(QuadPattern(graph.Context, triple));
            }
        }
        return sb.ToString();
    }

    private static string QuadPattern(Uri namedGraph, RDFTriple triple)
    {
        if (triple.Object is RDFTypedLiteral)
        {
            RDFTypedLiteral typedObj = (RDFTypedLiteral)triple.Object;
            return $"<{triple.Subject.ToString()}> <{triple.Predicate.ToString()}> \"{typedObj.ToString().Replace(Environment.NewLine, " ").Replace("^^", "\"^^<")}> <{namedGraph}> .";

        }
        else if (triple.Object is RDFLiteral)
        {
            return $"<{triple.Subject.ToString()}> <{triple.Predicate.ToString()}> \"{triple.Object.ToString().Replace(Environment.NewLine, " ")}\" <{namedGraph}> .";
        }
        return $"<{triple.Subject.ToString()}> <{triple.Predicate.ToString()}> <{triple.Object.ToString().Replace(Environment.NewLine, " ")}> <{namedGraph}> .";
    }
}