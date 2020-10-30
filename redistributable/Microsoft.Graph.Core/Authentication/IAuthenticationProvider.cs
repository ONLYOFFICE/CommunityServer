// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System.Net.Http;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface for authenticating requests.
    /// </summary>
    public interface IAuthenticationProvider
    {
        /// <summary>
        /// Authenticates the specified request message.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/> to authenticate.</param>
        /// <returns>The task to await.</returns>
        Task AuthenticateRequestAsync(HttpRequestMessage request);
    }
}
