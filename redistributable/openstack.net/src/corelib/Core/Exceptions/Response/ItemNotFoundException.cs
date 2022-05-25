using System;
using System.Net;

using RestResponse = JSIStudios.SimpleRESTServices.Client.Response;

namespace net.openstack.Core.Exceptions.Response
{
    /// <summary>
    /// Represents errors with status code <see cref="HttpStatusCode.NotFound"/> resulting
    /// from a call to a REST API.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    [Serializable]
    public class ItemNotFoundException : ResponseException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemNotFoundException"/> class with the
        /// specified REST response.
        /// </summary>
        /// <param name="response">The REST response.</param>
        public ItemNotFoundException(RestResponse response)
            : base("The item was not found or does not exist.", response)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemNotFoundException"/> class with the
        /// specified error message and REST response.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="response">The REST response.</param>
        public ItemNotFoundException(string message, RestResponse response)
            : base(message, response)
        {
        }
    }
}
