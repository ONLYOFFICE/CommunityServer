// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    /// <summary>
    /// Contains extension methods for <see cref="HttpRequestMessage"/>
    /// </summary>
    public static class HttpRequestMessageExtensions
    {
        /// <summary>
        /// Checks the HTTP request's content to determine if it's buffered or streamed content.
        /// </summary>
        /// <param name="httpRequestMessage">The <see cref="HttpRequestMessage"/>needs to be sent.</param>
        /// <returns></returns>
        internal static bool IsBuffered(this HttpRequestMessage httpRequestMessage)
        {
            HttpContent requestContent = httpRequestMessage.Content;

            if ((httpRequestMessage.Method == HttpMethod.Put || httpRequestMessage.Method == HttpMethod.Post || httpRequestMessage.Method.Method.Equals("PATCH"))
                && requestContent != null && (requestContent.Headers.ContentLength == null || (int)requestContent.Headers.ContentLength == -1))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Get's feature request header value from the incoming <see cref="HttpRequestMessage"/>
        /// </summary>
        /// <param name="httpRequestMessage">The <see cref="HttpRequestMessage"/> object</param>
        /// <returns></returns>
        internal static FeatureFlag GetFeatureFlags(this HttpRequestMessage httpRequestMessage)
        {
            httpRequestMessage.Headers.TryGetValues(CoreConstants.Headers.FeatureFlag, out IEnumerable<string> flags);

            if (!Enum.TryParse(flags?.FirstOrDefault(), out FeatureFlag featureFlag))
            {
                featureFlag = FeatureFlag.None;
            }

            return featureFlag;
        }

        /// <summary>
        /// Create a new HTTP request by copying previous HTTP request's headers and properties from response's request message.
        /// </summary>
        /// <param name="originalRequest">The previous <see cref="HttpRequestMessage"/> needs to be copy.</param>
        /// <returns>The <see cref="HttpRequestMessage"/>.</returns>
        /// <remarks>
        /// Re-issue a new HTTP request with the previous request's headers and properities
        /// </remarks>
        internal static async Task<HttpRequestMessage> CloneAsync(this HttpRequestMessage originalRequest)
        {
            var newRequest = new HttpRequestMessage(originalRequest.Method, originalRequest.RequestUri);

            // Copy request headers.
            foreach (var header in originalRequest.Headers)
                newRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);

            // Copy request properties.
            foreach (var property in originalRequest.Properties)
                newRequest.Properties.Add(property);

            // Set Content if previous request had one.
            if (originalRequest.Content != null)
            {
                // HttpClient doesn't rewind streams and we have to explicitly do so.
                await originalRequest.Content.ReadAsStreamAsync().ContinueWith(t => {
                    if (t.Result.CanSeek)
                        t.Result.Seek(0, SeekOrigin.Begin);

                    newRequest.Content = new StreamContent(t.Result);
                });

                // Copy content headers.
                if (originalRequest.Content.Headers != null)
                    foreach (var contentHeader in originalRequest.Content.Headers)
                        newRequest.Content.Headers.TryAddWithoutValidation(contentHeader.Key, contentHeader.Value);
            }

            return newRequest;
        }

        /// <summary>
        /// Gets a <see cref="GraphRequestContext"/> from <see cref="HttpRequestMessage"/>
        /// </summary>
        /// <param name="httpRequestMessage">The <see cref="HttpRequestMessage"/> representation of the request.</param>
        /// <returns></returns>
        public static GraphRequestContext GetRequestContext(this HttpRequestMessage httpRequestMessage)
        {
            GraphRequestContext requestContext = new GraphRequestContext();
            if (httpRequestMessage.Properties.TryGetValue(typeof(GraphRequestContext).ToString(), out var requestContextObject))
            {
                requestContext = (GraphRequestContext)requestContextObject;
            }
            return requestContext;
        }

        /// <summary>
        /// Gets a <see cref="IMiddlewareOption"/> from <see cref="HttpRequestMessage"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="httpRequestMessage">The <see cref="HttpRequestMessage"/> representation of the request.</param>
        /// <returns>A middleware option</returns>
        public static T GetMiddlewareOption<T>(this HttpRequestMessage httpRequestMessage) where T : IMiddlewareOption
        {
            IMiddlewareOption option = null;
            GraphRequestContext requestContext = httpRequestMessage.GetRequestContext();
            if (requestContext.MiddlewareOptions != null)
            {
                requestContext.MiddlewareOptions.TryGetValue(typeof(T).ToString(), out option);
            }
            return (T)option;
        }

    }
}
