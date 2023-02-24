namespace IntoRdf.RdfModels;

public static class RdfCommonProperties
{
    public static Uri CreateSubPropertyOfProperty() => new Uri(RdfPrefixes.Prefix2Uri["rdfs"] + "subPropertyOf");
    public static Uri CreateObjectProperty() => new Uri(RdfPrefixes.Prefix2Uri["owl"] + "ObjectProperty");
    public static Uri CreateDatatypeProperty() => new Uri(RdfPrefixes.Prefix2Uri["owl"] + "DatatypeProperty");
    public static Uri CreateType() => new Uri(RdfPrefixes.Prefix2Uri["rdf"] + "type");
    public static Uri CreateRange() => new Uri(RdfPrefixes.Prefix2Uri["rdfs"] + "range");
    public static Uri CreateDomain() => new Uri(RdfPrefixes.Prefix2Uri["rdfs"] + "domain");
    public static Uri CreateLabel() => new Uri(RdfPrefixes.Prefix2Uri["rdfs"] + "label");
    public static Uri CreateHasPhysicalQuantity() => new Uri(RdfPrefixes.Prefix2Uri["physical"] + "hasPhysicalQuantity");
    public static Uri CreateQuantityQualifiedAs() => new Uri(RdfPrefixes.Prefix2Uri["physical"] + "qualityQuantifiedAs");
    public static Uri CreateDatumUOM() => new Uri(RdfPrefixes.Prefix2Uri["physical"] + "datumUOM");
    public static Uri CreateDatumValue() => new Uri(RdfPrefixes.Prefix2Uri["physical"] + "datumValue");
    public static Uri CreateHasPrefix() => new Uri(RdfPrefixes.Prefix2Uri["sor"] + "hasPrefix");
}