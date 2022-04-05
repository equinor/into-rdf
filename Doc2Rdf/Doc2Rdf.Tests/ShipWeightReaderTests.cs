using System;
using System.Data;
using System.Linq;
using Doc2Rdf.Library;
using Doc2Rdf.Library.Models;
using Xunit;

namespace Doc2Rdf.Tests
{
    public class ShipWeightReaderTests
    {
        [Fact]
        public void TestConvertingInputToRdfDataTable()
        {
            Provenance provenance = GetProvenance();
            DataTable inputData = GetTestData();

            DataSet dataSet = new DataSet();
            dataSet.Tables.Add(inputData);

            var rdfPreprocessor= new RdfPreprocessor(provenance.DataSource);
            var transformedData = rdfPreprocessor.CreateRdfTables(provenance, dataSet);

            Assert.True(transformedData.Tables.Count == 3, $"Wrong number of rdf-preprocessedtables: {transformedData.Tables.Count}");

            Assert.Equal(transformedData.Tables["Provenance"].Rows[0]["http://rdf.equinor.com/ontology/facility#hasPlantId"], "http://rdf.equinor.com/ontology/facility#1234");

            Assert.True(transformedData.Tables["Transformation"] != Enumerable.Empty<DataTable>(), "Failed to create transformation table");

            Assert.True(transformedData.Tables["TestData"].Columns.Contains("id"), "InputData table does not contain the id column");

            Assert.True(transformedData.Tables["TestData"].Columns.Count == inputData.Columns.Count + 3, $"Wrong number of columns in InputData table {transformedData.Tables["TestData"].Columns.Count}");

            Assert.True(transformedData.Tables["TestData"].Rows.Count == inputData.Rows.Count, $"Wrong number of rows in InputData table: {transformedData.Tables["TestData"].Rows.Count}");
        }

        private Provenance GetProvenance()
        {
            var facility = new FacilityIdentifiers(sAPPlantId: "1234");
            var provenance = new Provenance(facility, "MyFacility", "01", "NA", DateTime.Now, DataSource.Shipweight, DataSourceType.Database, DataFormat.NA);

            return provenance;
        }

        private DataTable GetTestData()
        {
            var testTable = new DataTable();
            testTable.TableName = "TestData";

            testTable.Columns.Add(new DataColumn("UniqueNo", typeof(string)));
            testTable.Columns.Add(new DataColumn("timestamp", typeof(string)));
            testTable.Columns.Add(new DataColumn("weight", typeof(string)));

            var row = testTable.NewRow();
            row["UniqueNo"] = "1";
            row["timestamp"] = "2022-02-16";
            row["weight"] = "500";

            testTable.Rows.Add(row);

            row = testTable.NewRow();
            row["UniqueNo"] = "2";
            row["timestamp"] = "2022-02-16";
            row["weight"] = "700";

            testTable.Rows.Add(row);

            row = testTable.NewRow();
            row["UniqueNo"] = "3";
            row["timestamp"] = "2022-02-16";
            row["weight"] = "100";

            testTable.Rows.Add(row);

            return testTable;
        }
    }
}