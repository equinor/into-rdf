using Azure.Storage.Blobs.Models;
using Common.ProvenanceModels;
using Common.SpreadsheetModels;

namespace Services.TransformationServices.SpreadsheetTransformationServices;

public interface ISpreadsheetTransformationService
{
    string Transform(Provenance provenance, BlobDownloadResult blob);
    string Transform(Stream excelStream, string fileName);
    string Transform(Stream excelStream, SpreadsheetInfo details);
    string GetDataSource();
}
