using Common.GraphModels;
using Common.RevisionTrainModels;

namespace Services.RevisionServices;

public interface IRevisionTrainService
{
    Task<string> Add(string revisionTrain);
    Task<string> GetByName(string name);
    Task<string> GetByRecord(Uri record);
    Task<string> GetAll();
    Task Delete(string name);
    ResultGraph CreateRecordContext(RevisionTrainModel train, string revisionName, DateTime revisionDate);
}