using Common.TransformationModels;
using Common.RevisionTrainModels;
using VDS.RDF;

namespace Controller.RecordController;
public interface IRecordController
{
    Task<string> Add(RecordInputModel recordInput);
    Task Delete(Uri record);
}