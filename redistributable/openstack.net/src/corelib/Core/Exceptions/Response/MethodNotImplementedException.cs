using System;
using System.Net;

using RestResponse = JSIStudios.SimpleRESTServices.Client.Response;

namespace net.openstack.Core.Exceptions.Response
{
    /// <summary>
    /// Represents errors with status code <see cref="HttpStatusCode.NotImplemented"/> resulting
    /// from a call to a REST API.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    [Serializable]
    public class MethodNotImplementedException : ResponseException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MethodNotImplementedException"/> class with the
        /// specified REST response.
        /// </summary>
        /// <param name="response">The REST response.</param>
        public MethodNotImplementedException(RestResponse response)
            : base("The requested method is not implemented at the service.", response)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodNotImplementedException"/> class with the
        /// specified error message and REST response.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="response">The REST response.</param>
        public MethodNotImplementedException(string message, RestResponse response)
            : base(message, response)
        {
        }
    }
}
