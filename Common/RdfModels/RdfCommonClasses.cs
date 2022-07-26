using System;

namespace Common.RdfModels;

public static class RdfCommonClasses
{
    public static Uri CreateCollectionClass() => new Uri(RdfPrefixes.Prefix2Uri["prov"] + "Collection");
}