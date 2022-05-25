using System;
using System.Net;

using RestResponse = JSIStudios.SimpleRESTServices.Client.Response;

namespace net.openstack.Core.Exceptions.Response
{
    /// <summary>
    /// Represents errors with status code <see cref="HttpStatusCode.RequestEntityTooLarge"/> resulting
    /// from a call to a REST API.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    [Serializable]
    public class ServiceLimitReachedException : ResponseException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceLimitReachedException"/> class with the
        /// specified REST response.
        /// </summary>
        /// <param name="response">The REST response.</param>
        public ServiceLimitReachedException(RestResponse response)
            : base("The service rate limit has been reached. Either request a service limit increase or wait until the limit resets.", response)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceLimitReachedException"/> class with the
        /// specified error message and REST response.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="response">The REST response.</param>
        public ServiceLimitReachedException(string message, RestResponse response)
            : base(message, response)
        {
        }
    }
}
