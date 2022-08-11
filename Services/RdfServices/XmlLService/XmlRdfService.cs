using Common.ProvenanceModels;
using Services.TransformationServices.XMLTransformationServices;

namespace Services.RdfServices.XmlServives;

public class XmlRdfService : IXmlRdfService
{
    private readonly IEnumerable<IXMLTransformationService> _xmlTransformationService;
    public XmlRdfService(IEnumerable<IXMLTransformationService> xmlTransformationService) {
        _xmlTransformationService = xmlTransformationService;
    }
    //Generates N-Quad representation of RDF in order to place triples in a named graph.
    public async Task<string> ConvertAMLToRdf(Stream stream)
    {
        await Task.Delay(0);
        stream.Position = 0;
        var transformer = _xmlTransformationService.FirstOrDefault(t => t.GetDataSource() == DataSource.AML()) ??
        throw new ArgumentException($"A transformer of type {DataSource.AML()} is not available to RdfService");
        return transformer.Transform(stream);
    }
}