
using System.Runtime.Serialization;

namespace Common.Exceptions;

[Serializable]
public class ConflictOnInsertException : Exception
{
    public ConflictOnInsertException() { }
    public ConflictOnInsertException(string message) : base(message) { }
    public ConflictOnInsertException(string message, Exception inner) : base(message, inner) { }
}