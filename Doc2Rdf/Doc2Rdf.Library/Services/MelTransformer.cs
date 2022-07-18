using Azure.Storage.Blobs.Models;
using Common.ProvenanceModels;
using Common.SpreadsheetModels;
using Doc2Rdf.Library.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace Doc2Rdf.Library.Services;

public class MelTransformer : IMelTransformer
{
    private readonly IMelReader _melReader;
    private readonly IRdfTransformer _rdfTransformer;
    private readonly ILogger<MelTransformer> _logger;

    public MelTransformer(IMelReader melReader, IRdfTransformer rdfTransformer, ILogger<MelTransformer> logger)
    {
        _melReader = melReader;
        _rdfTransformer = rdfTransformer;
        _logger = logger;
    }

    public string Transform(Provenance provenance, BlobDownloadResult blob)
    {
        using Stream excelStream = blob.Content.ToStream();

        var details = GetDefaultSpreadsheetDetailsForMel();

        _logger.LogInformation("<MelTransformer> - Transform: Starting parsing of spreadsheet data from TIE message");

        var data = _melReader.GetSpreadsheetData(excelStream, details);

        _logger.LogInformation("<MelTransformer> - Transform: Spreadsheet table with {numberOfColumns} columns and {numberOfRows} rows retrieved", data.Columns.Count, data.Rows.Count);

        return _rdfTransformer.Transform(provenance, data);
    }

    public string Transform(Stream excelStream, string fileName)
    {
        var spreadsheetInfo = _melReader.GetSpreadsheetInfo(excelStream, fileName);

        return Transform(excelStream, spreadsheetInfo);
    }

    public string Transform(Stream excelStream, SpreadsheetInfo spreadsheetInfo)
    {
        var spreadsheetDetails = spreadsheetInfo.GetSpreadSheetDetails();
        var data = _melReader.GetSpreadsheetData(excelStream, spreadsheetDetails);
        var provenance = CreateProvenance(spreadsheetInfo);

        return _rdfTransformer.Transform(provenance, data);
    }

    private Provenance CreateProvenance(SpreadsheetInfo details)
    {
        var facilityId = details.ProjectCode != null ?
            GetFacilityId(details.ProjectCode) :
            throw new ArgumentNullException("Spreadsheet information does not contain facility Id");

        var previousRevision = details.Revision > 1 ? $"{(details.Revision - 1).ToString("D2")}" : string.Empty;

        var provenance = new Provenance(facilityId, DataSource.Mel());

        provenance.DocumentProjectId = details.ProjectCode;
        provenance.PlantId = "na";
        provenance.DataCollectionName = details.FileName;
        provenance.RevisionName = details.Revision.ToString("D2");
        provenance.RevisionNumber = details.Revision;
        provenance.RevisionDate = details.RevisionDate;
        provenance.PreviousRevisionNumber = previousRevision;
        provenance.DataSourceType = DataSourceType.File();
        provenance.Contractor = details.Contractor;
        provenance.RevisionStatus = RevisionStatus.New;

        return provenance;
    }

    private SpreadsheetDetails GetDefaultSpreadsheetDetailsForMel()
    {
        const string sheetName = "MEL";
        const int headerRow = 6;
        const int dataStartRow = 8;
        const int startColumn = 1;

        return new SpreadsheetDetails(sheetName, headerRow, dataStartRow, startColumn);
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