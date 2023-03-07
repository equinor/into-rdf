namespace IntoRdf.Public
{
    public class Utils
    {
        public static Dictionary<string, Uri> PrefixToUri = new Dictionary<string, Uri>()
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
    }
}
