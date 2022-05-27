using System;
using System.Data;
using Doc2Rdf.Library.Interfaces;
using Doc2Rdf.Library.Models;

namespace Doc2Rdf.Library.Services;

public class RdfMelTableBuilder : IRdfTableBuilder
{
    private DataTable _dataTable;

    public RdfMelTableBuilder()
    {
        _dataTable = new DataTable();
    }
    public void AddTableName(string tableName)
    {
        _dataTable.TableName = tableName;
    }
    public void CreateProvenanceSchema()
    {
        var idColumn = RdfCommonColumns.CreateIdColumn();
        _dataTable.Columns.Add(idColumn);
        _dataTable.PrimaryKey = new DataColumn[] { idColumn };

        _dataTable.Columns.Add(RdfCommonColumns.CreateType());
        _dataTable.Columns.Add(RdfCommonColumns.CreateGeneratedAtTime());
        _dataTable.Columns.Add(RdfCommonColumns.CreateHasDocumentProjectId());
        _dataTable.Columns.Add(RdfCommonColumns.CreateIsRevision());
        _dataTable.Columns.Add(RdfCommonColumns.CreateFromDataCollection());
        _dataTable.Columns.Add(RdfCommonColumns.CreateHasFormat());
        _dataTable.Columns.Add(RdfCommonColumns.CreateHasSource());
        _dataTable.Columns.Add(RdfCommonColumns.CreateHasSourceType());
        _dataTable.Columns.Add(RdfCommonColumns.CreateWasRevisionOf());
    }

    public void AddProvenanceRow(Uri dataCollectionUri, Provenance provenance)
    {
        var projectUriSegment = $"{provenance.Facility.FacilityId}/{provenance.Facility.DocumentProjectId}";
        var revisionOf = provenance.RevisionOf != string.Empty ? new Uri(RdfPrefixes.Prefix2Uri["equinor"] + projectUriSegment + "/mel/" + provenance.RevisionOf) : null;

        _dataTable.Rows.Add(
            dataCollectionUri,
            RdfCommonTypes.CreateCollectionType(),
            provenance.RevisionDate,
            new Uri(RdfPrefixes.Prefix2Uri["facility"] + provenance.Facility.DocumentProjectId.ToLower()),
            provenance.RevisionNumber,
            provenance.DataCollectionName,
            new Uri(RdfPrefixes.Prefix2Uri["sor"] + provenance.DataFormat.ToString()),
            new Uri(RdfPrefixes.Prefix2Uri["sor"] + provenance.DataSource.ToString()),
            new Uri(RdfPrefixes.Prefix2Uri["sor"] + provenance.DataSourceType.ToString()),
            revisionOf
            );
    }

    public void CreateTransformationSchema()
    {
        var idColumn = RdfCommonColumns.CreateIdColumn();
        _dataTable.Columns.Add(idColumn);
        _dataTable.PrimaryKey = new DataColumn[] { idColumn };

        _dataTable.Columns.Add(RdfCommonColumns.CreateStartedAtTime());
        _dataTable.Columns.Add(RdfCommonColumns.CreateUsed());
    }

    public void AddTransformationRow(Uri dataCollectionUri, Uri transformationUri)
    {
        _dataTable.Rows.Add(
           transformationUri,
           DateTime.Now.ToUniversalTime(),
           dataCollectionUri
        );
    }

    public void CreateDataCollectionSchema()
    {
        var idColumn = RdfCommonColumns.CreateIdColumn();
        _dataTable.Columns.Add(idColumn);

        _dataTable.Columns.Add(RdfCommonColumns.CreateHadMember());

    }

    public void CreateInputDataSchema(Provenance provenance, DataColumnCollection columns)
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

    public void AddDataCollectionRows(Uri dataCollectionUri, DataTable inputData)
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

    public void AddInputDataRows(Uri dataCollectionUri, Uri transformationUri, DataTable inputData)
    {
        const int NumberOfFixedColumns = 2;

        foreach (DataRow row in inputData.Rows)
        {
            var itemUri = new Uri($"{dataCollectionUri.AbsoluteUri}#row={row["id"]}");

            var dataRow = _dataTable.NewRow();
            dataRow[0] = itemUri;
            dataRow[1] = transformationUri;

            //Row number, adjusted for startrow, was added to the input data as column 0 and is
            //skipped here.
            for (var columnIndex = 1; columnIndex < inputData.Columns.Count; columnIndex++)
            {
                dataRow[columnIndex - 1 + NumberOfFixedColumns] = row[columnIndex];
            }
            _dataTable.Rows.Add(dataRow);
        }
    }

    public DataTable GetDataTable() => _dataTable;
}

