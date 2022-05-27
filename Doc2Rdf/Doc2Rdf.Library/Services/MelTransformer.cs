using Doc2Rdf.Library.Interfaces;
using Doc2Rdf.Library.Models;
using System;
using System.Data;
using System.IO;

namespace Doc2Rdf.Library.Services;

public class MelTransformer : IMelTransformer
{
    private IMelReader _melReader;
    private IRdfTransformer _rdfTransformer;

    public MelTransformer(IMelReader melReader, IRdfTransformer rdfTransformer)
    {
        _melReader = melReader;
        _rdfTransformer = rdfTransformer;
    }

    public string Transform(Stream excelStream, string fileName)
    {
        var details = _melReader.GetSpreadsheetDetails(excelStream, fileName);

        return Transform(excelStream, details);
    }

    public string Transform(Stream excelStream, SpreadsheetDetails details)
    {
        var data = _melReader.GetSpreadsheetData(excelStream, details);
        var provenance = CreateProvenance(details);

        return _rdfTransformer.Transform(provenance, data);
    }

    private Provenance CreateProvenance(SpreadsheetDetails details)
    {
        var facilityId = GetFacilityId(details.ProjectCode);
        var facility = new FacilityIdentifiers(facilityId: facilityId, documentProjectId: details.ProjectCode);

        var previousRevision = details.Revision > 1 ? $"{(details.Revision - 1).ToString("D2")}" : string.Empty;

        var provenance = new Provenance(facility,
                                        details.FileName,
                                        details.Revision.ToString("D2"),
                                        previousRevision,
                                        details.RevisionDate,
                                        DataSource.Mel(),
                                        DataSourceType.File(),
                                        DataFormat.Xlsx());

        return provenance;
    }

    //Hack to add facilityIds to namespace URIs
    //TODO - Remove when task Feature 65986 - Review - Enrich with Facility Data is implemented
    //https://dev.azure.com/EquinorASA/Spine/_backlogs/backlog/Loudred/Epics/?showParents=true&workitem=65986
    private string GetFacilityId(string projectId)
    {
        switch (projectId.ToLower())
        {
            case "c232":
                return "kra";
            case "c277":
                return "wist";
            default:
                throw new ArgumentException("Unknown projectId");
        }
    }
}