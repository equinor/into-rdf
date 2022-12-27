using Common.RevisionTrainModels;
using VDS.RDF;

namespace Services.TransformationServices.SpreadsheetTransformationServices;

public interface ISpreadsheetTransformationService
{
    Graph Transform(RevisionTrainModel revisionTrain, Stream content);
}
