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
            var content = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Failed to add record {record.Name}. Failed with status {response.StatusCode} and message {content}"); 
        }
    }

    public async Task Delete(string server, Uri record)
    {
        var response = await _fusekiService.Update(server, GetDropRecordQuery(record.AbsoluteUri));

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Failed to delete record {record}. Failed with status {response.StatusCode} and message {content}"); 
        }
    }
    public async Task Delete(string server, List<Uri> records)
    {
        string deleteAllQuery = string.Empty;
        foreach (var r in records)
        {
            deleteAllQuery += GetDropRecordQuery(r.AbsoluteUri);
        }

        var response = await _fusekiService.Update(server, deleteAllQuery);

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Failed to delete records. Failed with status {response.StatusCode} and message {content}"); 
        }
    }

    private string GetDropRecordQuery(string record)
    {
        return $"DROP GRAPH <{record}> ;";
    }
}