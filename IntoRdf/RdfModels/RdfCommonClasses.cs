namespace IntoRdf.RdfModels;

internal static class RdfCommonClasses
{
    internal static Uri CreateCollectionClass() => new Uri(Utils.Prefixes.PrefixToUri["prov"] + "Collection");
    internal static Uri CreateOwlClass() => new Uri(Utils.Prefixes.PrefixToUri["owl"] + "Class");
    internal static Uri CreateNamedIndividual() => new Uri(Utils.Prefixes.PrefixToUri["owl"] + "NamedIndividual");
    internal static Uri CreateNamespaceClass() => new Uri(Utils.Prefixes.PrefixToUri["sor"] + "Namespace");
    internal static Uri CreateNamedGraphClass() => new Uri(Utils.Prefixes.PrefixToUri["sor"] + "NamedGraph");
}