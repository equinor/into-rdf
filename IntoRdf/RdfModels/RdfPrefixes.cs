namespace IntoRdf.RdfModels;

public static class RdfPrefixes
{
    internal static Dictionary<Uri, string> Uri2Prefix => Utils.Prefixes.PrefixToUri.ToDictionary(
        (KeyValuePair<string, Uri> pair) => pair.Value,
        (KeyValuePair<string, Uri> pair) => pair.Key);

    internal static string FullForm2PrefixForm(Uri uri)
    {
        var inputUri = uri.AbsoluteUri;
        foreach (var pair in Utils.Prefixes.PrefixToUri)
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