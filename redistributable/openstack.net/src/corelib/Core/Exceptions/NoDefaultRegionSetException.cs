using System;
using System.Runtime.Serialization;

namespace net.openstack.Core.Exceptions
{
    /// <summary>
    /// The exception that is thrown when a service endpoint could not be obtained because
    /// no region was specified and no default region is available for the provider.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    [Serializable]
    public class NoDefaultRegionSetException : InvalidOperationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NoDefaultRegionSetException"/> class
        /// with the specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public NoDefaultRegionSetException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoDefaultRegionSetException"/> class with
        /// serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="info"/> is <see langword="null"/>.</exception>
        protected NoDefaultRegionSetException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
