using Common.GraphModels;

namespace Repositories.RecordRepository
{
    public interface IRecordRepository
    {
        Task Add(string server, ResultGraph record);
        Task Delete(string server, Uri record);
        Task Delete(string server, List<Uri> records);
    }
}

