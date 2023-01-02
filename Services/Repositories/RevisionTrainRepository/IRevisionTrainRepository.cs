using Common.GraphModels;
using VDS.RDF;

namespace Repositories.RevisionTrainRepository;

public interface IRevisionTrainRepository
{
    Task Add(string revisionTrain);
    Task<string> GetByName(string name);
    Task<string> GetByRecord(Uri record);
    Task<string> GetAll();
    Task Delete(string name);
}