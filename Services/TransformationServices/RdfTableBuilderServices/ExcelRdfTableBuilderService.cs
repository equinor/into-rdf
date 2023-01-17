using Common.RdfModels;
using System.Data;
using Common.TransformationModels;

namespace Services.TransformationServices.RdfTableBuilderServices;

public class ExcelRdfTableBuilderService : IExcelRdfTableBuilderService
{
    private DataTable _dataTable;

    public ExcelRdfTableBuilderService()
    {
        _dataTable = new DataTable();
    }

    public DataTable GetInputDataTable(Uri dataCollectionUri, SpreadsheetTransformationDetails transformationSettings, DataTable inputData)
    {
        _dataTable = new DataTable();
        CreateInputDataSchema(transformationSettings, inputData.Columns);
        AddInputDataRows(dataCollectionUri, transformationSettings, inputData);

        return _dataTable;
    }

    private void CreateInputDataSchema(SpreadsheetTransformationDetails transformationSettings, DataColumnCollection columns)
    {
        var idColumn = RdfCommonColumns.CreateIdColumn();
        _dataTable.Columns.Add(idColumn);
        _dataTable.PrimaryKey = new DataColumn[] { idColumn };

        var dataUri = $"{RdfPrefixes.Prefix2Uri["source"]}{transformationSettings.TransformationType}#";

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

    private void AddInputDataRows(Uri dataCollectionUri, SpreadsheetTransformationDetails transformationServices, DataTable inputData)
    {
        const int NumberOfFixedColumns = 1;

        var targetIdColumn = GetIdentificationColumn(transformationServices, inputData.Columns);
        var pathSegment = String.IsNullOrEmpty(targetIdColumn.Segment) ? "" : $"{targetIdColumn.Segment}/"; 
        dataCollectionUri = new Uri($"{dataCollectionUri.AbsoluteUri}{pathSegment}");

        foreach (DataRow row in inputData.Rows)
        {
            var itemUri = new Uri($"{dataCollectionUri.AbsoluteUri}{row[targetIdColumn.Target]}");
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

    private TargetPathSegment GetIdentificationColumn(SpreadsheetTransformationDetails transformationSettings, DataColumnCollection columns)
    {
        var targetPaths = transformationSettings.TargetPathSegments.Where(x => x.IsIdentity == true);

        if (targetPaths.Count() > 1) { throw new InvalidOperationException($"Wrong number of identity columns. Expected 1 got {targetPaths.Count()}"); }

        if (!columns.Contains(targetPaths.First().Target))
        {
            throw new InvalidOperationException($"Failed to parse spreadsheet. Unable to find column with identifiers for train type {targetPaths.First().Target}");
        }

        var identityColumn = targetPaths.Count() == 1 ? targetPaths.First() : new TargetPathSegment("id", "row", true);

        return identityColumn; 
    }
}