using Azure.Storage.Blobs.Models;
using Common.GraphModels;
using Common.ProvenanceModels;
using Common.SpreadsheetModels;
using VDS.RDF;

namespace Services.TransformationServices.SpreadsheetTransformationServices;

public interface ISpreadsheetTransformationService
{
    ResultGraph Transform(Provenance provenance, Graph ontology, BlobDownloadResult blob, SpreadsheetDetails details);
    ResultGraph Transform(Stream excelStream, Graph ontology, string fileName);
    ResultGraph Transform(Stream excelStream, Graph ontology, SpreadsheetInfo details);
    string GetDataSource();
}
