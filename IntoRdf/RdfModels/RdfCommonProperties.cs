namespace IntoRdf.RdfModels;

internal static class RdfCommonProperties
{
    internal static Uri CreateSubPropertyOfProperty() => new Uri(Public.Utils.PrefixToUri["rdfs"] + "subPropertyOf");
    internal static Uri CreateObjectProperty() => new Uri(Public.Utils.PrefixToUri["owl"] + "ObjectProperty");
    internal static Uri CreateDatatypeProperty() => new Uri(Public.Utils.PrefixToUri["owl"] + "DatatypeProperty");
    internal static Uri CreateType() => new Uri(Public.Utils.PrefixToUri["rdf"] + "type");
    internal static Uri CreateRange() => new Uri(Public.Utils.PrefixToUri["rdfs"] + "range");
    internal static Uri CreateDomain() => new Uri(Public.Utils.PrefixToUri["rdfs"] + "domain");
    internal static Uri CreateLabel() => new Uri(Public.Utils.PrefixToUri["rdfs"] + "label");
    internal static Uri CreateHasPhysicalQuantity() => new Uri(Public.Utils.PrefixToUri["physical"] + "hasPhysicalQuantity");
    internal static Uri CreateQuantityQualifiedAs() => new Uri(Public.Utils.PrefixToUri["physical"] + "qualityQuantifiedAs");
    internal static Uri CreateDatumUOM() => new Uri(Public.Utils.PrefixToUri["physical"] + "datumUOM");
    internal static Uri CreateDatumValue() => new Uri(Public.Utils.PrefixToUri["physical"] + "datumValue");
    internal static Uri CreateHasPrefix() => new Uri(Public.Utils.PrefixToUri["sor"] + "hasPrefix");
}