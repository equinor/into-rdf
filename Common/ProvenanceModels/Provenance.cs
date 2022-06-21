using System;
using System.IO;

namespace Common.ProvenanceModels
{
    public class Provenance
    {
        public Provenance(string facilityId, string dataSource)
        {
            FacilityId = facilityId;
            DataSource = dataSource;
        }

        public string FacilityId { get; }
        public string? DocumentProjectId { get; set; }
        public string? PlantId { get; set; }
        public string? DataCollectionName { get; set; }
        public string? RevisionName { get; set; }
        public int RevisionNumber { get; set; }
        public string? PreviousRevisionNumber { get; set; }
        public Uri? PreviousRevision { get; set; }
        public DateTime RevisionDate { get; set; }
        public string DataSource { get; }
        public string? DataSourceType { get; set; }
        public string? DataSourceTable { get; set; }
        public string? Contractor { get; set; }
        public RevisionStatus RevisionStatus { get; set; }
    }
}