using VDS.RDF.Shacl.Validation;

namespace Services.ValidationServices.RevisionTrainValidationServices;

public interface IRevisionTrainValidator
{
    Report ValidateRevisionTrain(string turtle);
}