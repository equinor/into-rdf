using System.Data;

namespace IntoRdf.RdfModels;

internal static class RdfCommonColumns
{
    internal static DataColumn CreateIdColumn() => new DataColumn("id", typeof(Uri));
    internal static DataColumn CreateHasFacilityId() => new DataColumn(Public.Utils.PrefixToUri["identification"] + "hasFacilityId", typeof(Uri));
    internal static DataColumn CreateHasDocumentProjectId() => new DataColumn(Public.Utils.PrefixToUri["identification"] + "hasDocumentProjectId", typeof(Uri));
    internal static DataColumn CreateHasPlantId() => new DataColumn(Public.Utils.PrefixToUri["identification"] + "hasPlantId", typeof(Uri));
    internal static DataColumn CreateGeneratedAtTime() => new DataColumn(Public.Utils.PrefixToUri["prov"] + "generatedAtTime", typeof(DateTime));
    internal static DataColumn CreateUsed() => new DataColumn(Public.Utils.PrefixToUri["prov"] + "used", typeof(Uri));
    internal static DataColumn CreateHadMember() => new DataColumn(Public.Utils.PrefixToUri["prov"] + "hadMember", typeof(Uri));
    internal static DataColumn CreateWasGeneratedBy() => new DataColumn(Public.Utils.PrefixToUri["prov"] + "wasGeneratedBy", typeof(Uri));
    internal static DataColumn CreateWasRevisionOf() => new DataColumn(Public.Utils.PrefixToUri["prov"] + "wasRevisionOf", typeof(Uri));
    internal static DataColumn CreateWasDerivedFrom() => new DataColumn(Public.Utils.PrefixToUri["prov"] + "wasDerivedFrom", typeof(string));
    internal static DataColumn CreateType() => new DataColumn(RdfCommonProperties.CreateType().AbsoluteUri, typeof(Uri));
    internal static DataColumn CreateFromDataCollection() => new DataColumn(Public.Utils.PrefixToUri["sor"] + "fromDataCollection", typeof(string));
    internal static DataColumn CreateHasSource() => new DataColumn(Public.Utils.PrefixToUri["sor"] + "hasSource", typeof(Uri));
    internal static DataColumn CreateHasSourceType() => new DataColumn(Public.Utils.PrefixToUri["sor"] + "hasSourceType", typeof(Uri));
    internal static DataColumn CreateHasRevisionNumber() => new DataColumn(Public.Utils.PrefixToUri["sor"] + "hasRevisionNumber", typeof(int));
    internal static DataColumn CreateHasRevisionName() => new DataColumn(Public.Utils.PrefixToUri["sor"] + "hasRevisionName", typeof(string));
    internal static DataColumn CreateStartedAtTime() => new DataColumn(Public.Utils.PrefixToUri["sor"] + "startedAtTime", typeof(DateTime));
    internal static DataColumn CreateTransformedBy() => new DataColumn(Public.Utils.PrefixToUri["transformation"] + "transformedBy", typeof(string));
    internal static DataColumn CreateHasDocumentName() => new DataColumn(Public.Utils.PrefixToUri["sor"] + "hasDocumentName", typeof(string));
    internal static DataColumn CreateHasContractNumber() => new DataColumn(Public.Utils.PrefixToUri["sor"] + "hasContractNumber", typeof(string));
    internal static DataColumn CreateHasProjectCode() => new DataColumn(Public.Utils.PrefixToUri["sor"] + "hasProjectCode", typeof(string));
    internal static DataColumn CreateHasDocumentTitle() => new DataColumn(Public.Utils.PrefixToUri["sor"] + "hasDocumentTitle", typeof(string));
}   
