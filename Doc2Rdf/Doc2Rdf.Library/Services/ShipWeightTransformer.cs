using Doc2Rdf.Library.Interfaces;
using Doc2Rdf.Library.Models;
using System;
using System.Data;

namespace Doc2Rdf.Library.Services;

public class ShipWeightTransformer : IShipWeightTransformer
{
    IRdfTransformer _rdfTransformer;
    public ShipWeightTransformer(IRdfTransformer rdfTransformer)
    {
        _rdfTransformer = rdfTransformer;
    }
    public string Transform(string facilityName, string plantId, DataTable inputData)
    {
        var provenance = CreateProvenance(facilityName, inputData.TableName, plantId);

        return _rdfTransformer.Transform(provenance, inputData);
    }

    private Provenance CreateProvenance(string facilityName, string tableName, string plantId)
    {
        var facilityId = GetFacilityId(plantId);
        var facility = new FacilityIdentifiers(facilityId: facilityId, sAPPlantId: plantId);

        var provenance = new Provenance(facility,
                                        facilityName,
                                        "01",
                                        "NA",
                                        DateTime.Now,
                                        DataSource.Shipweight(),
                                        DataSourceType.Database(),
                                        DataFormat.Unknown(),
                                        "NA",
                                        tableName);
        return provenance;
    }

    //Hack to add facilityIds to namespace URIs
    //TODO - Remove when task Feature 65986 - Review - Enrich with Facility Data is implemented
    //https://dev.azure.com/EquinorASA/Spine/_backlogs/backlog/Loudred/Epics/?showParents=true&workitem=65986
    private string GetFacilityId(string plantId)
    {
        switch (plantId)
        {
            case "1219":
                return "aha";
            case "1163":
                return "grd";
            case "1218":
                return "gkr";
            case "1755":
                return "gra";
            case "1930":
                return "jca";
            case "1782":
                return "kra";
            case "1138":
                return "val";
            default:
                throw new ArgumentException("Unknown plantId");
        }
    }
}
