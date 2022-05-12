using System.Runtime.Serialization;

namespace Common.Exceptions
{
    [Serializable]
    public class FusekiException : Exception
    {
        public FusekiException() { }
        public FusekiException(string message) : base(message) { }
        public FusekiException(string message, Exception inner) : base(message, inner) { }

        protected FusekiException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
