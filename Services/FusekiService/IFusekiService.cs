
namespace Services.FusekiService
{
    public interface IFusekiService
    {
        Task<FusekiResponse> QueryFusekiResponseAsApp(string server, string sparql);
        Task<FusekiResponse> QueryFusekiResponseAsUser(string server, string sparql);
        Task<string> QueryAsApp(string server, string sparql);
        Task<string> QueryAsUser(string server, string sparql);
        Task<HttpResponseMessage> PostAsApp(string server, string turtle);
        Task<HttpResponseMessage> PostAsUser(string server, string turtle);
    }
}