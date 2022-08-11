using Azure.Storage.Blobs.Models;
using Common.ProvenanceModels;
using Common.SpreadsheetModels;

namespace Services.TransformationServices.XMLTransformationServices;

public interface IXMLTransformationService
{
    string Transform(AMlProvenance provenance, BlobDownloadResult blob);
    string Transform(Stream xmlStream);
    string GetDataSource();
}
