namespace Common.Exceptions;

[Serializable]
public class RevisionTrainValidationException : Exception
{
    public RevisionTrainValidationException() { }
    public RevisionTrainValidationException(string message) : base(message) { }
    public RevisionTrainValidationException(string message, Exception inner) : base(message, inner) { }
}