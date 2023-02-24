namespace IntoRdf.RdfModels;

public class RdfDatatypes
{
    public static Uri CreateUriDatatype() => new Uri(RdfPrefixes.Prefix2Uri["xsd"] + "anyURI");
    public static Uri CreateStringDatatype() => new Uri(RdfPrefixes.Prefix2Uri["xsd"] + "string");
    public static Uri CreateIntegerDatatype() => new Uri(RdfPrefixes.Prefix2Uri["xsd"] + "integer");
    public static Uri CreateDoubleDatatype() => new Uri(RdfPrefixes.Prefix2Uri["xsd"] + "double");
    public static Uri CreateDateTimeDatatype() => new Uri(RdfPrefixes.Prefix2Uri["xsd"] + "dateTime");
}