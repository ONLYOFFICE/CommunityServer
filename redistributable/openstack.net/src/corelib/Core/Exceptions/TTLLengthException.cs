using System;
using System.Runtime.Serialization;

namespace net.openstack.Core.Exceptions
{
    /// <summary>
    /// The exception that is thrown when the TTL argument to a method is outside
    /// the range supported by the provider.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    [Serializable]
    public class TTLLengthException : ArgumentException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TTLLengthException"/> class.
        /// </summary>
        public TTLLengthException()
            : base("The specified TTL is outside the range supported by the provider.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TTLLengthException"/> class
        /// with the specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public TTLLengthException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TTLLengthException"/> class with
        /// serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="info"/> is <see langword="null"/>.</exception>
        protected TTLLengthException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
