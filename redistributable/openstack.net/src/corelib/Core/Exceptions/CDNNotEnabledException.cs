using System;
using System.Runtime.Serialization;
using net.openstack.Core.Providers;

namespace net.openstack.Core.Exceptions
{
    /// <summary>
    /// The exception that is thrown when an attempt is made to modify CDN properties
    /// of a container which is not CDN-enabled.
    /// </summary>
    /// <seealso cref="IObjectStorageProvider"/>
    /// <threadsafety static="true" instance="false"/>
    [Serializable]
    public class CDNNotEnabledException : InvalidOperationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CDNNotEnabledException"/> class.
        /// </summary>
        public CDNNotEnabledException()
            : base("The specified container is not CDN-enabled.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CDNNotEnabledException"/> class
        /// with the specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public CDNNotEnabledException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CDNNotEnabledException"/> class
        /// with the specified error message and a reference to the inner exception that
        /// is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.
        /// If the <paramref name="innerException"/> parameter is not a null reference, the current
        /// exception is raised in a <b>catch</b> block that handles the inner exception.</param>
        public CDNNotEnabledException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CDNNotEnabledException"/> class with
        /// serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="info"/> is <see langword="null"/>.</exception>
        protected CDNNotEnabledException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }	
}
