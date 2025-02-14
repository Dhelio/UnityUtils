using System;
using System.Runtime.Serialization;

namespace Castrimaris.Core.Exceptions {

    /// <summary>
    /// General exception for when the program expects a type and receives another.
    /// </summary>
    public class TypeMismatchException : Exception {
        public TypeMismatchException() : base() { }
        public TypeMismatchException(string message) : base (message){}
        public TypeMismatchException(string message, Exception innerException) : base(message, innerException) { }
        public TypeMismatchException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext) { }
    }

}