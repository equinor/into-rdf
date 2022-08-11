using System;
using System.IO;

namespace Common.ProvenanceModels
{
    public class AMlProvenance
    {
        public AMlProvenance(string facilityId, string dataSource)
        {
            FacilityId = facilityId;
            DataSource = dataSource;
        }

        public string FacilityId { get; }
        public string DocumentProjectId { get; set; } = "N/A";
        public string PlantId { get; set; } = "N/A";
        public string? WriterName { get; set; }
        public string? WriterVendor { get; set; }
        public string? WriterID { get; set; }
        public string? WriterProjectTitle { get; set; }
        public string? WriterProjectID { get; set; }
        public string? AMLDocumentIdentifier { get; set; }
        public string? WriterVendorURL { get; set; }
        public DateTime WriterLastWritingDateUTC0 { get; set; }
        public string? DataCollectionName { get; set; }
        public string? RevisionName { get; set; }
        public int RevisionNumber { get; set; }
        public string? PreviousRevisionNumber { get; set; }
        public Uri? PreviousRevision { get; set; }
        public DateTime RevisionDate { get; set; }
        public string DataSource { get; }
        public string? Contractor { get; set; }
        public RevisionStatus RevisionStatus { get; set; }
    }
}