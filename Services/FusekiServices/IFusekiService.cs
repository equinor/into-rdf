using Common.FusekiModels;
using Common.GraphModels;

namespace Services.FusekiServices
{
    public interface IFusekiService
    {
        Task<FusekiResponse> QueryFusekiResponseAsApp(string server, string sparql);
        Task<List<T>> QueryFusekiResponseAsApp<T>(string server, string sparql) where T : new();
        Task<string> QueryAsApp(string server, string sparql);
        Task<HttpResponseMessage> UpdateAsApp(string server, string sparql);
        Task<HttpResponseMessage> QueryAsUser(string server, string sparql);
        Task<HttpResponseMessage> PostAsApp(string server, ResultGraph turtle, string contentType = "text/turtle");
        Task<HttpResponseMessage> PostAsUser(string server, string turtle, string contentType = "text/turtle");
    }
}