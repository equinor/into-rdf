namespace IntoRdf.Public.Models;

public class TransformationDetails
{
    public TransformationDetails(Uri baseUri, Uri predicateBaseUri, TargetPathSegment? IdentifierSegment, List<TargetPathSegment> targetPathSegments, RdfFormat outputFormat)
    {
        BaseUri = baseUri;
        OutputFormat = outputFormat;
        SourcePredicateBaseUri = predicateBaseUri;
        TargetPathSegments = targetPathSegments;
        if (IdentifierSegment is not null)
        {
            IdentifierTargetPathSegment = IdentifierSegment;
            targetPathSegments.Add(IdentifierSegment);
        }
    }
    public Uri BaseUri { get; }
    public RdfFormat OutputFormat { get; }
    public Uri SourcePredicateBaseUri { get; }
    public List<TargetPathSegment> TargetPathSegments { get; }
    public TargetPathSegment? IdentifierTargetPathSegment { get; }
}