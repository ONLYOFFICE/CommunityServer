// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Monitor for async operations to the Graph service on the client.
    /// </summary>
    /// <typeparam name="T">The object type to return.</typeparam>
    public interface IAsyncMonitor<T>
    {
        /// <summary>
        /// Poll to check for completion of an async call to the Graph service.
        /// </summary>
        /// <param name="progress">The progress status.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The operation task.</returns>
        Task<T> PollForOperationCompletionAsync(IProgress<AsyncOperationStatus> progress, CancellationToken cancellationToken);
    }
}
