using VDS.RDF;

namespace IntoRdf.Services.TransformationServices.RecordService;

internal class RecordTransformationService : IRecordTransformationService
{
    public Graph CreateProtoRecord(Uri uri, Graph graph)
    {
        var subjects = graph.Triples.Select(triple => triple.Subject).ToList();
        
        graph.BaseUri = uri;
        graph.NamespaceMap.AddNamespace("rec", new Uri("https://rdf.equinor.com/ontology/record/"));
        graph.NamespaceMap.AddNamespace("rdf", new Uri("http://www.w3.org/1999/02/22-rdf-syntax-ns#"));
        
        var record = graph.CreateUriNode(uri);
        graph.Assert(record, graph.CreateUriNode("rdf:type"), graph.CreateUriNode("rec:Record"));

        var describes = graph.CreateUriNode("rec:describes");

        foreach (var subject in subjects)
        {
            graph.Assert(record, describes, subject);
        }

        return graph;
    }
}