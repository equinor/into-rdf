namespace IntoRdf.Public.Models;

public class TransformationDetails
{
    public TransformationDetails(Uri baseUri, Uri predicateBaseUri, List<TargetPathSegment> targetPathSegments, RdfFormat outputFormat)
    {
        BaseUri = baseUri;
        OutputFormat = outputFormat;
        SourcePredicateBaseUri = predicateBaseUri;
        TargetPathSegments = targetPathSegments;
    }

    public Uri BaseUri { get; }
    public RdfFormat OutputFormat { get; }
    public Uri SourcePredicateBaseUri { get; }
    public List<TargetPathSegment> TargetPathSegments { get; }
}