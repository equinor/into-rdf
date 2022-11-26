using Azure.Storage.Blobs.Models;
using Common.GraphModels;
using Common.ProvenanceModels;
using Common.RevisionTrainModels;
using Common.SpreadsheetModels;
using VDS.RDF;

namespace Services.TransformationServices.SpreadsheetTransformationServices;

public interface ISpreadsheetTransformationService
{
    string Transform(RevisionTrainModel revisionTrain, Stream content);
    string GetDataSource();
}
