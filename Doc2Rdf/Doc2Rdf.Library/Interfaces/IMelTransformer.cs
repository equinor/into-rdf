using Azure.Storage.Blobs.Models;
using Common.ProvenanceModels;
using Common.SpreadsheetModels;
using System.IO;

namespace Doc2Rdf.Library.Interfaces;

public interface IMelTransformer
{
    string Transform(Provenance provenance, BlobDownloadResult blob);
    string Transform(Stream excelStream, string fileName);
    string Transform(Stream excelStream, SpreadsheetInfo details);
}


