
namespace Repositories.RecordRepository;

public interface IRecordRepository
{
    Task Add(string server, string record, string contentType);
    Task Delete(string server, Uri record);
}

