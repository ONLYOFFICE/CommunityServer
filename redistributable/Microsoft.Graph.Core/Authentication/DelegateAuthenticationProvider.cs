// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System.Net.Http;
    using System.Threading.Tasks;

    /// <summary>
    /// Authenticate request async delegate.
    /// </summary>
    /// <param name="request">The <see cref="HttpRequestMessage"/> to authenticate.</param>
    /// <returns></returns>
    public delegate Task AuthenticateRequestAsyncDelegate(HttpRequestMessage request);

    /// <summary>
    /// A default <see cref="IAuthenticationProvider"/> implementation.
    /// </summary>
    public class DelegateAuthenticationProvider : IAuthenticationProvider
    {
        /// <summary>
        /// Constructs an <see cref="DelegateAuthenticationProvider"/>.
        /// </summary>
        public DelegateAuthenticationProvider(AuthenticateRequestAsyncDelegate authenticateRequestAsyncDelegate)
        {
            this.AuthenticateRequestAsyncDelegate = authenticateRequestAsyncDelegate;
        }

        /// <summary>
        /// Gets or sets the delegate for authenticating requests.
        /// </summary>
        public AuthenticateRequestAsyncDelegate AuthenticateRequestAsyncDelegate { get; set; }

        /// <summary>
        /// Authenticates the specified request message.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/> to authenticate.</param>
        public Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            if (this.AuthenticateRequestAsyncDelegate != null)
            {
                return this.AuthenticateRequestAsyncDelegate(request);
            }

            return Task.FromResult(0);
        }
    }
}
