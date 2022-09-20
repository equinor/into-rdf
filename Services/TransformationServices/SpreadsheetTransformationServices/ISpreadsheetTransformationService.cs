using Azure.Storage.Blobs.Models;
using Common.ProvenanceModels;
using Common.SpreadsheetModels;
using VDS.RDF;

namespace Services.TransformationServices.SpreadsheetTransformationServices;

public interface ISpreadsheetTransformationService
{
    string Transform(Provenance provenance, Graph ontology, BlobDownloadResult blob, SpreadsheetDetails details);
    string Transform(Stream excelStream, Graph ontology, string fileName);
    string Transform(Stream excelStream, Graph ontology, SpreadsheetInfo details);
    string GetDataSource();
}
