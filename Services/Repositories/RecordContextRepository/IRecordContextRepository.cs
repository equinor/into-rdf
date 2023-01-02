using Common.GraphModels;

namespace Repositories.RecordContextRepository;

public interface IRecordContextRepository
{
    Task Add(ResultGraph recordContext);
    Task Delete(Uri record);
}