using System;
using System.Net;

using RestResponse = JSIStudios.SimpleRESTServices.Client.Response;

namespace net.openstack.Core.Exceptions.Response
{
    /// <summary>
    /// Represents errors with status code <see cref="HttpStatusCode.BadRequest"/> resulting
    /// from a call to a REST API.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    [Serializable]
    public class BadServiceRequestException : ResponseException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BadServiceRequestException"/> class with the
        /// specified REST response.
        /// </summary>
        /// <param name="response">The REST response.</param>
        public BadServiceRequestException(RestResponse response)
            : base("Unable to process the service request.  Please try again later.", response)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BadServiceRequestException"/> class with the
        /// specified error message and REST response.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="response">The REST response.</param>
        public BadServiceRequestException(string message, RestResponse response)
            : base(message, response)
        {
        }
    }
}
