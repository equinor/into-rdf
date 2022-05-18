using System;

namespace Doc2Rdf.Library.Models;

public static class RdfCommonTypes
{
    public static Uri CreateCollectionType() => new Uri(RdfPrefixes.Prefix2Uri["prov"] + "Collection");
}