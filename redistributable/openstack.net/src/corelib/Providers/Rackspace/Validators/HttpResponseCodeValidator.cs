using System;
using System.Net;
using JSIStudios.SimpleRESTServices.Client;
using net.openstack.Core.Exceptions.Response;
using net.openstack.Core.Validators;

namespace net.openstack.Providers.Rackspace.Validators
{
    /// <summary>
    /// Provides an implementation of <see cref="IHttpResponseCodeValidator"/> for
    /// operation with Rackspace's products.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    public class HttpResponseCodeValidator : IHttpResponseCodeValidator
    {
        /// <summary>
        /// A default instance of <see cref="HttpResponseCodeValidator"/>.
        /// </summary>
        private static readonly HttpResponseCodeValidator _default = new HttpResponseCodeValidator();

        /// <summary>
        /// Gets a default instance of <see cref="HttpResponseCodeValidator"/>.
        /// </summary>
        public static HttpResponseCodeValidator Default
        {
            get
            {
                return _default;
            }
        }

        /// <inheritdoc/>
        public void Validate(Response response)
        {
            if (response == null)
                throw new ArgumentNullException("response");

            if (response.StatusCode <= (HttpStatusCode)299)
                return;

            switch (response.StatusCode)
            {
                case HttpStatusCode.BadRequest:
                    throw new BadServiceRequestException(response);
                case HttpStatusCode.Unauthorized:
                case HttpStatusCode.Forbidden:
                case HttpStatusCode.MethodNotAllowed:
                    throw new UserNotAuthorizedException(response);
                case HttpStatusCode.NotFound:
                    throw new ItemNotFoundException(response);
                case HttpStatusCode.Conflict:
                    throw new ServiceConflictException(response);
                case HttpStatusCode.RequestEntityTooLarge:
                    throw new ServiceLimitReachedException(response);
                case HttpStatusCode.InternalServerError:
                    throw new ServiceFaultException(response);
                case HttpStatusCode.NotImplemented:
                    throw new MethodNotImplementedException(response);
                case HttpStatusCode.ServiceUnavailable:
                    throw new ServiceUnavailableException(response);
                default:
                    throw new ResponseException(string.Format("Unexpected HTTP error: {0}", response.StatusCode), response);
            }
        }
    }
}
