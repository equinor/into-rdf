using Common.ProvenanceModels;
using Common.RdfModels;
using System.Data;
using System.Linq;

namespace Services.TransformationServices.RdfTableBuilderServices;

public class ExcelRdfTableBuilderService : IRdfTableBuilderService
{
    private string _builderType;
    private DataTable _dataTable;

    public ExcelRdfTableBuilderService()
    {
        _dataTable = new DataTable();
        _builderType = "spreadsheet";
    }

    public string GetBuilderType() => _builderType;

    public DataTable GetProvenanceTable(Uri dataCollectionUri, Provenance provenance)
    {
        _dataTable = new DataTable(); 
        
        AddTableName("Provenance");

        //Temporary solution so that the old MEL transformation works until it is migrated to named graph provenance.
        if (provenance.DataSource == DataSource.LineList)
        {
            CreateProvenanceForNamedGraphSchema();
            AddProvenanceForNamedGraphRow(provenance);
        }
        else
        {
            CreateProvenanceSchema();
            AddProvenanceRow(dataCollectionUri, provenance);
        }

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
        AddTableName("InputData");
        CreateInputDataSchema(provenance, inputData.Columns);
        AddInputDataRows(dataCollectionUri, transformationUri, provenance, inputData);

        return _dataTable;
    }

    private void AddTableName(string tableName)
    {
        _dataTable.TableName = tableName;
    }

    private void CreateProvenanceSchema()
    {
        var idColumn = RdfCommonColumns.CreateIdColumn();
        _dataTable.Columns.Add(idColumn);
        _dataTable.PrimaryKey = new DataColumn[] { idColumn };

        _dataTable.Columns.Add(RdfCommonColumns.CreateType());
        _dataTable.Columns.Add(RdfCommonColumns.CreateGeneratedAtTime());
        _dataTable.Columns.Add(RdfCommonColumns.CreateHasDocumentProjectId());
        _dataTable.Columns.Add(RdfCommonColumns.CreateHasRevisionNumber());
        _dataTable.Columns.Add(RdfCommonColumns.CreateHasRevisionName());
        _dataTable.Columns.Add(RdfCommonColumns.CreateFromDataCollection());
        _dataTable.Columns.Add(RdfCommonColumns.CreateHasSource());
        _dataTable.Columns.Add(RdfCommonColumns.CreateHasSourceType());
        _dataTable.Columns.Add(RdfCommonColumns.CreateWasRevisionOf());
    }

    private void CreateProvenanceForNamedGraphSchema()
    {
        var idColumn = RdfCommonColumns.CreateIdColumn();
        _dataTable.Columns.Add(idColumn);
        _dataTable.PrimaryKey = new DataColumn[] { idColumn };

        _dataTable.Columns.Add(RdfCommonColumns.CreateType());
        _dataTable.Columns.Add(RdfCommonColumns.CreateGeneratedAtTime());
        _dataTable.Columns.Add(RdfCommonColumns.CreateHasDocumentProjectId());
        _dataTable.Columns.Add(RdfCommonColumns.CreateHasRevisionNumber());
        _dataTable.Columns.Add(RdfCommonColumns.CreateHasRevisionName());
        _dataTable.Columns.Add(RdfCommonColumns.CreateWasDerivedFrom());
        _dataTable.Columns.Add(RdfCommonColumns.CreateHasSource());
        _dataTable.Columns.Add(RdfCommonColumns.CreateWasRevisionOf());
    }
    private void AddProvenanceForNamedGraphRow(Provenance provenance)
    {
        var projectUriSegment = $"{provenance.FacilityId.ToLower()}/{provenance.DocumentProjectId?.ToLower()}";
        
        var currentRevision = new Uri($"{RdfPrefixes.Prefix2Uri["equinor"].ToString()}graph/{projectUriSegment}/{provenance.DataSource}/{provenance.RevisionNumber}"); 
        
        var previousRevision = provenance.PreviousRevision 
                                ?? (String.IsNullOrWhiteSpace(provenance.PreviousRevisionNumber) == false 
                                        ? new Uri($"{RdfPrefixes.Prefix2Uri["equinor"].ToString()}{projectUriSegment}/{provenance.DataSource}/{provenance.PreviousRevisionNumber}") 
                                        : null);
        
        _dataTable.Rows.Add(
            currentRevision,
            RdfCommonClasses.CreateNamedGraphClass(),
            provenance.RevisionDate,
            new Uri(RdfPrefixes.Prefix2Uri["identifier"] + provenance.DocumentProjectId),
            provenance.RevisionNumber,
            provenance.RevisionName,
            provenance.DataCollectionName,
            new Uri(RdfPrefixes.Prefix2Uri["sor"] + provenance.DataSource ?? DataSource.Unknown),
            previousRevision
            );
    }


    private void AddProvenanceRow(Uri dataCollectionUri, Provenance provenance)
    {
        var projectUriSegment = $"{provenance.FacilityId}/{provenance.DocumentProjectId?.ToLower()}";
        var previousRevision = provenance.PreviousRevision 
                                ?? (String.IsNullOrWhiteSpace(provenance.PreviousRevisionNumber) == false 
                                        ? new Uri($"{RdfPrefixes.Prefix2Uri["equinor"].ToString()}{projectUriSegment}/{provenance.DataSource}/{provenance.PreviousRevisionNumber}") 
                                        : null);
        
        _dataTable.Rows.Add(
            dataCollectionUri,
            RdfCommonClasses.CreateCollectionClass(),
            provenance.RevisionDate,
            new Uri(RdfPrefixes.Prefix2Uri["identifier"] + provenance.DocumentProjectId),
            provenance.RevisionNumber,
            provenance.RevisionName,
            provenance.DataCollectionName,
            new Uri(RdfPrefixes.Prefix2Uri["sor"] + provenance.DataSource ?? DataSource.Unknown),
            new Uri(RdfPrefixes.Prefix2Uri["sor"] + provenance.DataSourceType ?? DataSourceType.Unknown()),
            previousRevision
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
           DateTime.Now.ToUniversalTime(),
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
        foreach (DataRow row in inputData.Rows)
        {
            var itemUri = new Uri($"{dataCollectionUri.AbsoluteUri}#row={row["id"]}");

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

        var dataUri = $"{RdfPrefixes.Prefix2Uri["source"]}{provenance.DataSource}#";

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



    private void AddInputDataRows(Uri dataCollectionUri, Uri transformationUri, Provenance provenance, DataTable inputData)
    {
        const int NumberOfFixedColumns = 2;

        GetItemIdentification(dataCollectionUri, inputData.Columns, provenance.DataSource, out var idColumn, out var identificationUri);

        foreach (DataRow row in inputData.Rows)
        {
            var itemUri = new Uri($"{identificationUri.AbsoluteUri}{row[idColumn]}");
            var existingId = _dataTable.AsEnumerable().Any(row => itemUri.ToString() == row.Field<Uri>("id")?.ToString());

            if (existingId)
            {
                itemUri = new Uri($"{itemUri.AbsoluteUri}_row={row["id"]}");
            }

            var dataRow = _dataTable.NewRow();
            dataRow[0] = itemUri;
            dataRow[1] = transformationUri;

            //Row number, adjusted for the start row, was added to the input data as column 0 and is
            //skipped here.
            for (var columnIndex = 1; columnIndex < inputData.Columns.Count; columnIndex++)
            {
                dataRow[columnIndex - 1 + NumberOfFixedColumns] = row[columnIndex];
            }

            _dataTable.Rows.Add(dataRow);
        }
    }

    private void GetItemIdentification(Uri dataCollectionUri, DataColumnCollection columns, string source, out string idColumn, out Uri identificationUri)
    {
        switch (source) 
        {
            case DataSource.LineList:
            {
                GetLineListItemIdentification(dataCollectionUri, columns, out idColumn, out identificationUri);
                break;
            }
            default:
            {
                idColumn = "id";
                identificationUri = new Uri($"{dataCollectionUri.AbsoluteUri}#row=");
                break;
            }
        }
    }

    private void GetLineListItemIdentification(Uri dataCollectionUri, DataColumnCollection columns, out string idColumn, out Uri identificationUri)
    {
        idColumn = columns.Contains("Line Tag") ? "Line Tag" : 
                        columns.Contains("Line number") ? "Line number" : "id";

        identificationUri = dataCollectionUri;

    }
}