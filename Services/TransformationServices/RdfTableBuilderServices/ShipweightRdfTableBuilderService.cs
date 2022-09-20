using Common.ProvenanceModels;
using Common.RdfModels;
using System.Data;

namespace Services.TransformationServices.RdfTableBuilderServices;

public class ShipweightRdfTableBuilderService : IRdfTableBuilderService
{
    private string _builderType;
    private DataTable _dataTable;
    private static DataColumn CreateIdColumn() => new DataColumn("id", typeof(Uri));

    public ShipweightRdfTableBuilderService()
    {
        _dataTable = new DataTable();
        _builderType = "shipweight";
    }

    public string GetBuilderType() => _builderType;

    public DataTable GetProvenanceTable(Uri dataCollectionUri, Provenance provenance)
    {
        _dataTable = new DataTable();

        AddTableName("Provenance");
        CreateProvenanceSchema();
        AddProvenanceRow(dataCollectionUri, provenance);

        return _dataTable;
    }

    public DataTable GetTransformationTable(Uri dataCollectionUri, Uri transformationUri)
    {
        _dataTable = new DataTable();

        AddTableName("Transformation");
        CreateTransformationSchema();
        AddTransformationRow(dataCollectionUri, transformationUri);

        return _dataTable;
    }

    public DataTable GetDataCollectionTable(Uri dataCollectionUri, DataTable inputData)
    {
        _dataTable = new DataTable();

        AddTableName("DataCollection");
        CreateDataCollectionSchema();
        AddDataCollectionRows(dataCollectionUri, inputData);

        return _dataTable;
    }

    public DataTable GetInputDataTable(Uri dataCollectionUri, Uri transformationUri, Provenance provenance, DataTable inputData)
    {
        _dataTable = new DataTable();

        AddTableName(inputData.TableName);
        CreateInputDataSchema(provenance, inputData.Columns);
        AddInputDataRows(dataCollectionUri, transformationUri, inputData);

        return _dataTable;
    }

    private void AddTableName(string tableName)
    {
        _dataTable.TableName = tableName;
    }

    private void CreateProvenanceSchema()
    {
        var idColumn = CreateIdColumn();
        _dataTable.Columns.Add(idColumn);
        _dataTable.PrimaryKey = new DataColumn[] { idColumn };

        _dataTable.Columns.Add(RdfCommonColumns.CreateGeneratedAtTime());
        _dataTable.Columns.Add(RdfCommonColumns.CreateHasPlantId());
        _dataTable.Columns.Add(RdfCommonColumns.CreateHasRevisionNumber());
        _dataTable.Columns.Add(RdfCommonColumns.CreateHasRevisionName());
        _dataTable.Columns.Add(RdfCommonColumns.CreateFromDataCollection());
        _dataTable.Columns.Add(RdfCommonColumns.CreateHasSource());
        _dataTable.Columns.Add(RdfCommonColumns.CreateHasSourceType());
    }

    private void AddProvenanceRow(Uri dataCollectionUri, Provenance provenance)
    {
        _dataTable.Rows.Add(
            dataCollectionUri,
            provenance.RevisionDate,
            new Uri(RdfPrefixes.Prefix2Uri["identifier"] + provenance.PlantId),
            provenance.RevisionNumber,
            provenance.RevisionName,
            provenance.DataCollectionName,
            new Uri(RdfPrefixes.Prefix2Uri["sor"] + provenance.DataSource?.ToString() ?? DataSource.Unknown),
            new Uri(RdfPrefixes.Prefix2Uri["sor"] + provenance.DataSourceType?.ToString() ?? DataSourceType.Unknown())
            );
    }

    private void CreateTransformationSchema()
    {
        var idColumn = RdfCommonColumns.CreateIdColumn();

        _dataTable.Columns.Add(idColumn);
        _dataTable.PrimaryKey = new DataColumn[] { idColumn };

        _dataTable.Columns.Add(RdfCommonColumns.CreateStartedAtTime());
        _dataTable.Columns.Add(RdfCommonColumns.CreateUsed());
    }

    private void AddTransformationRow(Uri dataCollectionUri, Uri transformationUri)
    {
        _dataTable.Rows.Add(
           transformationUri,
           DateTime.Now,
           dataCollectionUri
        );
    }

    private void CreateDataCollectionSchema()
    {
        var idColumn = RdfCommonColumns.CreateIdColumn();
        _dataTable.Columns.Add(idColumn);

        _dataTable.Columns.Add(RdfCommonColumns.CreateHadMember());

    }

    private void AddDataCollectionRows(Uri dataCollectionUri, DataTable inputData)
    {
        var isItem = inputData.Columns.Contains("UniqueNo");
        var index = 0;

        foreach (DataRow row in inputData.Rows)
        {
            index++;
            var itemUri = isItem ?
                             new Uri($"{dataCollectionUri.AbsoluteUri}#{row["UniqueNo"]}") :
                             new Uri($"{dataCollectionUri.AbsoluteUri}#row{index}");

            var dataRow = _dataTable.NewRow();
            dataRow[0] = dataCollectionUri;
            dataRow[1] = itemUri;

            _dataTable.Rows.Add(dataRow);
        }
    }

    private void CreateInputDataSchema(Provenance provenance, DataColumnCollection columns)
    {
        var idColumn = RdfCommonColumns.CreateIdColumn();
        _dataTable.Columns.Add(idColumn);
        _dataTable.PrimaryKey = new DataColumn[] { idColumn };

        _dataTable.Columns.Add(RdfCommonColumns.CreateWasGeneratedBy());

        var dataUri = $"{RdfPrefixes.Prefix2Uri["source"]}{provenance.DataSource}/{provenance.DataSourceTable}#";

        foreach (DataColumn column in columns)
        {
            _dataTable.Columns.Add(dataUri + column.ColumnName, typeof(string));
        }
    }

    private void AddInputDataRows(Uri dataCollectionUri, Uri transformationUri, DataTable inputData)
    {
        const int NumberOfFixedColumns = 2;
        var isItem = inputData.Columns.Contains("UniqueNo");
        var index = 0;

        foreach (DataRow row in inputData.Rows)
        {
            index++;
            var itemUri = isItem ?
                             new Uri($"{dataCollectionUri.AbsoluteUri}#{row["UniqueNo"]}") :
                             new Uri($"{dataCollectionUri.AbsoluteUri}#row{index}");

            var dataRow = _dataTable.NewRow();
            dataRow[0] = itemUri;
            dataRow[1] = transformationUri;

            for (var columnIndex = 0; columnIndex < inputData.Columns.Count; columnIndex++)
            {
                dataRow[columnIndex + NumberOfFixedColumns] = row[columnIndex];
            }
            _dataTable.Rows.Add(dataRow);
        }
    }
}