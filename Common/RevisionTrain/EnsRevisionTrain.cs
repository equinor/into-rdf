namespace Common.RevisionTrain
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="DocumentName">A string as defined in TR0052</param>
    /// <param name="ProjectCode">Bård will write something here</param>
    /// <param name="FacilityName">Bård will write something here</param>
    /// <param name="ContractNumber">Contract number as defined in "Bård will write something here" </param>
    /// <param name="DocumentType">Document </param>
    public record EnsRevisionTrain (string DocumentName, string FacilityName, string ProjectCode, string ContractNumber, string DocumentType, string Fuseki = "defualt");
}
