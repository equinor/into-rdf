namespace Common.RdfModels;

public static class RdfPrefixes
{
    public static Dictionary<string, Uri> Prefix2Uri = new Dictionary<string, Uri>()
    {
        {"xsd", new Uri("http://www.w3.org/2001/XMLSchema#")},
        {"rdf", new Uri("http://www.w3.org/1999/02/22-rdf-syntax-ns#")},
        {"rdfs", new Uri("http://www.w3.org/2000/01/rdf-schema#")},
        {"owl", new Uri("http://www.w3.org/2002/07/owl#")},
        {"prov", new Uri("http://www.w3.org/ns/prov#")},
        {"equinor", new Uri("https://rdf.equinor.com/")},
        {"source", new Uri("https://rdf.equinor.com/source/")},
        {"transformation", new Uri("https://rdf.equinor.com/ontology/transformation#")},
        {"identification", new Uri("https://rdf.equinor.com/ontology/facility-identification/v1#")},
        {"identifier", new Uri("https://rdf.equinor.com/data/facility-identification/")},
        {"sor", new Uri("https://rdf.equinor.com/ontology/sor#")},
        {"pca", new Uri("https://rdf.equinor.com/ontology/pca#")},
        {"physical", new Uri("https://rdf.equinor.com/ontology/physical/v1#")} 
    };

    public static Dictionary<Uri, string> Uri2Prefix => Prefix2Uri.ToDictionary(
        pair => pair.Value,
        pair => pair.Key);

    public static string FullForm2PrefixForm(Uri uri)
    {
        var inputUri = uri.AbsoluteUri;
        foreach (var pair in Prefix2Uri)
        {
            var prefixUri = pair.Value;
            if (inputUri.StartsWith(prefixUri.AbsoluteUri))
            {
                return inputUri.Replace(prefixUri.AbsoluteUri, Uri2Prefix[prefixUri] + ":");
            }
        }
        throw new Exception($"Could not find prefix for {uri.AbsoluteUri}");
    }
}