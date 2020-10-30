// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Graph;

namespace Microsoft.OneDrive.Sdk
{
    /// <summary>
    /// The type UploadChunkRequest.
    /// </summary>
    public partial class UploadChunkRequest : BaseRequest, IUploadChunkRequest
    {
        public long RangeBegin { get; private set; }
        public long RangeEnd { get; private set; }
        public long TotalSessionLength { get; private set; }
        public int RangeLength => (int)(this.RangeEnd - this.RangeBegin + 1);

        /// <summary>
        /// Request for uploading one chunk of a session
        /// </summary>
        /// <param name="sessionUrl">URL to upload the chunk.</param>
        /// <param name="client">Client used for sending the chunk.</param>
        /// <param name="options">Options</param>
        /// <param name="rangeBegin">Beginning of range of this chunk</param>
        /// <param name="rangeEnd">End of range of this chunk</param>
        /// <param name="totalSessionLength">Total session length. This MUST be consistent
        /// across all chunks.</param>
        public UploadChunkRequest(
            string sessionUrl,
            IBaseClient client,
            IEnumerable<Option> options,
            long rangeBegin,
            long rangeEnd,
            long totalSessionLength)
            : base(sessionUrl, client, options)
        {
            this.RangeBegin = rangeBegin;
            this.RangeEnd = rangeEnd;
            this.TotalSessionLength = totalSessionLength;
        }

        /// <summary>
        /// Uploads the chunk using PUT.
        /// </summary>
        /// <returns>The status of the upload.</returns>
        public Task<UploadChunkResult> PutAsync(Stream stream)
        {
            return this.PutAsync(stream, CancellationToken.None);
        }

        /// <summary>
        /// Uploads the chunk using PUT.
        /// </summary>
        /// <param name="stream">Stream of data to be sent in the request. Length must be equal to the length
        /// of this chunk (as defined by this.RangeLength)</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The status of the upload. If UploadSession.AdditionalData.ContainsKey("successResponse")
        /// is true, then the item has completed, and the value is the created item from the server.</returns>
        public virtual async Task<UploadChunkResult> PutAsync(Stream stream, CancellationToken cancellationToken)
        {
            this.Method = "PUT";
            using (var response = await this.SendRequestAsync(stream, cancellationToken).ConfigureAwait(false))
            {
                if (response.Content != null)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    
                    if (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK)
                    {
                        return new UploadChunkResult
                            {
                                ItemResponse =
                                    this.Client.HttpProvider.Serializer.DeserializeObject<Item>(responseString)
                            };
                    }
                    else
                    {
                        try
                        {
                            return new UploadChunkResult
                                {
                                    UploadSession =
                                        this.Client.HttpProvider.Serializer.DeserializeObject<UploadSession>(responseString)
                                };
                        }
                        catch (SerializationException exception)
                        {
                            throw new ServiceException(new Error()
                                {
                                    Code = OneDriveErrorCode.GeneralException.ToString(),
                                    Message = "Error deserializing UploadSession response: " + exception.Message,
                                    AdditionalData = new Dictionary<string, object>
                                        {
                                            { "rawResponse", responseString },
                                            { "rawHeaders", string.Join(", ", response.Headers.Select(h => $"{h.Key}: {h.Value}"))}
                                        }
                                });
                        }
                    }
                }

                throw new ServiceException(new Error
                    {
                        Code = OneDriveErrorCode.GeneralException.ToString(),
                        Message = "UploadChunkRequest received no response."
                    });
            }
        }

        private async Task<HttpResponseMessage> SendRequestAsync(
            Stream stream,
            CancellationToken cancellationToken,
            HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
        {
            if (string.IsNullOrEmpty(this.RequestUrl))
            {
                throw new ArgumentNullException(nameof(this.RequestUrl), "Session Upload URL cannot be null or empty.");
            }

            if (this.Client.AuthenticationProvider == null)
            {
                throw new ArgumentNullException(nameof(this.Client.AuthenticationProvider), "Client.AuthenticationProvider must not be null.");
            }

            using (var request = this.GetHttpRequestMessage())
            {
                request.Content = new StreamContent(stream);
                request.Content.Headers.ContentRange =
                    new ContentRangeHeaderValue(this.RangeBegin, this.RangeEnd, this.TotalSessionLength);
                request.Content.Headers.ContentLength = this.RangeLength;

                return await this.Client.HttpProvider.SendAsync(request, completionOption, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    public class UploadChunkResult
    {
        public UploadSession UploadSession;
        public Item ItemResponse;
        public bool UploadSucceeded => this.ItemResponse != null;
    }
}
