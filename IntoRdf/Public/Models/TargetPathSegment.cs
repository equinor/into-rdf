namespace IntoRdf.Public.Models;

///<summary>
///Class to aid in the creation of RDF individuals from literals
///</summary>
public class TargetPathSegment
{
    ///<summary> 
    ///Target is the property name of the literal to turn into an individual. 
    ///When transforming a spreadsheet this will typically refer to a column name 
    ///</summary>
    public string Target { get; }
    ///<summary>
    ///The UriSegment is appended to the base uri, to enable the creation of specific namespaces for different individuals.
    ///For instance: http://example.com (baseUri) + animal (uriSegment) + MyDog (from data) returns http://example.com/animal/MyDog
    ///<summary>
    public string UriSegment { get; }
    ///<summary>
    ///Setting the identity flag to true, means that the individuals created from the target become subject for all related triples.
    /// A dataset can maximum contain 1 identity target.
    ///<summary>
    public bool IsIdentity { get; }

    public TargetPathSegment(string target, string segment, bool isIdentity)
    {
        Target = target;
        UriSegment = segment;
        IsIdentity = isIdentity;
    }
}
