using System;
using System.IO;

namespace Doc2Rdf.Library.Models
{
    public class Provenance
    {
        public Provenance(FacilityIdentifiers facility,
                          string dataCollectionName,
                          string revisionNumber,
                          string revisionOf,
                          DateTime revisionDate,
                          DataSource dataSource,
                          DataSourceType dataSourceType,
                          DataFormat dataFormat,
                          string contractor = "NA")
        {
            Facility = facility;
            DataCollectionName = dataCollectionName;
            RevisionNumber = revisionNumber;
            RevisionOf = revisionOf;
            RevisionDate = revisionDate;
            DataSource = dataSource;
            DataSourceType = dataSourceType;
            DataFormat = dataFormat;
            Contractor = contractor;
        }

        public FacilityIdentifiers Facility { get; }

        public string DataCollectionName { get; }

        public string RevisionNumber { get; }

        public string RevisionOf { get; }

        public DateTime RevisionDate { get; }

        public DataSource DataSource { get; set; }

        public DataSourceType DataSourceType { get; set; }

        public DataFormat DataFormat { get; set; }
        public string? Contractor { get; set; }
    }
}