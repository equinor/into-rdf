namespace Common.Exceptions;

[Serializable]
public class UnsupportedContentTypeException : Exception
{
    public UnsupportedContentTypeException() { }
    public UnsupportedContentTypeException(string message) : base(message) { }
    public UnsupportedContentTypeException(string message, Exception inner) : base(message, inner) { }
}