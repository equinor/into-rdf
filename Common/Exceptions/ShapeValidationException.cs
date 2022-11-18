
using System.Runtime.Serialization;

namespace Common.Exceptions;

[Serializable]
public class ShapeValidationException : Exception
{
    public ShapeValidationException() { }
    public ShapeValidationException(string message) : base(message) { }
    public ShapeValidationException(string message, Exception inner) : base(message, inner) { }
}