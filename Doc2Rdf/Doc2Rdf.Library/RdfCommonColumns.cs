using System;
using System.Data;

namespace Doc2Rdf.Library
{
    internal static class RdfCommonColumns
    {
        public static DataColumn CreateIdColumn() => new DataColumn("id", typeof(Uri));

        public static DataColumn CreateHasFacilityId() => new DataColumn(Prefixes.Prefix2Uri["facility"] + "hasFacilityId", typeof(Uri));
        public static DataColumn CreateHasDocumentProjectId() => new DataColumn(Prefixes.Prefix2Uri["facility"] + "hasDocumentProjectId", typeof(Uri));
        public static DataColumn CreateHasPlantId() => new DataColumn(Prefixes.Prefix2Uri["facility"] + "hasPlantId", typeof(Uri));
        public static DataColumn CreateGeneratedAtTime() => new DataColumn(Prefixes.Prefix2Uri["prov"] + "generatedAtTime", typeof(DateTime));
        public static DataColumn CreateUsed() => new DataColumn(Prefixes.Prefix2Uri["prov"] + "used", typeof(Uri));
        public static DataColumn CreateWasDerivedFrom() => new DataColumn(Prefixes.Prefix2Uri["prov"] + "wasDerivedFrom", typeof(Uri));
        public static DataColumn CreateWasGeneratedBy() => new DataColumn(Prefixes.Prefix2Uri["prov"] + "wasGeneratedBy", typeof(Uri));
        public static DataColumn CreateWasRevisionOf() => new DataColumn(Prefixes.Prefix2Uri["prov"] + "wasRevisionOf", typeof(Uri));
        public static DataColumn CreateType() => new DataColumn(Prefixes.Prefix2Uri["rdf"] + "type", typeof(Uri));
        public static DataColumn CreateFromDataCollection() => new DataColumn(Prefixes.Prefix2Uri["sor"] + "fromDataCollection", typeof(string));
        public static DataColumn CreateHasFormat() => new DataColumn(Prefixes.Prefix2Uri["sor"] + "hasFormat", typeof(Uri));
        public static DataColumn CreateHasSource() => new DataColumn(Prefixes.Prefix2Uri["sor"] + "hasSource", typeof(Uri));
        public static DataColumn CreateHasSourceType() => new DataColumn(Prefixes.Prefix2Uri["sor"] + "hasSourceType", typeof(Uri));
        public static DataColumn CreateIsRevision() => new DataColumn(Prefixes.Prefix2Uri["sor"] + "isRevision", typeof(string));
        public static DataColumn CreateStartedAtTime() => new DataColumn(Prefixes.Prefix2Uri["sor"] + "startedAtTime", typeof(DateTime));
        public static DataColumn CreateTransformedBy() => new DataColumn(Prefixes.Prefix2Uri["transformation"] + "transformedBy", typeof(string));
        

        
    }
}