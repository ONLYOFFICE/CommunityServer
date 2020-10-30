// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// The UploadSliceRequest class to help with uploading file slices
    /// </summary>
    /// <typeparam name="T">The type to be uploaded</typeparam>
    internal class UploadSliceRequest<T> : BaseRequest
    {
        private UploadResponseHandler responseHandler;

        /// <summary>
        /// The beginning of the slice range to send.
        /// </summary>
        public long RangeBegin { get; private set; }

        /// <summary>
        /// The end of the slice range to send.
        /// </summary>
        public long RangeEnd { get; private set; }

        /// <summary>
        /// The length in bytes of the session.
        /// </summary>
        public long TotalSessionLength { get; private set; }

        /// <summary>
        /// The range length of the slice to send.
        /// </summary>
        public int RangeLength => (int)(this.RangeEnd - this.RangeBegin + 1);

        /// <summary>
        /// Request for uploading one slice of a session
        /// </summary>
        /// <param name="sessionUrl">URL to upload the slice.</param>
        /// <param name="client">Client used for sending the slice.</param>
        /// <param name="rangeBegin">Beginning of range of this slice</param>
        /// <param name="rangeEnd">End of range of this slice</param>
        /// <param name="totalSessionLength">Total session length. This MUST be consistent
        /// across all slice.</param>
        public UploadSliceRequest(
            string sessionUrl,
            IBaseClient client,
            long rangeBegin,
            long rangeEnd,
            long totalSessionLength)
            : base(sessionUrl, client, null)
        {
            this.RangeBegin = rangeBegin;
            this.RangeEnd = rangeEnd;
            this.TotalSessionLength = totalSessionLength;
            this.responseHandler = new UploadResponseHandler();
        }

        /// <summary>
        /// Uploads the slice using PUT.
        /// </summary>
        /// <param name="stream">Stream of data to be sent in the request.</param>
        /// <returns>The status of the upload.</returns>
        public Task<UploadResult<T>> PutAsync(Stream stream)
        {
            return this.PutAsync(stream, CancellationToken.None);
        }

        /// <summary>
        /// Uploads the slice using PUT.
        /// </summary>
        /// <param name="stream">Stream of data to be sent in the request. Length must be equal to the length
        /// of this slice (as defined by this.RangeLength)</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The status of the upload. If UploadSession.AdditionalData.ContainsKey("successResponse")
        /// is true, then the item has completed, and the value is the created item from the server.</returns>
        public virtual async Task<UploadResult<T>> PutAsync(Stream stream, CancellationToken cancellationToken)
        {
            this.Method = "PUT";
            this.ContentType = "application/octet-stream";
            using (var response = await this.SendRequestAsync(stream, cancellationToken).ConfigureAwait(false))
            {
                return await this.responseHandler.HandleResponse<T>(response);
            }
        }

        /// <summary>
        /// Send the Sliced Upload request
        /// </summary>
        /// <param name="stream">Stream of data to be sent in the request.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <param name="completionOption">The completion option for the request. Defaults to ResponseContentRead.</param>
        /// <returns></returns>
        private async Task<HttpResponseMessage> SendRequestAsync(
            Stream stream,
            CancellationToken cancellationToken,
            HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
        {
            // Append the relevant headers for the range upload request
            using (var request = this.GetHttpRequestMessage())
            {
                request.Content = new StreamContent(stream);
                request.Content.Headers.ContentRange = new ContentRangeHeaderValue(this.RangeBegin, this.RangeEnd, this.TotalSessionLength);
                request.Content.Headers.ContentLength = this.RangeLength;

                return await this.Client.HttpProvider.SendAsync(request, completionOption, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
