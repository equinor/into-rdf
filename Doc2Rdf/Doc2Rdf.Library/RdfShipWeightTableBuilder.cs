
using System;
using System.Data;
using Doc2Rdf.Library.Interfaces;
using Doc2Rdf.Library.Models;

namespace Doc2Rdf.Library
{
    internal class RdfShipWeightTableBuilder : IRdfTableBuilder
    {
        private DataTable _dataTable;
        private static DataColumn CreateIdColumn() => new DataColumn("id", typeof(Uri));

        public RdfShipWeightTableBuilder(string tableName)
        {
            _dataTable = new DataTable(tableName);
        }
        public void CreateProvenanceSchema()
        {
            var idColumn = CreateIdColumn();
            _dataTable.Columns.Add(idColumn);
            _dataTable.PrimaryKey = new DataColumn[] { idColumn };

            _dataTable.Columns.Add(RdfCommonColumns.CreateGeneratedAtTime());
            _dataTable.Columns.Add(RdfCommonColumns.CreateHasPlantId());
            _dataTable.Columns.Add(RdfCommonColumns.CreateIsRevision());
            _dataTable.Columns.Add(RdfCommonColumns.CreateFromDataCollection());
            _dataTable.Columns.Add(RdfCommonColumns.CreateHasFormat());
            _dataTable.Columns.Add(RdfCommonColumns.CreateHasSource());
            _dataTable.Columns.Add(RdfCommonColumns.CreateHasSourceType());
        }

        public void AddProvenanceRow(Uri dataCollectionUri, Provenance provenance)
        {
            _dataTable.Rows.Add(
                dataCollectionUri,
                provenance.RevisionDate,
                new Uri(Prefixes.Prefix2Uri["facility"] + provenance.Facility.SAPPlantId),
                provenance.RevisionNumber,
                provenance.DataCollectionName,
                new Uri(Prefixes.Prefix2Uri["sor"] + provenance.DataFormat.ToString()),
                new Uri(Prefixes.Prefix2Uri["sor"] + provenance.DataSource.ToString()),
                new Uri(Prefixes.Prefix2Uri["sor"] + provenance.DataSourceType.ToString())
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
               DateTime.Now,
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
                _dataTable.Columns.Add(Prefixes.Prefix2Uri["shipweightraw"] + column.ColumnName, typeof(string));
            }
        }

        public void AddInputDataRows(Uri dataCollectionUri, Uri transformationUri, DataTable inputData)
        {
            const int NumberOfFixedColumns = 3;

            foreach (DataRow row in inputData.Rows)
            {
                var itemUri = new Uri($"{dataCollectionUri.AbsoluteUri}#{row["UniqueNo"]}");

                var dataRow = _dataTable.NewRow();
                dataRow[0] = itemUri;
                dataRow[1] = dataCollectionUri;
                dataRow[2] = transformationUri;

                for (var columnIndex = 0; columnIndex < inputData.Columns.Count; columnIndex++)
                {
                    dataRow[columnIndex + NumberOfFixedColumns] = row[columnIndex];
                }
                _dataTable.Rows.Add(dataRow);
            }
        }

        public void CreatePhaseFilterSchema(DataColumnCollection columns)
        {
            var idColumn = RdfCommonColumns.CreateIdColumn();
            _dataTable.Columns.Add(idColumn);
            _dataTable.PrimaryKey = new DataColumn[] { idColumn };

            _dataTable.Columns.Add(RdfCommonColumns.CreateWasDerivedFrom());
            _dataTable.Columns.Add(RdfCommonColumns.CreateWasGeneratedBy());

            foreach (DataColumn column in columns)
            {
                if (column.ColumnName.Contains("FilterID"))
                {
                    _dataTable.Columns.Add($"{Prefixes.Prefix2Uri["shipweightraw"]}Description", typeof(string));       
                    _dataTable.Columns.Add($"{Prefixes.Prefix2Uri["shipweightraw"]}hasBuildPhase", typeof(Uri));
                    continue;
                }
                _dataTable.Columns.Add(Prefixes.Prefix2Uri["shipweightraw"] + column.ColumnName, typeof(string));
            }
        }

        public void AddPhaseFilterRows(Uri dataCollectionUri, Uri transformationUri, DataTable phaseFilterData)
        {
            var index = 1;

            foreach (DataRow row in phaseFilterData.Rows)
            {
                int numberOfAdditionalColumns = 3;
                var itemUri = new Uri($"{dataCollectionUri.AbsoluteUri}#buildPhaseFilter_{index}");

                var dataRow = _dataTable.NewRow();
                dataRow[0] = itemUri;
                dataRow[1] = dataCollectionUri;
                dataRow[2] = transformationUri;

                for (var columnIndex = 0; columnIndex < phaseFilterData.Columns.Count; columnIndex++)
                {      
                    if (phaseFilterData.Columns[columnIndex].ColumnName.Contains("FilterID"))
                    {
                        dataRow[columnIndex + numberOfAdditionalColumns] = row[columnIndex];
                        numberOfAdditionalColumns++;
                        
                        var cell = row[columnIndex];

                        //"As built" is written in different ways, with " ", "-" and possible in one word
                        if (cell.ToString()!.Contains("As") && 
                            cell.ToString()!.Contains("Built"))
                        {
                            dataRow[columnIndex + numberOfAdditionalColumns] = new Uri(Prefixes.Prefix2Uri["shipweightraw"] + "AsBuilt");
                        }
                        else if (cell.ToString()!.Contains("As") &&
                                 cell.ToString()!.Contains("Is"))
                        {
                            dataRow[columnIndex + numberOfAdditionalColumns] = new Uri(Prefixes.Prefix2Uri["shipweightraw"] + "AsIs");
                        }
                        else
                        {
                            dataRow[columnIndex + numberOfAdditionalColumns] = new Uri(Prefixes.Prefix2Uri["shipweightraw"] + "MiscPhase");
                        }
                    }
                    else
                    {
                        dataRow[columnIndex + numberOfAdditionalColumns] = row[columnIndex];
                    }
                }

                _dataTable.Rows.Add(dataRow);
                index++;
            }
        }

        public void CreatePhaseCodeSchema(DataColumnCollection columns)
        {
            var idColumn = RdfCommonColumns.CreateIdColumn();
            _dataTable.Columns.Add(idColumn);
            _dataTable.PrimaryKey = new DataColumn[] { idColumn };

            _dataTable.Columns.Add(RdfCommonColumns.CreateWasDerivedFrom());
            _dataTable.Columns.Add(RdfCommonColumns.CreateWasGeneratedBy());

            foreach (DataColumn column in columns)
            {
                _dataTable.Columns.Add(Prefixes.Prefix2Uri["shipweightraw"] + column.ColumnName, typeof(string));
            }
        }

        public void AddPhaseCodeRows(Uri dataCollectionUri,Uri transformationUri, DataTable inputData)
        {
            const int NumberOfFixedColumns = 3;
            int index = 1;
            foreach (DataRow row in inputData.Rows)
            {
                var itemUri = new Uri($"{dataCollectionUri.AbsoluteUri}#buildPhaseCode_{index}");

                var dataRow = _dataTable.NewRow();
                dataRow[0] = itemUri;
                dataRow[1] = dataCollectionUri;
                dataRow[2] = transformationUri;

                for (var columnIndex = 0; columnIndex < inputData.Columns.Count; columnIndex++)
                {
                    dataRow[columnIndex + NumberOfFixedColumns] = row[columnIndex];
                }
                _dataTable.Rows.Add(dataRow);
                index++;
            }
        }

        public DataTable GetDataTable() => _dataTable;
    }
}
