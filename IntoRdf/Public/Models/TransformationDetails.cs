namespace IntoRdf.Public.Models;

public class TransformationDetails
{
    public TransformationDetails(Uri baseUri, Uri predicateBaseUri, List<TargetPathSegment> targetPathSegments)
    {
        BaseUri = baseUri;
        SourcePredicateBaseUri = predicateBaseUri;
        TargetPathSegments = targetPathSegments;
    }

    public Uri BaseUri { get; }
    public Uri SourcePredicateBaseUri { get; }
    public List<TargetPathSegment> TargetPathSegments { get; }
}