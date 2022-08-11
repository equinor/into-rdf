using Azure.Storage.Blobs.Models;
using Common.ProvenanceModels;
using Common.SpreadsheetModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Services.FusekiServices;
using Services.ProvenanceServices;
using Services.TieMessageServices;
using Services.TransformationServices.SpreadsheetTransformationServices;
using Services.TransformationServices.XMLTransformationServices;

namespace Services.RdfServices
{
    public class RdfService : IRdfService
    {
        private readonly IFusekiService _fusekiService;
        private readonly IEnumerable<ISpreadsheetTransformationService> _spreadsheetTransformationService;
        private readonly IEnumerable<IXMLTransformationService> _xmlTransformationService;
        private readonly IProvenanceService _provenanceService;
        private readonly ITieMessageService _tieMessageService;
        private readonly ILogger<RdfService> _logger;
        public RdfService(IFusekiService fusekiService,
                          IEnumerable<ISpreadsheetTransformationService> spreadsheetTransformationService,
                          IEnumerable<IXMLTransformationService> xmlTransformationService,
                          IProvenanceService provenanceService,
                          ITieMessageService tieMessageService,
                          ILogger<RdfService> logger)
        {
            _fusekiService = fusekiService;
            _spreadsheetTransformationService = spreadsheetTransformationService;
            _xmlTransformationService = xmlTransformationService;
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
            var transformer = _spreadsheetTransformationService.FirstOrDefault(transformer => transformer.GetDataSource() == DataSource.Mel()) ??
                                    throw new ArgumentException($"A transformer of type {DataSource.Mel()} is not available to RdfService");

            return transformer.Transform(stream, formFile.FileName);
        }

        public async Task<Provenance?> HandleTieRequest(string datasource, List<BlobDownloadResult> blobData)
        {
            var tieData = _tieMessageService.ParseXmlMessage(blobData);
            _logger.LogInformation("<RdfService> - HandleTieRequest: Successfully parsed TIE message for {TieFileData}", tieData.GetDataCollectionName());

            var provenance = await _provenanceService.CreateProvenanceFromTieMessage(datasource, tieData);

            switch (provenance.RevisionStatus)
            {
                case RevisionStatus.Old:
                    {
                        _logger.LogError("<RdfService> - HandleTieRequest: Newer revisions of the submitted TIE data from {TieFileData} exist. The submitted data will not be uploaded", tieData.GetDataCollectionName());
                        return null;
                    }

                //TODO - How to handle unknown revisions? Discard or attempt to place them in their proper place in the revision chain.
                //FOllow up in task: https://dev.azure.com/EquinorASA/Spine/_workitems/edit/73431
                case RevisionStatus.Unknown:
                    {
                        _logger.LogError("<RdfService> - HandleTieRequest: TIE data from {TieFileData} contains a previously unknown revision that is older than the current latest revision. The submitted data will not be uploaded",
                            tieData.GetDataCollectionName());
                        return null;
                    }

                //TODO - How to handle updated revisions? I.e. the ones that are seemingly unaltered when looking at the XML. At one point the transformed
                //data should be compared to the data in the Fuseki. What do we do if they differ, add or replace?
                //https://dev.azure.com/EquinorASA/Spine/_workitems/edit/73433/
                case RevisionStatus.Update:
                    {
                        _logger.LogWarning("<RdfService> - HandleTieRequest: The submitted TIE data from {TieFileData} appears to be an update to an existing revision. A new revision should be created. The submitted data will not be uploaded",
                            tieData.GetDataCollectionName());
                        return null;
                    }
                default:
                    break;
            }

            _logger.LogInformation("<RdfService> - HandleTieRequest: Successfully created provenance information for facility '{FacilityId}' with revision name '{RevisionName}'",
                        provenance.FacilityId, provenance.RevisionName);

            var xlsxBlob = blobData.FirstOrDefault(blob => blob.Details.Metadata["Name"].ToLower().Contains("xlsx"))
                    ?? throw new ArgumentException("Blobdata does not exist");

            var transformationService = GetTransformationService(datasource);

            string rdfGraphData = transformationService.Transform(provenance, xlsxBlob);
            _logger.LogInformation("<RdfService> - HandleTieRequest: {TieFileName} Successfully transformed to rdf", tieData.GetDataCollectionName());

            //TODO - Push transformed data to Fuseki
            //https://dev.azure.com/EquinorASA/Spine/_workitems/edit/73432
            var server = "dugtrio";
            var response = await PostToFusekiAsApp(server, rdfGraphData, "text/turtle");

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                _logger.LogInformation("<RdfService> - HandleTieRequest: Successfully uploaded to Fuseki server {name}", server);
            }
            else
            {
                _logger.LogError("<RdfService> - HandleTieRequest: Upload to Fuseki server {name} failed", server);
            }

            return provenance;
        }

        public async Task<string> HandleSpreadsheetRequest(SpreadsheetInfo info, BlobDownloadResult blobData)
        {
            var provenance = await _provenanceService.CreateProvenanceFromSpreadsheetInfo(info);

            var datasource = info.DataSource ?? throw new InvalidOperationException("Spreadsheet info doesn't contain datasource");

            var transformationService = GetTransformationService(datasource);
            string rdfGraphData = transformationService.Transform(provenance, blobData);

            return rdfGraphData;
        }

        public async Task<HttpResponseMessage> PostToFusekiAsApp(string server, string data, string contentType)
        {
            return await _fusekiService.PostAsApp(server, data, contentType);
        }

        public async Task<HttpResponseMessage> PostToFusekiAsUser(string server, string data, string contentType)
        {
            return await _fusekiService.PostAsUser(server, data, contentType);
        }

        public async Task<string> QueryFusekiAsUser(string server, string query)
        {
            return await _fusekiService.QueryAsUser(server, query);
        }

        private ISpreadsheetTransformationService GetTransformationService(string datasource)
        {
            return _spreadsheetTransformationService.FirstOrDefault(transformer => transformer.GetDataSource() == datasource) ??
                throw new ArgumentException($"A transformer of type {datasource} is not available to RdfService");
        }

    }
}
