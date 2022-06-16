using Common.ProvenanceModels;
using System;
using System.Data;
using Doc2Rdf.Library.Interfaces;
using Doc2Rdf.Library.Models;

namespace Doc2Rdf.Library.Services;

public class RdfMelTableBuilder : IRdfTableBuilder
{
    private string _builderType;
    private DataTable _dataTable;

    public RdfMelTableBuilder()
    {
        _dataTable = new DataTable();
        _builderType = "mel";
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

    private void AddProvenanceRow(Uri dataCollectionUri, Provenance provenance)
    {
        var projectUriSegment = $"{provenance.FacilityId}/{provenance.DocumentProjectId}";

        var previousRevision = provenance.PreviousRevision 
                                ?? (provenance.PreviousRevisionNumber != string.Empty 
                                        ? new Uri(RdfPrefixes.Prefix2Uri["equinor"] + projectUriSegment + "/mel/" + provenance.PreviousRevisionNumber) 
                                        : null);
        
        _dataTable.Rows.Add(
            dataCollectionUri,
            RdfCommonTypes.CreateCollectionType(),
            provenance.RevisionDate,
            new Uri(RdfPrefixes.Prefix2Uri["facility"] + provenance.DocumentProjectId),
            provenance.RevisionNumber,
            provenance.RevisionName,
            provenance.DataCollectionName,
            new Uri(RdfPrefixes.Prefix2Uri["sor"] + provenance.DataSource?.ToString() ?? DataSource.Unknown()),
            new Uri(RdfPrefixes.Prefix2Uri["sor"] + provenance.DataSourceType?.ToString() ?? DataSourceType.Unknown()),
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



    private void AddInputDataRows(Uri dataCollectionUri, Uri transformationUri, DataTable inputData)
    {
        const int NumberOfFixedColumns = 2;

        foreach (DataRow row in inputData.Rows)
        {
            var itemUri = new Uri($"{dataCollectionUri.AbsoluteUri}#row={row["id"]}");

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

}

