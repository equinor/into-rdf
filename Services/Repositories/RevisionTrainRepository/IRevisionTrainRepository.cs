using Common.GraphModels;
using VDS.RDF;

namespace Repositories.RevisionTrainRepository;

public interface IRevisionTrainRepository
{
    Task Add(string revisionTrain);
    Task Restore(string train);
    Task<string> GetByName(string name);
    Task<string> GetByRecord(Uri record);
    Task<string> Get(Uri id);
    Task<string> GetAll();
    Task Delete(string name);
    Task AddRecordContext(ResultGraph recordContext);
    Task DeleteRecordContext(Uri record);
}