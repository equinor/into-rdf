
namespace Services.RdfServices
{
    public interface IRdfService
    {
        Task<HttpResponseMessage> QueryFusekiAsUser(string server, string query);

        Task<HttpResponseMessage> PostToFusekiAsUser(string server, string data, string contentType);
    }
}