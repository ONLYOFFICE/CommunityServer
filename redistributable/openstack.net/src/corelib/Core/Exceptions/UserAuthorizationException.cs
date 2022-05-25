using System;
using System.Net;
using System.Runtime.Serialization;
using net.openstack.Core.Domain;

namespace net.openstack.Core.Exceptions
{
    /// <summary>
    /// The exception that is thrown when a user does not have access to a REST API method
    /// (status code <see cref="HttpStatusCode.Forbidden"/>), or an endpoint could not be
    /// obtained for a particular provider because it was not present in the user's
    /// <see cref="UserAccess.ServiceCatalog"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    [Serializable]
    public class UserAuthorizationException : InvalidOperationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserAuthorizationException"/> class
        /// with the specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public UserAuthorizationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserAuthorizationException"/> class with
        /// serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="info"/> is <see langword="null"/>.</exception>
        protected UserAuthorizationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
