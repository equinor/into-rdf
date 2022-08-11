using Azure.Storage.Blobs.Models;
using Common.ProvenanceModels;
using Common.SpreadsheetModels;
using Microsoft.Extensions.Logging;
using Services.ProvenanceServices;
using Services.TransformationServices.XMLTransformationServices.Converters;

namespace Services.TransformationServices.XMLTransformationServices;

public class AmlTransformationService : IXMLTransformationService
{
    private readonly IEnumerable<IProvenanceService> _provenanceServices;
    public AmlTransformationService(IEnumerable<IProvenanceService> provenanceServices, AmlToRdfConverter converter) {
        _provenanceServices = provenanceServices; //On hold until vacation is proper over.

    }
    public string GetDataSource()
    {
        return DataSource.AML();
    }

    public string Transform(AMlProvenance provenance, BlobDownloadResult blob)
    {
        throw new NotImplementedException();
    }

    public string Transform(Stream xmlStream)
    {
        var res = AmlToRdfConverter.Convert(xmlStream);
        return res;
    }
}
