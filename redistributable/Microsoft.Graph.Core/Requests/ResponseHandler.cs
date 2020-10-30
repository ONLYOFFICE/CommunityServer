// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides method(s) to deserialize raw HTTP responses into strong types.
    /// </summary>
    public class ResponseHandler : IResponseHandler
    {
        private readonly ISerializer serializer;
        /// <summary>
        /// Constructs a new <see cref="ResponseHandler"/>.
        /// </summary>
        /// <param name="serializer"></param>
        public ResponseHandler(ISerializer serializer)
        {
            this.serializer = serializer;
        }

        /// <summary>
        /// Process raw HTTP response into requested domain type.
        /// </summary>
        /// <typeparam name="T">The type to return</typeparam>
        /// <param name="response">The HttpResponseMessage to handle</param>
        /// <returns></returns>
        public async Task<T> HandleResponse<T>(HttpResponseMessage response)
        {
            if (response.Content != null)
            {
                var responseString = await GetResponseString(response);
                return serializer.DeserializeObject<T>(responseString);
            }

            return default(T);
        }

        /// <summary>
        /// Get the response content string
        /// </summary>
        /// <param name="hrm">The response object</param>
        /// <returns>The full response string to return</returns>
        private async Task<string> GetResponseString(HttpResponseMessage hrm)
        {
            var responseContent = "";

            var content = await hrm.Content.ReadAsStringAsync().ConfigureAwait(false);

            //Only add headers if we are going to return a response body
            if (content.Length > 0)
            {
                var responseHeaders = hrm.Headers;
                var statusCode = hrm.StatusCode;

                Dictionary<string, string[]> headerDictionary = responseHeaders.ToDictionary(x => x.Key, x => x.Value.ToArray());
                var responseHeaderString = serializer.SerializeObject(headerDictionary);

                responseContent = content.Substring(0, content.Length - 1) + ", ";
                responseContent += "\"responseHeaders\": " + responseHeaderString + ", ";
                responseContent += "\"statusCode\": \"" + statusCode + "\"}";
            }

            return responseContent;
        }

    }
}
