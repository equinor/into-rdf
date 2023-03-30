using System.Data;

namespace IntoRdf.RdfModels;

internal static class RdfCommonColumns
{
    internal static DataColumn CreateIdColumn() => new DataColumn("id", typeof(Uri));
    internal static DataColumn CreateHasFacilityId() => new DataColumn(Utils.Prefixes.PrefixToUri["identification"] + "hasFacilityId", typeof(Uri));
    internal static DataColumn CreateHasDocumentProjectId() => new DataColumn(Utils.Prefixes.PrefixToUri["identification"] + "hasDocumentProjectId", typeof(Uri));
    internal static DataColumn CreateHasPlantId() => new DataColumn(Utils.Prefixes.PrefixToUri["identification"] + "hasPlantId", typeof(Uri));
    internal static DataColumn CreateGeneratedAtTime() => new DataColumn(Utils.Prefixes.PrefixToUri["prov"] + "generatedAtTime", typeof(DateTime));
    internal static DataColumn CreateUsed() => new DataColumn(Utils.Prefixes.PrefixToUri["prov"] + "used", typeof(Uri));
    internal static DataColumn CreateHadMember() => new DataColumn(Utils.Prefixes.PrefixToUri["prov"] + "hadMember", typeof(Uri));
    internal static DataColumn CreateWasGeneratedBy() => new DataColumn(Utils.Prefixes.PrefixToUri["prov"] + "wasGeneratedBy", typeof(Uri));
    internal static DataColumn CreateWasRevisionOf() => new DataColumn(Utils.Prefixes.PrefixToUri["prov"] + "wasRevisionOf", typeof(Uri));
    internal static DataColumn CreateWasDerivedFrom() => new DataColumn(Utils.Prefixes.PrefixToUri["prov"] + "wasDerivedFrom", typeof(string));
    internal static DataColumn CreateType() => new DataColumn(RdfCommonProperties.CreateType().AbsoluteUri, typeof(Uri));
    internal static DataColumn CreateFromDataCollection() => new DataColumn(Utils.Prefixes.PrefixToUri["sor"] + "fromDataCollection", typeof(string));
    internal static DataColumn CreateHasSource() => new DataColumn(Utils.Prefixes.PrefixToUri["sor"] + "hasSource", typeof(Uri));
    internal static DataColumn CreateHasSourceType() => new DataColumn(Utils.Prefixes.PrefixToUri["sor"] + "hasSourceType", typeof(Uri));
    internal static DataColumn CreateHasRevisionNumber() => new DataColumn(Utils.Prefixes.PrefixToUri["sor"] + "hasRevisionNumber", typeof(int));
    internal static DataColumn CreateHasRevisionName() => new DataColumn(Utils.Prefixes.PrefixToUri["sor"] + "hasRevisionName", typeof(string));
    internal static DataColumn CreateStartedAtTime() => new DataColumn(Utils.Prefixes.PrefixToUri["sor"] + "startedAtTime", typeof(DateTime));
    internal static DataColumn CreateTransformedBy() => new DataColumn(Utils.Prefixes.PrefixToUri["transformation"] + "transformedBy", typeof(string));
    internal static DataColumn CreateHasDocumentName() => new DataColumn(Utils.Prefixes.PrefixToUri["sor"] + "hasDocumentName", typeof(string));
    internal static DataColumn CreateHasContractNumber() => new DataColumn(Utils.Prefixes.PrefixToUri["sor"] + "hasContractNumber", typeof(string));
    internal static DataColumn CreateHasProjectCode() => new DataColumn(Utils.Prefixes.PrefixToUri["sor"] + "hasProjectCode", typeof(string));
    internal static DataColumn CreateHasDocumentTitle() => new DataColumn(Utils.Prefixes.PrefixToUri["sor"] + "hasDocumentTitle", typeof(string));
}   
