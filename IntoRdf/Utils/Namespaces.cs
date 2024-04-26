public struct Namespaces
{
    public struct Prov
    {
        public const string BaseUrl = "http://www.w3.org/ns/prov#";
        public const string WasGeneratedBy = $"{BaseUrl}wasGeneratedBy";
        public const string WasAssociatedWith = $"{BaseUrl}wasAssociatedWith";
    }

    public struct Rdfs
    {
        public const string BaseUrl = "http://www.w3.org/2000/01/rdf-schema#";
        public const string Comment = $"{BaseUrl}comment";
    }
}

