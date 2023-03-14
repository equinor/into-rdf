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
    public TargetPathSegment(string target, string segment)
    {
        Target = target;
        UriSegment = segment;
    }
}
