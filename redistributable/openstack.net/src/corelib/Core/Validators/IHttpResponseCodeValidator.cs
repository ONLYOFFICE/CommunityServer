using System;
using JSIStudios.SimpleRESTServices.Client;
using net.openstack.Core.Exceptions.Response;

namespace net.openstack.Core.Validators
{
    /// <summary>
    /// Represents an object that can determine whether a particular response from a
    /// REST API call succeeded.
    /// </summary>
    public interface IHttpResponseCodeValidator
    {
        /// <summary>
        /// Verifies that <paramref name="response"/> represents a successful response
        /// for a REST API call, throwing an appropriate <see cref="ResponseException"/>
        /// if <paramref name="response"/> indicates a failure.
        /// </summary>
        /// <param name="response">The response from the REST call.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="response"/> is <see langword="null"/>.</exception>
        /// <exception cref="ResponseException">If <paramref name="response"/> indicates the REST API call failed.</exception>
        void Validate(Response response);
    }
}
