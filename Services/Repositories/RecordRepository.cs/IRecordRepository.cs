using Common.GraphModels;

namespace Repositories.RecordRepository
{
    public interface IRecordRepository
    {
        Task Add(string server, ResultGraph record);
    }
}

