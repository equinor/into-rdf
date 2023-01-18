using Common.Utils;
using Repositories.RecordRepository;
using VDS.RDF;

namespace Services.RecordServices;

public class RecordService : IRecordService
{
    private readonly IRecordRepository _recordRepository;

    public RecordService(
        IRecordRepository recordRepository
        )
    {
        _recordRepository = recordRepository;
    }

    public async Task Add(int cursor, Stream record, string contentType)
    {
        using StreamReader stream = new StreamReader(record);
        var recordAsString = await stream.ReadToEndAsync(); 

        await _recordRepository.Add(ServerKeys.Main, recordAsString, contentType);
    }

    public async Task Delete(Uri record)
    {
        await _recordRepository.Delete(ServerKeys.Main, record);
    }
}