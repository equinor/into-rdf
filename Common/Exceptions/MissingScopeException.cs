using System.Net;
using System.Runtime.Serialization;

namespace Common.Exceptions;

[Serializable]
public class MissingScopeException : Exception
{
    public string FusekiName;

    public MissingScopeException(string fusekiName) => FusekiName = fusekiName;
    public MissingScopeException(string message, string fusekiName) : base(message) => FusekiName = fusekiName;

    public MissingScopeException(string message, Exception inner, string fusekiName) : base(message, inner)
        => FusekiName = fusekiName;


    protected MissingScopeException(
        SerializationInfo info,
        StreamingContext context,
        string fusekiName) : base(info, context) => FusekiName = fusekiName;

    public static HttpStatusCode StatusCode() => HttpStatusCode.Forbidden;
}