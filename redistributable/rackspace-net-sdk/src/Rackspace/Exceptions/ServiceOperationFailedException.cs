using System;
using System.Runtime.Serialization;

namespace Rackspace
{
    /// <summary>
    /// The exception that is thrown when a service operation, such as Create, Delete or Update, fails.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    [Serializable]
    public sealed class ServiceOperationFailedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceOperationFailedException"/> class.
        /// </summary>
        public ServiceOperationFailedException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceOperationFailedException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ServiceOperationFailedException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceOperationFailedException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public ServiceOperationFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceOperationFailedException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        private ServiceOperationFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
