
using Services.TransformationServices.RdfPreprocessingServices;
using System;
using System.Data;
using System.Linq;
using Common.ProvenanceModels;
using Xunit;

namespace Services.Tests
{
    public class ShipWeightReaderTests
    {
        private readonly IRdfPreprocessingService _rdfPreprocessingService;

        public ShipWeightReaderTests(IRdfPreprocessingService rdfPreprocessingService)
        {
            _rdfPreprocessingService = rdfPreprocessingService;
        }

        [Fact]
        public void TestConvertingInputToRdfDataTable()
        {
            Provenance provenance = GetProvenance();
            DataTable inputData = GetTestData();

            var transformedData = _rdfPreprocessingService.CreateRdfTables(provenance, inputData);

            Assert.True(transformedData.Tables.Count == 4, $"Wrong number of rdf-preprocessed tables: {transformedData.Tables.Count}");

            Assert.Equal(new Uri("https://rdf.equinor.com/data/facility-identification/1234"), transformedData.Tables["Provenance"].Rows[0]["https://rdf.equinor.com/ontology/facility-identification/v1#hasPlantId"]);

            Assert.True(transformedData.Tables["Transformation"] != Enumerable.Empty<DataTable>(), "Failed to create transformation table");

            Assert.True(transformedData.Tables["TestData"].Columns.Contains("id"), "InputData table does not contain the id column");

            Assert.True(transformedData.Tables["TestData"].Columns.Count == inputData.Columns.Count + 2, $"Wrong number of columns in InputData table {transformedData.Tables["TestData"].Columns.Count}");

            Assert.True(transformedData.Tables["TestData"].Rows.Count == inputData.Rows.Count, $"Wrong number of rows in InputData table: {transformedData.Tables["TestData"].Rows.Count}");
        }

        private Provenance GetProvenance()
        {
            var facilityId = "Test";
            var provenance = new Provenance(facilityId, DataSource.Shipweight());
            provenance.DocumentProjectId = "na";
            provenance.PlantId = "1234";
            provenance.DataCollectionName = "MyFacility";
            provenance.RevisionName = "01";
            provenance.RevisionNumber = 1;
            provenance.RevisionDate = DateTime.Now;
            provenance.DataSourceType = DataSourceType.Database();
            provenance.DataSourceTable = "TestData";

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