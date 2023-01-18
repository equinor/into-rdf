namespace Services.FusekiServices
{
    public interface IFusekiService
    {
        Task<HttpResponseMessage> Query(string server, string sparql, IEnumerable<string?>? accepts = null);
        Task<HttpResponseMessage> Update(string server, string sparql);
        Task<HttpResponseMessage> AddData(string server, string graph, string contentType);
    }
}