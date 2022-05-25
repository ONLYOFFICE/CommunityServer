using System;
using System.Linq;
using System.Threading.Tasks;

using Flurl.Http;

using OpenStack;
using OpenStack.Authentication;

// ReSharper disable once CheckNamespace
namespace Flurl.Extensions
{
    /// <summary>
    /// Useful Flurl extension methods for custom implementations.
    /// </summary>
    /// <exclude/>
    public static class FlurlExtensions
    {
        /// <summary>
        /// Converts a <see cref="Url"/> to a <see cref="Uri"/>.
        /// </summary>
        /// <param name="url">The URL.</param>
        public static Uri ToUri(this Url url)
        {
            return new Uri(url.ToString());
        }

        /// <summary>
        /// Removes any query parameters which have a null or empty value.
        /// </summary>
        /// <param name="url">The URL.</param>
        public static Url RemoveNullOrEmptyQueryParams(this string url)
        {
            return new Url(url).RemoveNullOrEmptyQueryParams();    
        }

        /// <summary>
        /// Removes any query parameters which have a null or empty value.
        /// </summary>
        /// <param name="url">The URL.</param>
        public static Url RemoveNullOrEmptyQueryParams(this Url url)
        {
            foreach (var queryParam in url.QueryParams.ToList())
            {
                if (queryParam.Value == null || queryParam.Value.ToString() == string.Empty)
                    url.QueryParams.Remove(queryParam.Name);
            }

            return url;
        }

        /// <summary>
        /// Applies OpenStack authentication to a request.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="authenticationProvider">The authentication provider.</param>
        /// <returns>
        /// An authenticated request.
        /// </returns>
        public static PreparedRequest Authenticate(this string url, IAuthenticationProvider authenticationProvider)
        {
            return new Url(url).Authenticate(authenticationProvider);
        }

        /// <summary>
        /// Applies OpenStack authentication to a request.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="authenticationProvider">The authentication provider.</param>
        /// <returns>
        /// An authenticated request.
        /// </returns>
        public static PreparedRequest Authenticate(this Url url, IAuthenticationProvider authenticationProvider)
        {
            return url.PrepareRequest().Authenticate(authenticationProvider);
        }

        /// <summary>
        /// Builds a prepared Flurl request which can be executed at a later time.
        /// </summary>
        /// <param name="url">The URL.</param>
        public static PreparedRequest PrepareRequest(this string url)
        {
            return new Url(url).PrepareRequest();
        }

        /// <summary>
        /// Builds a prepared Flurl request which can be executed at a later time.
        /// </summary>
        /// <param name="url">The URL.</param>
        public static PreparedRequest PrepareRequest(this Url url)
        {
            return new PreparedRequest(url, autoDispose: true)
            {
                Settings = OpenStackNet.Configuration.FlurlHttpSettings
            };
        }

        /// <summary>
        /// Applies OpenStack authentication to a request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="authenticationProvider">The authentication provider.</param>
        /// <returns>
        /// An authenticated request.
        /// </returns>
        public static PreparedRequest Authenticate(this PreparedRequest request, IAuthenticationProvider authenticationProvider)
        {
            var authenticatedMessageHandler = request.Client.HttpMessageHandler as AuthenticatedMessageHandler;
            if (authenticatedMessageHandler != null)
            {
                authenticatedMessageHandler.AuthenticationProvider = authenticationProvider;
            }
            return request;
        }

        /// <inheritdoc cref="ClientConfigExtensions.WithHeader(FlurlClient,string,object)" />
        public static PreparedRequest WithHeader(this PreparedRequest request, string key, object value)
        {
            ((FlurlRequest)request).WithHeader(key, value);
            return request;
        }

        /// <summary>
        /// Sends the <see cref="PreparedRequest"/>.
        /// </summary>
        /// <param name="requestTask">A task which returns the request.</param>
        /// <returns>The HTTP response message.</returns>
        public static async Task<IFlurlResponse> SendAsync(this Task<PreparedRequest> requestTask)
        {
            PreparedRequest request = await requestTask.ConfigureAwait(false);
            return await request.SendAsync().ConfigureAwait(false);
        }
    }
}
