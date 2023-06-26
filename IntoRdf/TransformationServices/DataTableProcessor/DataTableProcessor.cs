using IntoRdf.Exceptions;
using IntoRdf.Models;
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
                var trimmedData = data.Trim();

                if (matchingConfig == null)
                {
                    if (trimmedData == string.Empty)
                    {
                        processedRow[processedColumnIndex] = null;
                    }
                    else
                    {
                        processedRow[processedColumnIndex] = trimmedData;
                    }

                }
                else
                {
                    var dataUri = CreateUri(transformationDetails.BaseUri, matchingConfig.UriSegment, trimmedData, transformationDetails.CustomEncoding);
                    processedRow[processedColumnIndex] = dataUri;
                }
            }

            processedData.Rows.Add(processedRow);
        }
        return processedData;
    }

    private static Uri? CreateIdUri(TransformationDetails details, DataTable alreadyProcessedData, DataRow inputRow)
    {
        if (details.IdentifierTargetPathSegment == null)
        {
            return new Uri($"{details.BaseUri.AbsoluteUri}{Guid.NewGuid()}");
        }
        else
        {
            var data = inputRow[details.IdentifierTargetPathSegment.Target].ToString();
            if (data == null)
            {
                throw new ArgumentNullException("Cannot find column with name " + details.IdentifierTargetPathSegment.Target);
            }
            var idUri = CreateUri(details.BaseUri, details.IdentifierTargetPathSegment.UriSegment, data, details.CustomEncoding);

            if (idUri == null)
            {
                return null;
            }

            if (ContainsId(alreadyProcessedData, idUri))
            {
                var msg = $"'{details.IdentifierTargetPathSegment.Target}' was specified to be a unique id column, but duplicate: '{data}' found";
                throw new IntoRdfException(msg);
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
            .Select(n =>
            {
                var matchingConfig = transformationDetails.TargetPathSegments.Find(t => t.Target == n);
                var dataType = matchingConfig == null ? typeof(string) : typeof(Uri);
                var uri = CreateUri(transformationDetails.SourcePredicateBaseUri, null, n, transformationDetails.CustomEncoding);

                return new DataColumn(uri?.AbsoluteUri, dataType);
            })
            .ToArray()
        );
        return processedData;
    }

    private static Uri? CreateUri(Uri prefix, string? uriSegment, string data, IDictionary<string, string> customEncoding)
    {
        if (string.IsNullOrEmpty(data))
        {
            return null;
        }

        var slashedUriSegment = string.IsNullOrEmpty(uriSegment) ? "" : $"{uriSegment}/";
        var fullPrefix = new Uri($"{prefix}{slashedUriSegment}");
        return new Uri($"{fullPrefix}{Escape(data, customEncoding)}");
    }

    private static string Escape(string value, IDictionary<string, string> customEncoding)
    {
        var customEscaped = customEncoding.Aggregate(value, (current, pair) => {
            return current.Replace(pair.Key, pair.Value);
        });
        return Uri.EscapeDataString(customEscaped);
    }
}