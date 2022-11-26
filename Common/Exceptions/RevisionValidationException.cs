namespace Common.Exceptions;

[Serializable]
public class RevisionValidationException : Exception
{
    public RevisionValidationException() { }
    public RevisionValidationException(string message) : base(message) { }
    public RevisionValidationException(string message, Exception inner) : base(message, inner) { }
}