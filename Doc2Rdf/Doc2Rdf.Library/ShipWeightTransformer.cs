using Doc2Rdf.Library.Interfaces;
using Doc2Rdf.Library.Models;
using System;
using System.Data;
using Doc2Rdf.Library.IO;

namespace Doc2Rdf.Library
{
    public class ShipWeightTransformer
    {
        public string Transform(string facilityName)
        {
            var inputData = ShipWeightDBReader.GetWeightData(facilityName);
            var facilityPlantId = inputData.Rows[0]["Plant"].ToString();

            if (string.IsNullOrWhiteSpace(facilityPlantId))
            {
                throw new InvalidOperationException($"Invalid plant id for {facilityName}");
            }

            var provenance = CreateProvenance(facilityName, facilityPlantId);
            
            var rdfTransformer = new RdfTransformer();
            return rdfTransformer.Transform(provenance, inputData);
        }

        private Provenance CreateProvenance(string facilityName, string plantId)
        {
            var facility = new FacilityIdentifiers(sAPPlantId: plantId);

            var provenance = new Provenance(facility,
                                            facilityName,
                                            "01",
                                            "NA",
                                            DateTime.Now,
                                            DataSource.Shipweight,
                                            DataSourceType.Database,
                                            DataFormat.NA);

            return provenance;
        }
    }
}