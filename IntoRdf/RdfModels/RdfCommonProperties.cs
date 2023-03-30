namespace IntoRdf.RdfModels;

internal static class RdfCommonProperties
{
    internal static Uri CreateSubPropertyOfProperty() => new Uri(Utils.Prefixes.PrefixToUri["rdfs"] + "subPropertyOf");
    internal static Uri CreateObjectProperty() => new Uri(Utils.Prefixes.PrefixToUri["owl"] + "ObjectProperty");
    internal static Uri CreateDatatypeProperty() => new Uri(Utils.Prefixes.PrefixToUri["owl"] + "DatatypeProperty");
    internal static Uri CreateType() => new Uri(Utils.Prefixes.PrefixToUri["rdf"] + "type");
    internal static Uri CreateRange() => new Uri(Utils.Prefixes.PrefixToUri["rdfs"] + "range");
    internal static Uri CreateDomain() => new Uri(Utils.Prefixes.PrefixToUri["rdfs"] + "domain");
    internal static Uri CreateLabel() => new Uri(Utils.Prefixes.PrefixToUri["rdfs"] + "label");
    internal static Uri CreateHasPhysicalQuantity() => new Uri(Utils.Prefixes.PrefixToUri["physical"] + "hasPhysicalQuantity");
    internal static Uri CreateQuantityQualifiedAs() => new Uri(Utils.Prefixes.PrefixToUri["physical"] + "qualityQuantifiedAs");
    internal static Uri CreateDatumUOM() => new Uri(Utils.Prefixes.PrefixToUri["physical"] + "datumUOM");
    internal static Uri CreateDatumValue() => new Uri(Utils.Prefixes.PrefixToUri["physical"] + "datumValue");
    internal static Uri CreateHasPrefix() => new Uri(Utils.Prefixes.PrefixToUri["sor"] + "hasPrefix");
}