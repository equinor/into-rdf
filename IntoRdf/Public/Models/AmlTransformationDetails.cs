
namespace IntoRdf.Models
{
    public class AmlTransformationDetails
    {
        public AmlTransformationDetails(Uri baseUri, List<(string, Uri)> identityCollectionsAndPatternsArgs)
        {
            BaseUri = baseUri;
            IdentityCollectionsAndPatternsArgs = identityCollectionsAndPatternsArgs;
        }
        public Uri BaseUri { get; }

        public List<(string, Uri)> IdentityCollectionsAndPatternsArgs { get; } = new List<(string, Uri)>();
    }
}
