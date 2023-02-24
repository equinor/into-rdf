namespace IntoRdf.RdfModels;

public static class RdfCommonClasses
{
    public static Uri CreateCollectionClass() => new Uri(RdfPrefixes.Prefix2Uri["prov"] + "Collection");
    public static Uri CreateOwlClass() => new Uri(RdfPrefixes.Prefix2Uri["owl"] + "Class");
    public static Uri CreateNamedIndividual() => new Uri(RdfPrefixes.Prefix2Uri["owl"] + "NamedIndividual");
    public static Uri CreateNamespaceClass() => new Uri(RdfPrefixes.Prefix2Uri["sor"] + "Namespace");
    public static Uri CreateNamedGraphClass() => new Uri(RdfPrefixes.Prefix2Uri["sor"] + "NamedGraph");
}