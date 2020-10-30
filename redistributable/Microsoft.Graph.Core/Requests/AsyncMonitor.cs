// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Monitor for async operations to the Graph service on the client.
    /// </summary>
    /// <typeparam name="T">The object type to return.</typeparam>
    public class AsyncMonitor<T> : IAsyncMonitor<T>
    {
        private AsyncOperationStatus asyncOperationStatus;
        private IBaseClient client;

        internal string monitorUrl;

        /// <summary>
        /// Construct an Async Monitor.
        /// </summary>
        /// <param name="client">The client to monitor.</param>
        /// <param name="monitorUrl">The URL to monitor.</param>
        public AsyncMonitor(IBaseClient client, string monitorUrl)
        {
            this.client = client;
            this.monitorUrl = monitorUrl;
        }
        
        /// <summary>
        /// Poll to check for completion of an async call to the Graph service.
        /// </summary>
        /// <param name="progress">The progress status.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The operation task.</returns>
        public async Task<T> PollForOperationCompletionAsync(IProgress<AsyncOperationStatus> progress, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, this.monitorUrl))
                {
                    using (var responseMessage = await this.client.HttpProvider.SendAsync(httpRequestMessage).ConfigureAwait(false))
                    {
                        // The monitor service will return an Accepted status for any monitor operation that hasn't completed.
                        // If we have a success code that isn't Accepted, the operation is complete. Return the resulting object.
                        if (responseMessage.StatusCode != HttpStatusCode.Accepted && responseMessage.IsSuccessStatusCode)
                        {
                            using (var responseStream = await responseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false))
                            {
                                return this.client.HttpProvider.Serializer.DeserializeObject<T>(responseStream);
                            }
                        }

                        using (var responseStream = await responseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false))
                        {
                            this.asyncOperationStatus = this.client.HttpProvider.Serializer.DeserializeObject<AsyncOperationStatus>(responseStream);

                            if (this.asyncOperationStatus == null)
                            {
                                throw new ServiceException(
                                    new Error
                                    {
                                        Code = ErrorConstants.Codes.GeneralException,
                                        Message = "Error retrieving monitor status."
                                    });
                            }

                            if (string.Equals(this.asyncOperationStatus.Status, "cancelled", StringComparison.OrdinalIgnoreCase))
                            {
                                return default(T);
                            }

                            if (string.Equals(this.asyncOperationStatus.Status, "failed", StringComparison.OrdinalIgnoreCase)
                                || string.Equals(this.asyncOperationStatus.Status, "deleteFailed", StringComparison.OrdinalIgnoreCase))
                            {
                                object message = null;
                                if (this.asyncOperationStatus.AdditionalData != null)
                                {
                                    this.asyncOperationStatus.AdditionalData.TryGetValue("message", out message);
                                }

                                throw new ServiceException(
                                    new Error
                                    {
                                        Code = ErrorConstants.Codes.GeneralException,
                                        Message = message as string
                                    });
                            }
                            
                            if (progress != null)
                            {
                                progress.Report(this.asyncOperationStatus);
                            }
                        }
                    }
                }

                await Task.Delay(CoreConstants.PollingIntervalInMs, cancellationToken).ConfigureAwait(false);
            }
            
            return default(T);
        }
    }
}
