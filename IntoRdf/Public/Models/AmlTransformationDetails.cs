
namespace IntoRdf.Public.Models
{
    public class AmlTransformationDetails
    {
        public AmlTransformationDetails(Uri baseUri, List<Uri> scopes, List<(string, Uri)> identityCollectionsAndPatternsArgs)
        {
            BaseUri = baseUri;
            Scopes = scopes;
            IdentityCollectionsAndPatternsArgs = identityCollectionsAndPatternsArgs;
        }
        public Uri BaseUri { get; }


        public List<Uri> Scopes { get; } = new List<Uri>();
        public List<(string, Uri)> IdentityCollectionsAndPatternsArgs { get; } = new List<(string, Uri)>();
    }
}
