// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System.Collections.Generic;
    using System.Net.Http;

    /// <summary>
    /// The base request interface.
    /// </summary>
    public interface IBaseRequest
    {
        /// <summary>
        /// Gets or sets the content type for the request.
        /// </summary>
        string ContentType { get; set; }

        /// <summary>
        /// Gets the <see cref="HeaderOption"/> collection for the request.
        /// </summary>
        IList<HeaderOption> Headers { get; }

        /// <summary>
        /// Gets the <see cref="IBaseClient"/> for handling requests.
        /// </summary>
        IBaseClient Client { get; }

        /// <summary>
        /// Gets or sets the HTTP method string for the request.
        /// </summary>
        string Method { get; }

        /// <summary>
        /// Gets the URL for the request, without query string.
        /// </summary>
        string RequestUrl { get; }

        /// <summary>
        /// Gets the <see cref="QueryOption"/> collection for the request.
        /// </summary>
        IList<QueryOption> QueryOptions { get; }

        /// <summary>
        /// Gets the <see cref="IMiddlewareOption"/> collection for the request.
        /// </summary>
        IDictionary<string, IMiddlewareOption> MiddlewareOptions { get; }

        /// <summary>
        /// Gets the <see cref="HttpRequestMessage"/> representation of the request.
        /// </summary>
        /// <returns>The <see cref="HttpRequestMessage"/> representation of the request.</returns>
        HttpRequestMessage GetHttpRequestMessage();
    }
}
