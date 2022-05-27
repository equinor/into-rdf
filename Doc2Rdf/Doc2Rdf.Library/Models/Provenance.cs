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
                          string dataSource,
                          string dataSourceType,
                          string dataFormat,
                          string contractor = "NA",
                          string tableName = "NA")
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
            TableName = tableName.ToLower();
        }

        public FacilityIdentifiers Facility { get; }

        public string DataCollectionName { get; }

        public string RevisionNumber { get; }

        public string RevisionOf { get; }

        public DateTime RevisionDate { get; }

        public string DataSource { get; set; }

        public string DataSourceType { get; set; }

        public string DataFormat { get; set; }
        public string? Contractor { get; set; }
        public string? TableName { get; set; }
    }
}