using IntoRdf.Public.Models;
using System.Data;
using VDS.RDF;

namespace IntoRdf.TransformationServices;

internal class DataTableProcessor : IDataTableProcessor
{
    public const string SubjectColumnName = "subject";

    public DataTable ProcessDataTable(TransformationDetails transformationDetails, DataTable rawData)
    {
        var idSegment = transformationDetails.IdentifierTargetPathSegment;
        var predicatePrefix = new Uri($"{transformationDetails.SourcePredicateBaseUri}");
        var processedData = InitDataTable(transformationDetails, rawData.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList());

        foreach (DataRow inputRow in rawData.Rows)
        {
            var processedRow = processedData.NewRow();
            processedRow[0] = CreateIdUri(transformationDetails, processedData, inputRow);

            for (var rawColumnIndex = 0; rawColumnIndex < rawData.Columns.Count; rawColumnIndex++)
            {
                var processedColumnIndex = rawColumnIndex + 1;
                var rawColumnHeader = rawData.Columns[rawColumnIndex].ColumnName;
                var matchingConfig = transformationDetails.TargetPathSegments.Find(t => t.Target == rawColumnHeader);
                var data = inputRow[processedColumnIndex - 1].ToString() ?? "";

                if (matchingConfig == null)
                {
                    processedRow[processedColumnIndex] = data;
                }
                else
                {
                    var slashedUriSegment = string.IsNullOrEmpty(matchingConfig.UriSegment) ? "" : $"{matchingConfig.UriSegment}/";
                    var dataUriPrefix = new Uri($"{transformationDetails.BaseUri.AbsoluteUri}{slashedUriSegment}");
                    var dataUri = CreateUri(dataUriPrefix, data);
                    processedRow[processedColumnIndex] = dataUri;
                }
            }

            processedData.Rows.Add(processedRow);
        }
        return processedData;
    }

    private static Uri CreateIdUri(TransformationDetails details, DataTable alreadyProcessedData, DataRow inputRow)
    {
        if (details.IdentifierTargetPathSegment == null)
        {
            return new Uri($"{details.BaseUri.AbsoluteUri}{Guid.NewGuid()}/");
        }
        else
        {
            var uriSegment = details.IdentifierTargetPathSegment.UriSegment;
            var slashedUriSegment = string.IsNullOrEmpty(uriSegment) ? "" : $"{uriSegment}/";
            var idPrefix = new Uri($"{details.BaseUri.AbsoluteUri}{slashedUriSegment}");
            var data = inputRow[details.IdentifierTargetPathSegment.Target].ToString();
            if (data == null)
            {
                throw new ArgumentNullException("Cannot find column with name " + details.IdentifierTargetPathSegment.Target);
            }
            var idUri = CreateUri(idPrefix, data);

            if (ContainsId(alreadyProcessedData, idUri))
            {
                return new Uri($"{idUri.AbsoluteUri}_row={inputRow["id"]}");
            }
            return idUri;
        }

    }

    private static bool ContainsId(DataTable table, Uri id)
    {
        return table.AsEnumerable().Any(row => id.AbsoluteUri.Equals(row.Field<string>(SubjectColumnName)));
    }

    private static DataTable InitDataTable(TransformationDetails transformationDetails, List<string> rawColumnNames)
    {
        var processedData = new DataTable();
        processedData.Columns.Add(SubjectColumnName);
        processedData.Columns.AddRange(rawColumnNames
            .Select(n => {
                var matchingConfig = transformationDetails.TargetPathSegments.Find(t => t.Target == n);
                var dataType = matchingConfig == null ? typeof(string) : typeof(Uri);
                return new DataColumn(
                    CreateUri(transformationDetails.SourcePredicateBaseUri, n).AbsoluteUri, dataType);
            })
            .ToArray()
        );
        return processedData;
    }

    private static Uri CreateUri(Uri prefix, string data)
    {
        return new Uri($"{prefix}{Escape(data)}");
    }

    private static string Escape(string value)
    {
        return Uri.EscapeDataString(value.Trim());
    }
}