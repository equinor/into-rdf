namespace IntoRdf.Models;

public class TransformationDetails
{
    public TransformationDetails(
        Uri baseUri,
        Uri predicateBaseUri,
        TargetPathSegment? identifierSegment,
        List<TargetPathSegment> targetPathSegments,
        RdfFormat outputFormat,
        IDictionary<string, string>? customEncoding = null)
    {
        BaseUri = baseUri;
        OutputFormat = outputFormat;
        SourcePredicateBaseUri = predicateBaseUri;
        TargetPathSegments = targetPathSegments;
        IdentifierTargetPathSegment = identifierSegment;
        CustomEncoding = customEncoding ?? new Dictionary<string, string>();

    }
    public Uri BaseUri { get; }
    public RdfFormat OutputFormat { get; }
    public Uri SourcePredicateBaseUri { get; }
    public List<TargetPathSegment> TargetPathSegments { get; }
    public TargetPathSegment? IdentifierTargetPathSegment { get; }

    /// <summary>
    /// A list of key values that will cause key-pattern to be replaced with values all places where these patterns are found before creating Uris.
    /// This can be used to mitigate some encoding bugs in dotnetRDF
    /// For example:
    /// {"²", "squared"}
    /// </summary>
    public IDictionary<string, string> CustomEncoding { get; }
}