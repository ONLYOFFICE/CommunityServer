using System;
using System.Net;

using RestResponse = JSIStudios.SimpleRESTServices.Client.Response;

namespace net.openstack.Core.Exceptions.Response
{
    /// <summary>
    /// Represents errors with status code <see cref="HttpStatusCode.Conflict"/> resulting
    /// from a call to a REST API.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    [Serializable]
    public class ServiceConflictException : ResponseException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceConflictException"/> class with the
        /// specified REST response.
        /// </summary>
        /// <param name="response">The REST response.</param>
        public ServiceConflictException(RestResponse response)
            : base("There was a conflict with the service. Entity may already exist.", response)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceConflictException"/> class with the
        /// specified error message and REST response.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="response">The REST response.</param>
        public ServiceConflictException(string message, RestResponse response)
            : base(message, response)
        {
        }
    }
}
