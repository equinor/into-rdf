namespace IntoRdf.Models;

///<summary>
///Class to aid in the creation of RDF individuals from literals. Should probably be renamed to ColumnConfiuration, but is kept for now to avoid breaking changes
///</summary>
public class TargetPathSegment : IEquatable<TargetPathSegment>
{
    ///<summary>
    ///Target is the property name of the literal to turn into an individual.
    ///When transforming a spreadsheet this will typically refer to a column name
    ///</summary>
    public string Target { get; }
    ///<summary>
    /// PredicateUri will will be used to create the RDF Predicate
    //////<summary>
    public string? PredicateUri { get; }
    ///<summary>
    ///The UriSegment is appended to the base uri, to enable the creation of specific namespaces for different RDF Objects.
    ///For instance: http://example.com (baseUri) + animal (uriSegment) + MyDog (from data) returns http://example.com/animal/MyDog
    ///<summary>
    public string? UriSegment { get; }

    public TargetPathSegment(string target, string? segment, string? predicateUri = null)
    {
        Target = target;
        UriSegment = segment;
        PredicateUri = predicateUri;
    }

    public bool Equals(TargetPathSegment? other)
    {
        if (other is null) return false;

        return
            StringEqual(Target, other.Target) &&
            StringEqual(UriSegment, other.UriSegment) &&
            StringEqual(PredicateUri, other.PredicateUri);
    }

    private static bool StringEqual(string? first, string? second) {
        if (first == null && second == null) return true;
        if (first == null) return false;
        return first.Equals(second, StringComparison.InvariantCultureIgnoreCase);
    }
}
