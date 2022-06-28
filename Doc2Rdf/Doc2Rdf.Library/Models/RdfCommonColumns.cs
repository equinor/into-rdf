using System;
using System.Data;

namespace Doc2Rdf.Library.Models;

public static class RdfCommonColumns
{
    public static DataColumn CreateIdColumn() => new DataColumn("id", typeof(Uri));
    public static DataColumn CreateHasFacilityId() => new DataColumn(RdfPrefixes.Prefix2Uri["identification"] + "hasFacilityId", typeof(Uri));
    public static DataColumn CreateHasDocumentProjectId() => new DataColumn(RdfPrefixes.Prefix2Uri["identification"] + "hasDocumentProjectId", typeof(Uri));
    public static DataColumn CreateHasPlantId() => new DataColumn(RdfPrefixes.Prefix2Uri["identification"] + "hasPlantId", typeof(Uri));
    public static DataColumn CreateGeneratedAtTime() => new DataColumn(RdfPrefixes.Prefix2Uri["prov"] + "generatedAtTime", typeof(DateTime));
    public static DataColumn CreateUsed() => new DataColumn(RdfPrefixes.Prefix2Uri["prov"] + "used", typeof(Uri));
    public static DataColumn CreateHadMember() => new DataColumn(RdfPrefixes.Prefix2Uri["prov"] + "hadMember", typeof(Uri));
    public static DataColumn CreateWasGeneratedBy() => new DataColumn(RdfPrefixes.Prefix2Uri["prov"] + "wasGeneratedBy", typeof(Uri));
    public static DataColumn CreateWasRevisionOf() => new DataColumn(RdfPrefixes.Prefix2Uri["prov"] + "wasRevisionOf", typeof(Uri));
    public static DataColumn CreateType() => new DataColumn(RdfPrefixes.Prefix2Uri["rdf"] + "type", typeof(Uri));
    public static DataColumn CreateFromDataCollection() => new DataColumn(RdfPrefixes.Prefix2Uri["sor"] + "fromDataCollection", typeof(string));
    public static DataColumn CreateHasSource() => new DataColumn(RdfPrefixes.Prefix2Uri["sor"] + "hasSource", typeof(Uri));
    public static DataColumn CreateHasSourceType() => new DataColumn(RdfPrefixes.Prefix2Uri["sor"] + "hasSourceType", typeof(Uri));
    public static DataColumn CreateHasRevisionNumber() => new DataColumn(RdfPrefixes.Prefix2Uri["sor"] + "hasRevisionNumber", typeof(int));
    public static DataColumn CreateHasRevisionName() => new DataColumn(RdfPrefixes.Prefix2Uri["sor"] + "hasRevisionName", typeof(string));
    public static DataColumn CreateStartedAtTime() => new DataColumn(RdfPrefixes.Prefix2Uri["sor"] + "startedAtTime", typeof(DateTime));
    public static DataColumn CreateTransformedBy() => new DataColumn(RdfPrefixes.Prefix2Uri["transformation"] + "transformedBy", typeof(string));
}
