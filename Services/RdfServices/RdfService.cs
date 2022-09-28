
using Azure.Storage.Blobs.Models;
using Common.GraphModels;
using Common.ProvenanceModels;
using Common.SpreadsheetModels;
using Common.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Services.FusekiServices;
using Services.OntologyServices.OntologyService;
using Services.ProvenanceServices;
using Services.TieMessageServices;
using Services.TransformationServices.SpreadsheetTransformationServices;
using Services.TransformationServices.XMLTransformationServices;
using System.Net;

namespace Services.RdfServices
{
    public class RdfService : IRdfService
    {
        private readonly IFusekiService _fusekiService;
        private readonly IEnumerable<ISpreadsheetTransformationService> _spreadsheetTransformationService;
        private readonly IEnumerable<IXMLTransformationService> _xmlTransformationService;
        private readonly IOntologyService _ontologyService; 
        private readonly IProvenanceService _provenanceService;
        private readonly ITieMessageService _tieMessageService;
        private readonly ILogger<RdfService> _logger;
        public RdfService(IFusekiService fusekiService,
                          IEnumerable<ISpreadsheetTransformationService> spreadsheetTransformationService,
                          IEnumerable<IXMLTransformationService> xmlTransformationService,
                          IOntologyService ontologyService,
                          IProvenanceService provenanceService,
                          ITieMessageService tieMessageService,
                          ILogger<RdfService> logger)
        {
            _fusekiService = fusekiService;
            _spreadsheetTransformationService = spreadsheetTransformationService;
            _xmlTransformationService = xmlTransformationService;
            _ontologyService = ontologyService;
            _provenanceService = provenanceService;
            _tieMessageService = tieMessageService;
            _logger = logger;
        }

        //TODO - The method is used to upload an Excel file to a Fuseki.
        //However it is hardcoded to only accept a "MEL", which will, among other things, result in a mel-namespace for the resulting turtle.
        //Follow up in task https://dev.azure.com/EquinorASA/Spine/_workitems/edit/75590/
        public async Task<string> ConvertDocToRdf(IFormFile formFile)
        {
            using var stream = new MemoryStream();
            await formFile.CopyToAsync(stream);
            var ontology = await _ontologyService.GetSourceOntologies(DataSource.Mel);

            var transformer = _spreadsheetTransformationService.FirstOrDefault(transformer => transformer.GetDataSource() == DataSource.Mel) ??
                                    throw new ArgumentException($"A transformer of type {DataSource.Mel} is not available to RdfService");

            var resultGraph =  transformer.Transform(stream, ontology, formFile.FileName);

            return resultGraph.Content;
        }

        public async Task<Provenance?> HandleTieRequest(string datasource, List<BlobDownloadResult> blobData)
        {
            var tieData = _tieMessageService.ParseXmlMessage(blobData);
            _logger.LogInformation("<RdfService> - HandleTieRequest: Successfully parsed TIE message for {TieFileData}", tieData.GetDataCollectionName());

            var ontology = await _ontologyService.GetSourceOntologies(datasource);
            var provenance = await _provenanceService.CreateProvenanceFromTieMessage(datasource, tieData);

            if (!IsProvenanceValid(provenance, tieData.GetDataCollectionName()))
            {
                return null;
            }

            _logger.LogInformation("<RdfService> - HandleTieRequest: Successfully created provenance information for facility '{FacilityId}' with revision name '{RevisionName}'",
                        provenance.FacilityId, provenance.RevisionName);

            var xlsxBlob = blobData.FirstOrDefault(blob => blob.Details.Metadata["Name"].ToLower().Contains("xlsx"))
                    ?? throw new ArgumentException("Blobdata does not exist");

            var transformationService = GetTransformationService(datasource);

            var spreadsheetInfo = new SpreadsheetInfo();
            spreadsheetInfo.DataSource = provenance.DataSource;

            ResultGraph rdfGraphData = transformationService.Transform(provenance, ontology, xlsxBlob, spreadsheetInfo.GetSpreadSheetDetails());
            _logger.LogInformation("<RdfService> - HandleTieRequest: {TieFileName} Successfully transformed to rdf", tieData.GetDataCollectionName());

            var response = await PostToFusekiAsApp(ServerKeys.Dugtrio, rdfGraphData, "text/turtle");
            LogResponse(response, ServerKeys.Dugtrio);

            return provenance;
        }

        public async Task<string> HandleSpreadsheetRequest(SpreadsheetInfo info, BlobDownloadResult blobData)
        {
            var provenance = await _provenanceService.CreateProvenanceFromSpreadsheetInfo(info);
            
            if (!IsProvenanceValid(provenance, info?.FileName ?? "Unknown"))
            {
                return String.Empty;
            }

            var ontology = await _ontologyService.GetSourceOntologies(provenance.DataSource);
            var datasource = info?.DataSource ?? throw new InvalidOperationException("Spreadsheet info doesn't contain datasource");

            var transformationService = GetTransformationService(datasource);
            _logger.LogDebug("RdfService> - HandleSpreadsheetRequest: Using transformation service: {name}", transformationService.GetDataSource());

            var resultGraph = transformationService.Transform(provenance, ontology, blobData, info.GetSpreadSheetDetails());

            var response = await PostToFusekiAsApp(ServerKeys.Dugtrio, resultGraph, "text/turtle");
            LogResponse(response, ServerKeys.Dugtrio);

            return resultGraph.Content;
        }

        public async Task<HttpResponseMessage> PostToFusekiAsUser(string server, string data, string contentType)
        {
            return await _fusekiService.PostAsUser(server, data, contentType);
        }

        public async Task<HttpResponseMessage> QueryFusekiAsUser(string server, string query)
        {
            return await _fusekiService.QueryAsUser(server, query);
        }

        private async Task<HttpResponseMessage> PostToFusekiAsApp(string server, ResultGraph resultGraph, string contentType)
        {
            return await _fusekiService.PostAsApp(server, resultGraph, contentType);
        }

        private ISpreadsheetTransformationService GetTransformationService(string datasource)
        {
            return _spreadsheetTransformationService.FirstOrDefault(transformer => transformer.GetDataSource() == datasource) ??
                throw new ArgumentException($"A transformer of type {datasource} is not available to RdfService");
        }

        private bool IsProvenanceValid(Provenance provenance, string fileName)
        { 
            switch (provenance.RevisionStatus)
            {
                case RevisionStatus.Old:
                    {
                        _logger.LogError("<RdfService> - IsProvenanceValid: Newer revisions of the submitted data from {file} exist. The submitted data will not be uploaded", fileName);
                        return false;
                    }

                //TODO - How to handle unknown revisions? Discard or attempt to place them in their proper place in the revision chain.
                //FOllow up in task: https://dev.azure.com/EquinorASA/Spine/_workitems/edit/73431
                case RevisionStatus.Unknown:
                    {
                        _logger.LogError("<RdfService> - IsProvenanceValid: Data from {file} contains a previously unknown revision that is older than the current latest revision. The submitted data will not be uploaded",
                            fileName);
                        return false;
                    }

                //TODO - How to handle updated revisions? I.e. the ones that are seemingly unaltered when looking at the XML. At one point the transformed
                //data should be compared to the data in the Fuseki. What do we do if they differ, add or replace?
                //https://dev.azure.com/EquinorASA/Spine/_workitems/edit/73433/
                case RevisionStatus.Update:
                    {
                        _logger.LogWarning("<RdfService> - IsProvenanceValid: The submitted data from {file} appears to be an update to an existing revision. A new revision should be created. The submitted data will not be uploaded",
                            fileName);
                        return false;
                    }
                default:
                    return true;
            }
        }

        private void LogResponse(HttpResponseMessage responseMessage, string serverName)
        {
            if (responseMessage.IsSuccessStatusCode)
            {
                _logger.LogInformation("<RdfService> - LogResponse: Successfully uploaded to Fuseki server {name}", serverName);
            }
            else
            {
                _logger.LogError("<RdfService> - LogResponse: Upload to Fuseki server {name} failed with code {code}", serverName, responseMessage.StatusCode);
            }
        }
    }
}
