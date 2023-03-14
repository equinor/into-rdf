using IntoRdf.Public.Models;
using IntoRdf.RdfModels;
using System.Data;

namespace IntoRdf.TransformationServices.RdfTableBuilderServices;

internal class ExcelRdfTableBuilderService : IExcelRdfTableBuilderService
{
    private DataTable _dataTable;

    public ExcelRdfTableBuilderService()
    {
        _dataTable = new DataTable();
    }

    public DataTable GetInputDataTable(TransformationDetails transformationDetails, DataTable inputData)
    {
        _dataTable = new DataTable();
        CreateInputDataSchema(transformationDetails, inputData.Columns);
        AddInputDataRows(transformationDetails, inputData);

        return _dataTable;
    }

    private void CreateInputDataSchema(TransformationDetails transformationSettings, DataColumnCollection columns)
    {
        var idColumn = RdfCommonColumns.CreateIdColumn();
        _dataTable.Columns.Add(idColumn);
        _dataTable.PrimaryKey = new DataColumn[] { idColumn };

        var dataUri = $"{transformationSettings.SourcePredicateBaseUri ?? transformationSettings.BaseUri}";

        foreach (DataColumn column in columns)
        {
            //For excel input, row numbers are temporarily stored in an id column. The row number is 
            //used as a row uri, but the literal value is taken away again when creating the rdfDataTables.
            if (column.ColumnName == "id")
            {
                continue;
            }

            _dataTable.Columns.Add(dataUri + column.ColumnName, typeof(string));
        }
    }

    private void AddInputDataRows(TransformationDetails transformationDetails, DataTable inputData)
    {
        const int NumberOfFixedColumns = 1;

        var targetIdColumn = GetIdentificationColumn(transformationDetails, inputData.Columns);
        var pathSegment = String.IsNullOrEmpty(targetIdColumn.UriSegment) ? "" : $"{targetIdColumn.UriSegment}/"; 
        var dataCollectionUri = new Uri($"{transformationDetails.BaseUri.AbsoluteUri}{pathSegment}");

        foreach (DataRow row in inputData.Rows)
        {
            var itemString = EscapeSlashes($"{row[targetIdColumn.Target]}");
            var itemUri = new Uri($"{dataCollectionUri.AbsoluteUri}{itemString}");
            var existingId = _dataTable.AsEnumerable().Any(row => itemUri.ToString() == row.Field<Uri>("id")?.ToString());

            if (existingId)
            {
                itemUri = new Uri($"{itemUri.AbsoluteUri}_row={row["id"]}");
            }

            var dataRow = _dataTable.NewRow();
            dataRow[0] = itemUri;

            //Row number, adjusted for the start row, was added to the input data as column 0 and is
            //skipped here.
            for (var columnIndex = 1; columnIndex < inputData.Columns.Count; columnIndex++)
            {
                dataRow[columnIndex - 1 + NumberOfFixedColumns] = row[columnIndex];
            }

            _dataTable.Rows.Add(dataRow);
        }
    }

    private string EscapeSlashes(string value)
    {
        return value.Replace("/", "%2f").Replace("\\", "%5c");
    }

    private TargetPathSegment GetIdentificationColumn(TransformationDetails transformationSettings, DataColumnCollection columns)
    {
        var targetPaths = transformationSettings.TargetPathSegments.Where(x => x.IsIdentity == true);

        if (targetPaths.Count() > 1) { throw new InvalidOperationException($"Wrong number of identity columns. Expected 1 got {targetPaths.Count()}"); }

        if (targetPaths.Count() == 1 && !columns.Contains(targetPaths.First().Target))
        {
            throw new InvalidOperationException($"Failed to parse spreadsheet. Unable to find column with identifiers for train type {targetPaths.First().Target}");
        }

        var identityColumn = targetPaths.Count() == 1 ? targetPaths.First() : new TargetPathSegment("id", "row", true);

        return identityColumn;
    }
}