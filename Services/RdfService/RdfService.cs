using Azure.Storage.Blobs.Models;
using Common.ProvenanceModels;
using Doc2Rdf.Library.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Services.FusekiService;
using Services.ProvenanceService;
using Services.TieMessageService;

namespace Services.RdfService
{
    public class RdfService : IRdfService
    {
        private readonly IFusekiService _fusekiService;
        private readonly IMelTransformer _melTransformer;
        private readonly IProvenanceService _provenanceService;
        private readonly ITieMessageService _tieMessageService;
        private readonly ILogger<RdfService> _logger;
        public RdfService(IFusekiService fusekiService,
                          IMelTransformer melTransformer,
                          IProvenanceService provenanceService,
                          ITieMessageService tieMessageService,
                          ILogger<RdfService> logger)
        {
            _fusekiService = fusekiService;
            _melTransformer = melTransformer;
            _provenanceService = provenanceService;
            _tieMessageService = tieMessageService;
            _logger = logger;
        }

        public async Task<string> ConvertDocToRdf(IFormFile formFile)
        {
            using var stream = new MemoryStream();
            await formFile.CopyToAsync(stream);
            return _melTransformer.Transform(stream, formFile.FileName);
        }

        public async Task HandleStorageFiles(List<BlobDownloadResult> blobData)
        {
            var tieData = _tieMessageService.ParseXmlMessage(blobData);
            _logger.LogInformation("<RdfService> - HandleStorageFiles: Successfully parsed TIE message for {TieFileData}", tieData.FileData.Name);

            var provenance = await _provenanceService.CreateProvenanceFromTieMessage(tieData);

            switch (provenance.RevisionStatus)
            {
                case RevisionStatus.Old:
                    {
                        _logger.LogError("<RdfService> - HandleStorageFiles: Newer revisions of the submitted TIE data in {TieFileData} exist. Data will not be uploaded", tieData.FileData.Name);
                        return;
                    }
                //TODO - How to handle unknown revisions? Discard or attempt to place them in their proper place in the revision chain.
                //Task created 73431 - Handling previously "unknown" revisions.
                case RevisionStatus.Unknown:
                    {
                        _logger.LogError("<RdfService> - HandleStorageFiles: TIE data in {TieFileData} contains a previously unknown revision that is older than the current latest revision. Data will not be uploaded",
                            tieData.FileData.Name);
                        return;
                    }
                default:
                    break;
            }

            _logger.LogInformation("<RdfService> - HandleStorageFiles: Successfully created provenance information for facility '{FacilityId}' with revision name '{RevisionName}'",
                        provenance.FacilityId, provenance.RevisionName);

            var xlsxBlob = blobData.FirstOrDefault(blob => blob.Details.Metadata["Name"].ToLower().Contains("xlsx"))
                    ?? throw new ArgumentException("Blobdata does not exist");

            string rdfGraphData = _melTransformer.Transform(provenance, xlsxBlob);
            _logger.LogInformation("<RdfService> - HandleStorageFiles: {TieFileName} Successfully transformed to rdf", tieData.FileData.Name);

            //TODO - How to handle updated revisions? I.e. the ones that are seemingly unaltered when looking at the XML. At one point the transformed
            //data should be compared to the data in the Fuseki. What do we do if they differ, add or replace?
            //Task created 73432 - Handle revision "updates"

            //TODO - Push transformed data to Fuseki
            //Task created 73432 - Push transformed data to correct Fuseki instance
            var server = "dugtrio";
            var response = await PostToFusekiAsApp(server, rdfGraphData);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                _logger.LogInformation("<RdfService> - HandleStorageFiles: Successfully uploaded to Fuseki server {name}", server);
            } 
            else
            {
                _logger.LogError("<RdfService> - HandleStorageFiles: Upload to Fuseki server {name} failed", server);
            }
        }

        public async Task<HttpResponseMessage> PostToFusekiAsApp(string server, string data)
        {
            return await _fusekiService.PostAsApp(server, data);
        }

        public async Task<HttpResponseMessage> PostToFusekiAsUser(string server, string data)
        {
            return await _fusekiService.PostAsUser(server, data);
        }

        public async Task<string> QueryFusekiAsUser(string server, string query)
        {
            return await _fusekiService.QueryAsUser(server, query);
        }
    }
}
