using System;
using System.Net;

using RestResponse = JSIStudios.SimpleRESTServices.Client.Response;

namespace net.openstack.Core.Exceptions.Response
{
    /// <summary>
    /// Represents errors with status code <see cref="HttpStatusCode.Unauthorized"/>,
    /// <see cref="HttpStatusCode.Forbidden"/>, or <see cref="HttpStatusCode.MethodNotAllowed"/>
    /// resulting from a call to a REST API.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    [Serializable]
    public class UserNotAuthorizedException : ResponseException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserNotAuthorizedException"/> class with the
        /// specified REST response.
        /// </summary>
        /// <param name="response">The REST response.</param>
        public UserNotAuthorizedException(RestResponse response)
            : base("Unable to authenticate user and retrieve authorized service endpoints", response)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserNotAuthorizedException"/> class with the
        /// specified error message and REST response.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="response">The REST response.</param>
        public UserNotAuthorizedException(string message, RestResponse response)
            : base(message, response)
        {
        }
    }
}
