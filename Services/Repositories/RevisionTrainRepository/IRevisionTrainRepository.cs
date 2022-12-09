using Common.GraphModels;
using VDS.RDF;

namespace Repositories.RevisionTrainRepository;

public interface IRevisionTrainRepository
{
    Task Add(Stream train);
    Task Restore(string train);
    Task<Graph> GetByName(string name);
    Task<Graph> GetByRecord(Uri record);
    Task<Graph> Get(Uri id);
    Task<Graph> GetAll();
    Task Delete();
    Task AddRecordContext(ResultGraph recordContext);
    Task DeleteRecordContext(Uri record);
}