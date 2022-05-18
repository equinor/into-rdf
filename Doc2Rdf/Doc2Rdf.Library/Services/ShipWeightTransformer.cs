using Doc2Rdf.Library.Interfaces;
using Doc2Rdf.Library.Models;
using System;
using Doc2Rdf.Library.IO;

namespace Doc2Rdf.Library.Services;

public class ShipWeightTransformer : IShipWeightTransformer
{
    IRdfTransformer _rdfTransformer;
    public ShipWeightTransformer(IRdfTransformer rdfTransformer)
    {
        _rdfTransformer = rdfTransformer;
    }
    public string Transform(string facilityName)
    {
        var inputDataSet = ShipWeightDBReader.GetData(facilityName);
        var weightTable = inputDataSet.Tables["Weight"] ?? null;

        if (weightTable == null)
        {
            throw new NullReferenceException($"Empty weight table for {facilityName}");
        }

        var facilityPlantId = weightTable.Rows[0]["Plant"].ToString();

        if (string.IsNullOrWhiteSpace(facilityPlantId))
        {
            throw new InvalidOperationException($"Invalid plant id for {facilityName}");
        }

        var provenance = CreateProvenance(facilityName, facilityPlantId);

        return _rdfTransformer.Transform(provenance, inputDataSet);
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