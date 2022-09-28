using Azure.Storage.Blobs.Models;
using Common.GraphModels;
using Common.ProvenanceModels;
using Common.SpreadsheetModels;
using Microsoft.Extensions.Logging;
using Services.DomReaderServices.ExcelDomReaderServices;
using Services.TransformationServices.RdfTransformationServices;
using VDS.RDF;  

namespace Services.TransformationServices.SpreadsheetTransformationServices;

public class MelTransformationService : ISpreadsheetTransformationService
{

    private readonly IExcelDomReaderService _excelDomReaderService;
    private readonly IRdfTransformationService _rdfTransformerService;
    private readonly ILogger<MelTransformationService> _logger;
    private string _dataSource;

    public MelTransformationService(IExcelDomReaderService excelDomReaderService, IRdfTransformationService rdfTransformerService, ILogger<MelTransformationService> logger)
    {
        _excelDomReaderService = excelDomReaderService;
        _rdfTransformerService = rdfTransformerService;
        _logger = logger;
        _dataSource = DataSource.Mel;
    }

    public ResultGraph Transform(Provenance provenance, Graph ontology, BlobDownloadResult blob, SpreadsheetDetails details)
    {
        using Stream excelStream = blob.Content.ToStream();

        _logger.LogInformation("<MelTransformer> - Transform: Starting parsing of spreadsheet data from TIE message");

        var data = _excelDomReaderService.GetSpreadsheetData(excelStream, details);

        _logger.LogInformation("<MelTransformer> - Transform: Spreadsheet table with {numberOfColumns} columns and {numberOfRows} rows retrieved", data.Columns.Count, data.Rows.Count);

        return _rdfTransformerService.Transform(provenance, ontology, data);
    }

    public ResultGraph Transform(Stream excelStream, Graph ontology, string fileName)
    {
        var spreadsheetInfo = _excelDomReaderService.GetSpreadsheetInfo(excelStream, fileName);

        return Transform(excelStream, ontology, spreadsheetInfo);
    }

    public ResultGraph Transform(Stream excelStream, Graph ontology, SpreadsheetInfo spreadsheetInfo)
    {
        var spreadsheetDetails = spreadsheetInfo.GetSpreadSheetDetails();
        var data = _excelDomReaderService.GetSpreadsheetData(excelStream, spreadsheetDetails);
        var provenance = CreateProvenance(spreadsheetInfo);

        return _rdfTransformerService.Transform(provenance, ontology, data);
    }
    
    public string GetDataSource()
    {
        return _dataSource;
    }

    private Provenance CreateProvenance(SpreadsheetInfo details)
    {
        var facilityId = details.DocumentProjectId != null ?
            GetFacilityId(details.DocumentProjectId) :
            throw new ArgumentNullException("Spreadsheet information does not contain facility Id");

        var previousRevision = details.Revision > 1 ? $"{(details.Revision - 1).ToString("D2")}" : string.Empty;

        var provenance = new Provenance(facilityId, DataSource.Mel);

        provenance.DocumentProjectId = details.DocumentProjectId;
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

    //Hack to add facilityIds to namespace URIs
    //TODO - Remove when task Feature 65986 - Review - Enrich with Facility Data is implemented
    //https://dev.azure.com/EquinorASA/Spine/_backlogs/backlog/Loudred/Epics/?showParents=true&workitem=65986
    //NOTE - After moving Excel2Rdf.Cli to LocalFunctions only the Splinter API relies on this.
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