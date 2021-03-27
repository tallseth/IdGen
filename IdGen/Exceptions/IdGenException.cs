using System;

namespace IdGen.Exceptions
{
    /// <summary>
    /// Marker Exception type, base class for all exceptions from this library
    /// </summary>
    public class IdGenException : Exception
    {
        public IdGenException() { }
        
        public IdGenException(string message) : base(message) { }
        
        public IdGenException(string message, Exception innerException) : base(message, innerException) { }
    }
}