namespace Doc2Rdf.Library.Models
{
    public class FacilityIdentifiers
    {
        public FacilityIdentifiers(string facilityId = "NA",
                        string sAPPlantId = "NA",
                        string documentProjectId = "NA")
        {
            FacilityId = facilityId;
            SAPPlantId = sAPPlantId;
            DocumentProjectId = documentProjectId;
        }

        public string FacilityId{ get; }

        public string SAPPlantId { get; }

        public string DocumentProjectId {get; }
    }
}