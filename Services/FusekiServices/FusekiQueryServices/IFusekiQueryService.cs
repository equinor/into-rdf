namespace Services.FusekiServices;

public interface IFusekiQueryService
{
    Task<bool> Ask(string server, string query);
    Task<List<T>> Select<T>(string server, string sparql) where T : new();
}