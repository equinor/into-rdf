
namespace Services.RdfServices
{
    public interface IRdfService
    {
        Task<HttpResponseMessage> QueryFusekiAsUser(string server, string query, IEnumerable<string?>? accepts = null);

        Task<HttpResponseMessage> PostToFusekiAsUser(string server, string data, string contentType = "text/turtle");
    }
}