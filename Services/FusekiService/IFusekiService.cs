
namespace Services.FusekiService
{
    public interface IFusekiService
    {
        Task<FusekiResponse> QueryFusekiServer(string server, string sparql);
        Task<string> Query(string server, string sparql);
        Task<HttpResponseMessage> Post(string server, string turtle);
    }
}