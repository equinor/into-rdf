using System;
using System.IO;

namespace Common.ProvenanceModels
{
    public class Provenance
    {
        public string? FacilityId { get; init; }
        public string? DocumentProjectId { get; init; }
        public string? PlantId { get; init; }
        public string? DataCollectionName { get; init; }
        public string? RevisionName { get; init; }
        public int RevisionNumber { get; init; }
        public Uri? PreviousRevision { get; init; }
        public DateTime RevisionDate { get; init; }
        public string? DataSource { get; init; }
        public string? DataSourceType { get; init; }
        public string? DataSourceTable { get; init; }
        public string? Contractor { get; init; }
        public string? RevisionStatus { get; init; }
    }
}