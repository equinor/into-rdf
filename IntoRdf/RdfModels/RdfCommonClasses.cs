namespace IntoRdf.RdfModels;

internal static class RdfCommonClasses
{
    internal static Uri CreateCollectionClass() => new Uri(Public.Utils.PrefixToUri["prov"] + "Collection");
    internal static Uri CreateOwlClass() => new Uri(Public.Utils.PrefixToUri["owl"] + "Class");
    internal static Uri CreateNamedIndividual() => new Uri(Public.Utils.PrefixToUri["owl"] + "NamedIndividual");
    internal static Uri CreateNamespaceClass() => new Uri(Public.Utils.PrefixToUri["sor"] + "Namespace");
    internal static Uri CreateNamedGraphClass() => new Uri(Public.Utils.PrefixToUri["sor"] + "NamedGraph");
}