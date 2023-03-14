namespace IntoRdf.Public.Models;
using IntoRdf.Public.Exceptions;

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
            if (targetPathSegments.Contains(IdentifierSegment))
            {
                throw new IntoRdfException("Same TargetPathSegment Object passed as both Identity and related Individual. A property cannot be described both as and Identity and a different Individual.");
            }
            if(targetPathSegments.Where(e => e.Equals(IdentifierSegment)).Count() > 0) {
                throw new IntoRdfException("Equal TargetPathSegment objects passed as both Identity and related Individual. A property cannot be described both as and Identity and a different Individual.");
            }
        }
    }
    public Uri BaseUri { get; }
    public RdfFormat OutputFormat { get; }
    public Uri SourcePredicateBaseUri { get; }
    public List<TargetPathSegment> TargetPathSegments { get; }
    public TargetPathSegment? IdentifierTargetPathSegment { get; }

}