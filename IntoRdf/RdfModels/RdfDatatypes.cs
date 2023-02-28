namespace IntoRdf.RdfModels;

internal class RdfDatatypes
{
    internal static Uri CreateUriDatatype() => new Uri(Public.Utils.PrefixToUri["xsd"] + "anyURI");
    internal static Uri CreateStringDatatype() => new Uri(Public.Utils.PrefixToUri["xsd"] + "string");
    internal static Uri CreateIntegerDatatype() => new Uri(Public.Utils.PrefixToUri["xsd"] + "integer");
    internal static Uri CreateDoubleDatatype() => new Uri(Public.Utils.PrefixToUri["xsd"] + "double");
    internal static Uri CreateDateTimeDatatype() => new Uri(Public.Utils.PrefixToUri["xsd"] + "dateTime");
}   