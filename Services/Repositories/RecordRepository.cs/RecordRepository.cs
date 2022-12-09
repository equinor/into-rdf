using Common.GraphModels;
using Services.FusekiServices;

namespace Repositories.RecordRepository;

public class RecordRepository : IRecordRepository
{
    private readonly IFusekiService _fusekiService;

    public RecordRepository(IFusekiService fusekiService)
    {
        _fusekiService = fusekiService;
    }

    public async Task Add(string server, ResultGraph record)
    {
        var response = await _fusekiService.AddData(server, record, "text/turtle");

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Failed to add record {record.Name}");
        }
    }
}