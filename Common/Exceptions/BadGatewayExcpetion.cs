namespace Common.Exceptions;

[Serializable]
public class BadGatewayException : Exception
{
    public BadGatewayException() { }
    public BadGatewayException(string message) : base(message) { }
    public BadGatewayException(string message, Exception inner) : base(message, inner) { }
}