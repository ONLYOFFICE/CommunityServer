namespace net.openstack.Core
{
    using System;
    using System.Linq;
    using System.Net;
    using JSIStudios.SimpleRESTServices.Client;
    using JSIStudios.SimpleRESTServices.Client.Json;

    /// <summary>
    /// Contains extension methods to the <see cref="Response"/> class.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    internal static class ResponseExtensions
    {
        /// <summary>
        /// The content type used for standard JSON requests and responses.
        /// </summary>
        private static readonly string JsonContentType = new JsonRequestSettings().ContentType;

        /// <summary>
        /// Retrieves a standard HTTP response header from a REST response, if available.
        /// </summary>
        /// <param name="response">The REST response.</param>
        /// <param name="header">The header to retrieve.</param>
        /// <param name="value">Returns the value for <paramref name="header"/>.</param>
        /// <returns><see langword="true"/> if the specified header is contained in <paramref name="response"/>, otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="response"/> is <see langword="null"/>.</exception>
        public static bool TryGetHeader(this Response response, HttpResponseHeader header, out string value)
        {
            if (response == null)
                throw new ArgumentNullException("response");

            if (response.Headers == null)
            {
                value = null;
                return false;
            }

            WebHeaderCollection collection = new RestWebHeaderCollection(response.Headers);
            value = collection[header];
            return value != null;
        }

        /// <summary>
        /// Retrieves a custom HTTP response header from a REST response, if available.
        /// </summary>
        /// <param name="response">The REST response.</param>
        /// <param name="header">The header to retrieve.</param>
        /// <param name="value">Returns the value for <paramref name="header"/>.</param>
        /// <returns><see langword="true"/> if the specified header is contained in <paramref name="response"/>, otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="response"/> is <see langword="null"/>.</exception>
        public static bool TryGetHeader(this Response response, string header, out string value)
        {
            HttpHeader httpHeader = response.Headers.FirstOrDefault(i => string.Equals(i.Key, header, StringComparison.OrdinalIgnoreCase));
            value = httpHeader != null ? httpHeader.Value : null;
            return value != null;
        }

        /// <summary>
        /// This method checks if a REST <see cref="Response"/> contains a JSON-formatted body.
        /// The response is assumed to be JSON if the content type is reported as <c>application/json</c>
        /// and the body is not empty.
        /// </summary>
        /// <param name="response">The REST response.</param>
        /// <returns><see langword="true"/> if <paramref name="response"/> contains a JSON response body, otherwise <see langword="false"/>.</returns>
        public static bool HasJsonBody(this Response response)
        {
            if (response == null)
                throw new ArgumentNullException("response");

            string contentTypeHeader;
            if (response.TryGetHeader(HttpResponseHeader.ContentType, out contentTypeHeader))
            {
                // ignore optional parameters when checking the content type
                string contentType = contentTypeHeader.Split(';')[0].Trim();
                if (string.Equals(contentType, JsonContentType, StringComparison.OrdinalIgnoreCase))
                {
                    return !string.IsNullOrEmpty(response.RawBody);
                }
            }

            return false;
        }
    }
}
