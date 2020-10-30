// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.Core.Requests
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// The interface IBatchRequest.
    /// </summary>
    public interface IBatchRequest : IBaseRequest
    {
        /// <summary>
        /// Sends out the <see cref="BatchRequestContent"/> using the POST method
        /// </summary>
        /// <param name="batchRequestContent">The <see cref="BatchRequestContent"/> for the request</param>
        /// <returns></returns>
        Task<BatchResponseContent> PostAsync(BatchRequestContent batchRequestContent);

        /// <summary>
        /// Sends out the <see cref="BatchRequestContent"/> using the POST method
        /// </summary>
        /// <param name="batchRequestContent">The <see cref="BatchRequestContent"/> for the request</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request.</param>
        /// <returns></returns>
        Task<BatchResponseContent> PostAsync(BatchRequestContent batchRequestContent, CancellationToken cancellationToken);
    }
}
