using System;
using System.Net;

using RestResponse = JSIStudios.SimpleRESTServices.Client.Response;

namespace net.openstack.Core.Exceptions.Response
{
    /// <summary>
    /// Represents errors with status code <see cref="HttpStatusCode.InternalServerError"/> resulting
    /// from a call to a REST API.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    [Serializable]
    public class ServiceFaultException : ResponseException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceFaultException"/> class with the
        /// specified REST response.
        /// </summary>
        /// <param name="response">The REST response.</param>
        public ServiceFaultException(RestResponse response)
            : base("There was an unhandled error at the service endpoint.  Please try again later.", response)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceFaultException"/> class with the
        /// specified error message and REST response.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="response">The REST response.</param>
        public ServiceFaultException(string message, RestResponse response)
            : base(message, response)
        {
        }
    }
}
