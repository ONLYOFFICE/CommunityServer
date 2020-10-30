// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.Core.Requests
{
    using Newtonsoft.Json.Linq;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// The type BatchRequest
    /// </summary>
    public class BatchRequest: BaseRequest, IBatchRequest
    {
        /// <summary>
        /// Constructs a new BatchRequest.
        /// </summary>
        /// <param name="requestUrl">The URL for the built request.</param>
        /// <param name="client">The <see cref="IBaseClient"/> for handling requests.</param>
        /// <param name="options">Query and header option name value pairs for the request.</param>
        public BatchRequest(
            string requestUrl, 
            IBaseClient client, 
            IEnumerable<Option> options = null) 
            : base(requestUrl, client, options)
        {
        }

        /// <summary>
        /// Sends out the <see cref="BatchRequestContent"/> using the POST method
        /// </summary>
        /// <param name="batchRequestContent">The <see cref="BatchRequestContent"/> for the request</param>
        /// <returns></returns>
        public Task<BatchResponseContent> PostAsync(BatchRequestContent batchRequestContent)
        {
            return this.PostAsync(batchRequestContent, CancellationToken.None);
        }

        /// <summary>
        /// Sends out the <see cref="BatchRequestContent"/> using the POST method
        /// </summary>
        /// <param name="batchRequestContent">The <see cref="BatchRequestContent"/> for the request</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request.</param>
        /// <returns></returns>
        public async Task<BatchResponseContent> PostAsync(BatchRequestContent batchRequestContent, CancellationToken cancellationToken)
        {
            this.ContentType = "application/json";
            this.Method = "POST";
            JObject serializableContent = await batchRequestContent.GetBatchRequestContentAsync().ConfigureAwait(false);
            HttpResponseMessage httpResponseMessage = await this.SendRequestAsync(serializableContent, cancellationToken).ConfigureAwait(false);
            return new BatchResponseContent(httpResponseMessage);
        }
    }
}