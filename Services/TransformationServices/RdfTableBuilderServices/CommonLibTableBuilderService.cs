using Common.ProvenanceModels;
using Common.RdfModels;
using Common.RevisionTrainModels;
using System.Data;

namespace Services.TransformationServices.RdfTableBuilderServices;

public class CommonLibTableBuilderService : IRdfTableBuilderService
{
    public string GetBuilderType() => DataSource.CommonLib;

    public DataTable GetDataCollectionTable(Uri dataCollectionUri, DataTable inputData)
    {
        var dataTable = new DataTable
        {
            TableName = "DataCollection",
        };

        return dataTable;
    }

    public DataTable GetInputDataTable(Uri dataCollectionUri, Uri transformationUri, Provenance provenance, DataTable inputData)
    {
        var dataTable = new DataTable
        {
            TableName = inputData.TableName
        };

        CreateInputDataSchema(dataTable, provenance, inputData.Columns);
        AddInputDataRows(dataTable, dataCollectionUri, transformationUri, inputData);

        return dataTable;
    }

    public DataTable GetInputDataTable(Uri dataCollectionUri, RevisionTrainModel revisionTrain, DataTable inputData)
    {
        throw new NotImplementedException();
    }

    public DataTable GetProvenanceTable(Uri dataCollectionUri, Provenance provenance)
    {
        var dataTable = new DataTable
        {
            TableName = "Provenance"
        };
        CreateProvenanceForNamedGraphSchema(dataTable);
        return AddProvenanceRow(dataTable, provenance);
    }

    public DataTable GetTransformationTable(Uri dataCollectionUri, Uri transformationUri)
    {
        var dataTable = CreateTransformationSchema(new DataTable
        {
            TableName = "Transformation"
        });
        dataTable.Rows.Add(transformationUri, DateTime.Now, dataCollectionUri);
        return dataTable;
    }

    private static DataTable CreateProvenanceForNamedGraphSchema(DataTable dataTable)
    {
        var idColumn = RdfCommonColumns.CreateIdColumn();
        dataTable.Columns.Add(idColumn);
        dataTable.PrimaryKey = new DataColumn[] { idColumn };

        dataTable.Columns.Add(RdfCommonColumns.CreateType());
        dataTable.Columns.Add(RdfCommonColumns.CreateGeneratedAtTime());
        dataTable.Columns.Add(RdfCommonColumns.CreateHasFacilityId());
        dataTable.Columns.Add(RdfCommonColumns.CreateHasDocumentName());
        dataTable.Columns.Add(RdfCommonColumns.CreateHasRevisionNumber());
        dataTable.Columns.Add(RdfCommonColumns.CreateHasRevisionName());
        dataTable.Columns.Add(RdfCommonColumns.CreateHasSource());
        dataTable.Columns.Add(RdfCommonColumns.CreateWasRevisionOf());
        return dataTable;
    }

    private static DataTable AddProvenanceRow(DataTable dataTable, Provenance provenance)
    {
        var currentRevision = new Uri(RdfPrefixes.Prefix2Uri["equinor"], $"graph/{provenance.DocumentName}/{provenance.RevisionNumber}");
        dataTable.Rows.Add(
            currentRevision,
            RdfCommonClasses.CreateNamedGraphClass(),
            provenance.RevisionDate,
            new Uri(RdfPrefixes.Prefix2Uri["identifier"] + provenance.FacilityId),
            provenance.DocumentName,
            provenance.RevisionNumber,
            provenance.RevisionName,
            new Uri($"{RdfPrefixes.Prefix2Uri["sor"]}{provenance.DataSourceTable ?? DataSource.Unknown}"),
            provenance.PreviousRevision
        );

        return dataTable;
    }

    private static DataTable CreateTransformationSchema(DataTable dataTable)
    {
        var idColumn = RdfCommonColumns.CreateIdColumn();

        dataTable.Columns.Add(idColumn);
        dataTable.PrimaryKey = new DataColumn[] { idColumn };

        dataTable.Columns.Add(RdfCommonColumns.CreateStartedAtTime());
        dataTable.Columns.Add(RdfCommonColumns.CreateUsed());

        return dataTable;
    }

    private static DataTable CreateInputDataSchema(DataTable dataTable, Provenance provenance, DataColumnCollection columns)
    {
        var idColumn = RdfCommonColumns.CreateIdColumn();
        dataTable.Columns.Add(idColumn);
        dataTable.PrimaryKey = new DataColumn[] { idColumn };

        dataTable.Columns.Add(RdfCommonColumns.CreateWasGeneratedBy());

        var dataUri = $"{RdfPrefixes.Prefix2Uri["source"]}{provenance.DataSourceTable}#";

        foreach (DataColumn column in columns)
            dataTable.Columns.Add(dataUri + column.ColumnName, typeof(string));

        return dataTable;
    }

    private static DataTable AddInputDataRows(DataTable dataTable, Uri dataCollectionUri, Uri transformationUri, DataTable inputData)
    {
        const int NumberOfFixedColumns = 2;
        foreach (DataRow row in inputData.Rows)
        {
            var itemUri = new Uri(dataCollectionUri, row.Field<string>("Identity"));

            var dataRow = dataTable.NewRow();
            dataRow[0] = itemUri;
            dataRow[1] = transformationUri;

            for (var columnIndex = 0; columnIndex < inputData.Columns.Count; columnIndex++)
                dataRow[columnIndex + NumberOfFixedColumns] = row[columnIndex];
            dataTable.Rows.Add(dataRow);
        }
        return dataTable;
    }

    public Uri? GetTransformationUri(Provenance provenance)
    {
        return new Uri($"{RdfPrefixes.Prefix2Uri["transformation"]}{provenance.DataSourceTable}_{DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd")}");
    }
}
