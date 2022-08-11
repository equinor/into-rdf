
namespace Services.RdfServices.XmlServives ;

public interface IXmlRdfService {
            Task<string> ConvertAMLToRdf(Stream stream);
}