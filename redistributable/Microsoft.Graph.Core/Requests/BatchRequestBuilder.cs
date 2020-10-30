// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.Core.Requests
{
    using System.Collections.Generic;

    /// <summary>
    /// The type BatchRequestBuilder
    /// </summary>
    public class BatchRequestBuilder: BaseRequestBuilder, IBatchRequestBuilder
    {
        /// <summary>
        /// Constructs a new BatchRequestBuilder.
        /// </summary>
        /// <param name="requestUrl">The URL for the built request.</param>
        /// <param name="client">The <see cref="IBaseClient"/> for handling requests.</param>
        public BatchRequestBuilder(
            string requestUrl, 
            IBaseClient client) 
            : base(requestUrl, client)
        {
        }

        /// <summary>
        /// Builds the request.
        /// </summary>
        /// <returns>The built request.</returns>
        public IBatchRequest Request()
        {
            return this.Request(null);
        }

        /// <summary>
        /// Builds the request.
        /// </summary>
        /// <param name="options">The query and header options for the request.</param>
        /// <returns>The built request.</returns>
        public IBatchRequest Request(IEnumerable<Option> options)
        {
            return new BatchRequest(this.RequestUrl,this.Client,options);
        }
    }
}