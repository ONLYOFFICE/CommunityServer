// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface for an HTTP provider to send requests.
    /// </summary>
    public interface IHttpProvider : IDisposable
    {
        /// <summary>
        /// Gets a serializer for serializing and deserializing JSON objects.
        /// </summary>
        ISerializer Serializer { get; }

        /// <summary>
        /// Gets or sets the timeout interval. The default value is 100 seconds.
        /// </summary>
        TimeSpan OverallTimeout { get; set; }

        /// <summary>
        /// Sends the request.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/> to send.</param>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);

        /// <summary>
        /// Sends the request.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/> to send.</param>
        /// <param name="completionOption">The <see cref="HttpCompletionOption"/> to pass to the <see cref="IHttpProvider"/> on send.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request.</param>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            HttpCompletionOption completionOption,
            CancellationToken cancellationToken);
    }
}
