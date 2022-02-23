using System;
using System.Data;
using Doc2Rdf.Library.Interfaces;
using Doc2Rdf.Library.Models;

namespace Doc2Rdf.Library
{
    internal class RdfMelTableBuilder : IRdfTableBuilder
    {
        private DataTable _dataTable;
        private static DataColumn CreateIdColumn() => new DataColumn("id", typeof(Uri));

        public RdfMelTableBuilder(string tableName)
        {
            _dataTable = new DataTable(tableName);
        }

        public void CreateProvenanceSchema()
        {
            var idColumn = RdfCommonColumns.CreateIdColumn();
            _dataTable.Columns.Add(idColumn);
            _dataTable.PrimaryKey = new DataColumn[] { idColumn };

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
            _dataTable.Rows.Add(
                dataCollectionUri,
                provenance.RevisionDate,
                new Uri(Prefixes.Prefix2Uri["facility"] + provenance.Facility.DocumentProjectId),
                provenance.RevisionNumber, 
                provenance.DataCollectionName,
                new Uri(Prefixes.Prefix2Uri["sor"] + provenance.DataFormat.ToString()),
                new Uri(Prefixes.Prefix2Uri["sor"] + provenance.DataSource.ToString()),
                new Uri(Prefixes.Prefix2Uri["sor"] + provenance.DataSourceType.ToString()),
                new Uri(Prefixes.Prefix2Uri["ext"] + "mel/" + provenance.RevisionOf)
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

        public void CreateInputDataSchema(DataColumnCollection columns)
        {
            var idColumn = RdfCommonColumns.CreateIdColumn();
            _dataTable.Columns.Add(idColumn);
            _dataTable.PrimaryKey = new DataColumn[] { idColumn };

            _dataTable.Columns.Add(RdfCommonColumns.CreateWasDerivedFrom());
            _dataTable.Columns.Add(RdfCommonColumns.CreateWasGeneratedBy());

            foreach (DataColumn column in columns)
            {
                //For excel input, row numbers are temporarily stored in an id column. The row number is 
                //used as a row uri, but the literal value is taken away again when creating the rdfDataTables.
                if (column.ColumnName == "id")
                {
                    continue;
                }
                
                _dataTable.Columns.Add(Prefixes.Prefix2Uri["melraw"] + column.ColumnName, typeof(string));
            }
        }

        public void AddInputDataRows(Uri dataCollectionUri, Uri transformationUri, DataTable inputData)
        {
            const int NumberOfFixedColumns = 3;

            int k = 0;
            foreach (DataRow row in inputData.Rows)
            {   
                if (k <= 1)
                {
                    Console.WriteLine($"{row[0]}, {row[1]}, {row[2]}");
                    k++;
                }
                var itemUri = new Uri($"{dataCollectionUri.AbsoluteUri}#row={row["id"]}");

                var dataRow = _dataTable.NewRow();
                dataRow[0] = itemUri;
                dataRow[1] = dataCollectionUri;
                dataRow[2] = transformationUri;

                //Row number, adjusted for startrow, was added to the input data as column 0 and is
                //skipped here.
                for (var columnIndex = 1; columnIndex < inputData.Columns.Count; columnIndex++)
                {
                    dataRow[columnIndex - 1 + NumberOfFixedColumns] = row[columnIndex];
                }
                _dataTable.Rows.Add(dataRow);

                if(k <= 1)
                {
                    Console.WriteLine($"{_dataTable.Columns[0]}, {dataRow[0]}");
                    Console.WriteLine($"{_dataTable.Columns[1]}, {dataRow[1]}");
                    Console.WriteLine($"{_dataTable.Columns[2]}, {dataRow[2]}");
                    Console.WriteLine($"{_dataTable.Columns[3]}, {dataRow[3]}");
                    Console.WriteLine($"{_dataTable.Columns[4]}, {dataRow[4]}");
                }
            }
        }

        public DataTable GetDataTable() => _dataTable;
    }
}
