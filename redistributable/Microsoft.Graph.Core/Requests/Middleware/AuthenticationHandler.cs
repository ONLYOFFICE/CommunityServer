// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    /// <summary>
    /// A <see cref="DelegatingHandler"/> implementation using standard .NET libraries.
    /// </summary>
    public class AuthenticationHandler : DelegatingHandler
    {
        /// <summary>
        /// MaxRetry property for 401's
        /// </summary>
        private int MaxRetry { get; set; } = 1;

        /// <summary>
        /// AuthOption property
        /// </summary>
        internal AuthenticationHandlerOption AuthOption { get; set; }

        /// <summary>
        /// AuthenticationProvider property
        /// </summary>
        public IAuthenticationProvider AuthenticationProvider { get; set; }

        /// <summary>
        /// Construct a new <see cref="AuthenticationHandler"/>
        /// <param name="authenticationProvider">An authentication provider to pass to <see cref="AuthenticationHandler"/> for authenticating requests.</param>
        /// </summary>
        /// <param name="authOption">An OPTIONAL <see cref="Microsoft.Graph.AuthenticationHandlerOption"/> to configure <see cref="AuthenticationHandler"/></param>
        public AuthenticationHandler(IAuthenticationProvider authenticationProvider, AuthenticationHandlerOption authOption = null)
        {
            AuthenticationProvider = authenticationProvider;
            AuthOption = authOption ?? new AuthenticationHandlerOption();
        }

        /// <summary>
        /// Construct a new <see cref="AuthenticationHandler"/>
        /// </summary>
        /// <param name="authenticationProvider">An authentication provider to pass to <see cref="AuthenticationHandler"/> for authenticating requests.</param>
        /// <param name="innerHandler">A HTTP message handler to pass to the <see cref="AuthenticationHandler"/> for sending requests.</param>
        /// <param name="authOption">An OPTIONAL <see cref="Microsoft.Graph.AuthenticationHandlerOption"/> to configure <see cref="AuthenticationHandler"/></param>
        public AuthenticationHandler(IAuthenticationProvider authenticationProvider, HttpMessageHandler innerHandler, AuthenticationHandlerOption authOption = null)
            : this(authenticationProvider, authOption)
        {
            InnerHandler = innerHandler;
            AuthenticationProvider = authenticationProvider;
        }

        /// <summary>
        /// Checks HTTP response message status code if it's unauthorized (401) or not
        /// </summary>
        /// <param name="httpResponseMessage">The <see cref="HttpResponseMessage"/>to send.</param>
        /// <returns></returns>
        private bool IsUnauthorized(HttpResponseMessage httpResponseMessage)
        {
            return httpResponseMessage.StatusCode == HttpStatusCode.Unauthorized;
        }

        /// <summary>
        /// Retry sending HTTP request
        /// </summary>
        /// <param name="httpResponseMessage">The <see cref="HttpResponseMessage"/>to send.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>to send.</param>
        /// <param name="authProvider">An authentication provider to pass to <see cref="AuthenticationHandler"/> for authenticating requests.</param>
        /// <returns></returns>
        private async Task<HttpResponseMessage> SendRetryAsync(HttpResponseMessage httpResponseMessage, IAuthenticationProvider authProvider, CancellationToken cancellationToken)
        {
            int retryAttempt = 0;
            while (retryAttempt < MaxRetry)
            {
                // general clone request with internal CloneAsync (see CloneAsync for details) extension method 
                var newRequest = await httpResponseMessage.RequestMessage.CloneAsync();

                // Authenticate request using AuthenticationProvider

                await authProvider.AuthenticateRequestAsync(newRequest);
                httpResponseMessage = await base.SendAsync(newRequest, cancellationToken);

                retryAttempt++;

                if (!IsUnauthorized(httpResponseMessage) || !newRequest.IsBuffered())
                {
                    // Re-issue the request to get a new access token
                    return httpResponseMessage;
                }
            }

            return httpResponseMessage;
        }

        /// <summary>
        /// Sends a HTTP request and retries the request when the response is unauthorized.
        /// This can happen when a token from the cache expires between graph getting the request and the backend receiving the request
        /// </summary>
        /// <param name="httpRequestMessage">The <see cref="HttpRequestMessage"/> to send.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request.</param>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken)
        {
            AuthOption = httpRequestMessage.GetMiddlewareOption<AuthenticationHandlerOption>() ?? AuthOption;

            // If default auth provider is not set, use the option
            var authProvider = AuthOption.AuthenticationProvider ?? AuthenticationProvider;

            // Authenticate request using AuthenticationProvider
            if (authProvider != null)
            {
                await authProvider.AuthenticateRequestAsync(httpRequestMessage);

                HttpResponseMessage response = await base.SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false);

                // Check if response is a 401 & is not a streamed body (is buffered)
                if (IsUnauthorized(response) && httpRequestMessage.IsBuffered())
                {
                    // re-issue the request to get a new access token
                    response = await SendRetryAsync(response, authProvider, cancellationToken);
                }

                return response;
            }
            else
            {
                // NOTE: In order to support HttpProvider, we'll skip authentication if no provider is set.
                // We will add this check once we re-write a new HttpProvider.
                return await base.SendAsync(httpRequestMessage, cancellationToken);
            }
        }
    }
}
