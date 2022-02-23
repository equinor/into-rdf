using System;
using System.Collections.Generic;
using System.Linq;

namespace Doc2Rdf.Library
{
    internal static class Prefixes
    {
        public static Dictionary<string, Uri> Prefix2Uri = new Dictionary<string, Uri>
        {
            { "xsd", new Uri("http://www.w3.org/2001/XMLSchema#") },
            { "rdf", new Uri("http://www.w3.org/1999/02/22-rdf-syntax-ns#") },
            { "rdfs", new Uri("http://www.w3.org/2000/01/rdf-schema#") },
            { "owl", new Uri("http://www.w3.org/2002/07/owl#") },
            { "prov", new Uri("http://www.w3.org/ns/prov#") },
            { "ext", new Uri("http://rdf.equinor.com/ext/") },
            { "transformation", new Uri("http://rdf.equinor.com/ext/transformation#") },
            { "project", new Uri("http://rdf.equinor.com/ext/project#") },
            { "facility", new Uri("http://rdf.equinor.com/ontology/facility#") },
            { "sor", new Uri("http://rdf.equinor.com/ontology/sor#") },
            { "mel", new Uri("http://rdf.equinor.com/ontology/mel#") },
            { "melraw", new Uri("http://rdf.equinor.com/raw/melexcel#") },
            { "capacity", new Uri("http://rdf.equinor.com/ontology/capacity#") },
            { "capacityraw", new Uri("http://rdf.equinor.com/raw/capacityexcel#") },
            { "alarm", new Uri("http://rdf.equinor.com/ontology/alarm#") },
            { "alarmraw", new Uri("http://rdf.equinor.com/raw/alarmexcel#") },
            { "shipweight", new Uri("http://rdf.equinor.com/ontology/shipweight#") },
            { "shipweightraw", new Uri("http://rdf.equinor.com/raw/shipweight#") }
        };

        public static Dictionary<Uri, string> Uri2Prfix => Prefix2Uri.ToDictionary(
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
                    return inputUri.Replace(prefixUri.AbsoluteUri, Uri2Prfix[prefixUri] + ":");
                }
            }
            throw new Exception($"Could not find prefix for {uri.AbsoluteUri}");
        }
    }
}
