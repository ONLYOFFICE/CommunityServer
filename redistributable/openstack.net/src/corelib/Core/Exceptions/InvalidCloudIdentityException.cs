using System;
using System.Runtime.Serialization;
using net.openstack.Core.Domain;

namespace net.openstack.Core.Exceptions
{
    /// <summary>
    /// The exception thrown when the <see cref="CloudIdentity"/> instance passed
    /// to a provider method is not supported by that provider.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    [Serializable]
    internal class InvalidCloudIdentityException : NotSupportedException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidCloudIdentityException"/> class
        /// with the specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public InvalidCloudIdentityException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidCloudIdentityException"/> class with
        /// serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="info"/> is <see langword="null"/>.</exception>
        protected InvalidCloudIdentityException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
