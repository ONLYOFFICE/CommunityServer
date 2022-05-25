using System;
using System.Runtime.Serialization;

namespace net.openstack.Providers.Rackspace.Exceptions
{
    /// <summary>
    /// Represents errors which occur while validating the ETag following an object transfer.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    [Serializable]
    public class InvalidETagException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidETagException"/> class.
        /// </summary>
        public InvalidETagException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidETagException"/> class with
        /// serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="info"/> is <see langword="null"/>.</exception>
        protected InvalidETagException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
