namespace Common.RevisionTrainModels
{
    public class TieContext
    {
        public TieContext(string facilityName, string objectName, string contractNumber)
        {
            FacilityName = facilityName;
            ObjectName = objectName;
            ContractNumber = contractNumber;
        }

        public string FacilityName { get; set; }
        public string ObjectName { get; set; }
        public string ContractNumber { get; set; }
        public string? ProjectCode { get; set; }
        public string? ContractorCode { get; set; }
        public string? DocumentTitle { get; set; }
    }
}
