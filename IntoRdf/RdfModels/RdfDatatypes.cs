namespace IntoRdf.RdfModels;

internal class RdfDatatypes
{
    internal static Uri CreateUriDatatype() => new Uri(Utils.Prefixes.PrefixToUri["xsd"] + "anyURI");
    internal static Uri CreateStringDatatype() => new Uri(Utils.Prefixes.PrefixToUri["xsd"] + "string");
    internal static Uri CreateIntegerDatatype() => new Uri(Utils.Prefixes.PrefixToUri["xsd"] + "integer");
    internal static Uri CreateDoubleDatatype() => new Uri(Utils.Prefixes.PrefixToUri["xsd"] + "double");
    internal static Uri CreateDateTimeDatatype() => new Uri(Utils.Prefixes.PrefixToUri["xsd"] + "dateTime");
}   