namespace Common.RevisionTrainModels
{
    /// <summary>
    /// A revision train is a versional object containing common information about all the revisions it refers to.
    /// </summary>
    /// <param name="DocumentName">The name of the revision train based on the document</param>
    /// <param name="FacilityName">The facility the revision train belongs to</param>
    /// <param name="ProjectCode">The project code the revision train belongs to</param>
    /// <param name="ContractNumber">The contract the revision train belongs to</param>
    /// <param name="TripleStore">The triple store where the revision train is stored</param>
    public class RevisionTrainModel
    {
        public RevisionTrainModel(){}
        
        public string? DocumentName { get; set; }
        public string? FacilityName { get; set; }
        public string? ProjectCode { get; set; }
        public string? ContractNumber {get; set; }
        public string? TripleStore { get; set; }
    }
}
