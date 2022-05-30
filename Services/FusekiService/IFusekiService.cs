
namespace Services.FusekiService
{
    public interface IFusekiService
    {
        Task<string> Query(string server, string sparql);
        Task<HttpResponseMessage> Post(string server, string turtle);
    }
}