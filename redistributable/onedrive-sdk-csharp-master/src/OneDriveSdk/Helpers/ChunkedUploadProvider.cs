// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using Microsoft.Graph;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.OneDrive.Sdk.Helpers
{
    /// <summary>
    /// Use this class to make resumable uploads or to upload large files. This
    /// class allows the client to control the size of chunks uploaded (for example, can be useful
    /// to use small chunks if the connection is slow). Also allows the client to
    /// pause an upload and resume later.
    /// </summary>
    public class ChunkedUploadProvider
    {
        private const int DefaultMaxChunkSize = 5 * 1024 * 1024;
        private const int RequiredChunkSizeIncrement = 320 * 1024;

        public UploadSession Session { get; private set; }
        private IBaseClient client;
        private Stream uploadStream;
        private readonly int maxChunkSize;
        private List<Tuple<long, long>> rangesRemaining;
        private long totalUploadLength => uploadStream.Length;
        
        /// <summary>
        /// Helps with resumable uploads. Generates chunk requests based on <paramref name="session"/>
        /// information, and can control uploading of requests using <paramref name="client"/>
        /// </summary>
        /// <param name="session">Session information.</param>
        /// <param name="client">Client used to upload chunks.</param>
        /// <param name="uploadStream">Readable, seekable stream to be uploaded. Length of session is determined via uploadStream.Length</param>
        /// <param name="maxChunkSize">Max size of each chunk to be uploaded. Multiple of 320 KiB (320 * 1024) is required.
        /// If less than 0, default value of 5 MiB is used. .</param>
        public ChunkedUploadProvider(UploadSession session, IBaseClient client, Stream uploadStream, int maxChunkSize = -1)
        {
            if (!uploadStream.CanRead || !uploadStream.CanSeek)
            {
                throw new ArgumentException("Must provide stream that can read and seek");
            }

            this.Session = session;
            this.client = client;
            this.uploadStream = uploadStream;
            this.rangesRemaining = this.GetRangesRemaining(session);
            this.maxChunkSize = maxChunkSize < 0 ? DefaultMaxChunkSize : maxChunkSize;
            if (this.maxChunkSize % RequiredChunkSizeIncrement != 0)
            {
                throw new ArgumentException("Max chunk size must be a multiple of 320 KiB", nameof(maxChunkSize));
            }
        }

        /// <summary>
        /// Get the series of requests needed to complete the upload session. Call <see cref="UpdateSessionStatusAsync"/>
        /// first to update the internal session information.
        /// </summary>
        /// <param name="options">Options to be applied to each request.</param>
        /// <returns>All requests currently needed to complete the upload session.</returns>
        public virtual IEnumerable<UploadChunkRequest> GetUploadChunkRequests(IEnumerable<Option> options = null)
        {
            foreach (var range in this.rangesRemaining)
            {
                var currentRangeBegins = range.Item1;

                while (currentRangeBegins <= range.Item2)
                {
                    var nextChunkSize = NextChunkSize(currentRangeBegins, range.Item2);
                    var uploadRequest = new UploadChunkRequest(
                        this.Session.UploadUrl,
                        this.client,
                        options,
                        currentRangeBegins,
                        currentRangeBegins + nextChunkSize - 1,
                        this.totalUploadLength);
                    
                    yield return uploadRequest;

                    currentRangeBegins += nextChunkSize;
                }
            }
        }

        /// <summary>
        /// Get the status of the session. Stores returned session internally.
        /// Updates internal list of ranges remaining to be uploaded (according to the server).
        /// </summary>
        /// <returns>UploadSession returned by the server.</returns>
        public virtual async Task<UploadSession> UpdateSessionStatusAsync()
        {
            var request = new UploadSessionRequest(this.Session, this.client, null);
            var newSession = await request.GetAsync();
            
            var newRangesRemaining = this.GetRangesRemaining(newSession);

            this.rangesRemaining = newRangesRemaining;
            newSession.UploadUrl = this.Session.UploadUrl; // Sometimes the UploadUrl is not returned
            this.Session = newSession;
            return newSession;
        }

        /// <summary>
        /// Delete the session.
        /// </summary>
        /// <returns>Once returned task is complete, the session has been deleted.</returns>
        public async Task DeleteSession()
        {
            var request = new UploadSessionRequest(this.Session, this.client, null);
            await request.DeleteAsync();
        }

        /// <summary>
        /// Upload the whole session.
        /// </summary>
        /// <param name="maxTries">Number of times to retry entire session before giving up.</param>
        /// <returns>Item information returned by server.</returns>
        public async Task<Item> UploadAsync(int maxTries = 3, IEnumerable<Option> options = null)
        {
            var uploadTries = 0;
            var readBuffer = new byte[this.maxChunkSize];
            var trackedExceptions = new List<Exception>();
            
            while (uploadTries < maxTries)
            {
                var chunkRequests = this.GetUploadChunkRequests(options);

                foreach (var request in chunkRequests)
                {
                    var result = await this.GetChunkRequestResponseAsync(request, readBuffer, trackedExceptions);

                    if (result.UploadSucceeded)
                    {
                        return result.ItemResponse;
                    }
                }

                await this.UpdateSessionStatusAsync();
                uploadTries += 1;
                if (uploadTries < maxTries)
                {
                    // Exponential backoff in case of failures.
                    await Task.Delay(2000 * uploadTries * uploadTries).ConfigureAwait(false);
                }
            }

            throw new TaskCanceledException("Upload failed too many times. See InnerException for list of exceptions that occured.", new AggregateException(trackedExceptions.ToArray()));
        }

        public virtual async Task<UploadChunkResult> GetChunkRequestResponseAsync(UploadChunkRequest request, byte[] readBuffer, ICollection<Exception> exceptionTrackingList)
        {
            var firstAttempt = true;
            this.uploadStream.Seek(request.RangeBegin, SeekOrigin.Begin);
            await this.uploadStream.ReadAsync(readBuffer, 0, request.RangeLength).ConfigureAwait(false);

            while (true)
            {
                using (var requestBodyStream = new MemoryStream(request.RangeLength))
                {
                    await requestBodyStream.WriteAsync(readBuffer, 0, request.RangeLength).ConfigureAwait(false);
                    requestBodyStream.Seek(0, SeekOrigin.Begin);

                    try
                    {
                        return await request.PutAsync(requestBodyStream).ConfigureAwait(false);
                    }
                    catch (ServiceException exception)
                    {
                        if (exception.IsMatch("generalException") || exception.IsMatch("timeout"))
                        {
                            if (firstAttempt)
                            {
                                firstAttempt = false;
                                exceptionTrackingList.Add(exception);
                            }
                            else
                            {
                                throw;
                            }
                        }
                        else if (exception.IsMatch("invalidRange"))
                        {
                            // Succeeded previously, but nothing to return right now
                            return new UploadChunkResult();
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }
        }

        internal List<Tuple<long, long>> GetRangesRemaining(UploadSession session)
        {
            // nextExpectedRanges: https://dev.onedrive.com/items/upload_large_files.htm
            // Sample: ["12345-55232","77829-99375"]
            // Also, second number in range can be blank, which means 'until the end'
            var newRangesRemaining = new List<Tuple<long, long>>();
            foreach (var range in session.NextExpectedRanges)
            {
                var rangeSpecifiers = range.Split('-');
                newRangesRemaining.Add(new Tuple<long, long>(long.Parse(rangeSpecifiers[0]),
                    string.IsNullOrEmpty(rangeSpecifiers[1]) ? this.totalUploadLength - 1 : long.Parse(rangeSpecifiers[1])));
            }

            return newRangesRemaining;
        }

        private int NextChunkSize(long rangeBegin, long rangeEnd)
        {
            var sizeBasedOnRange = (int) (rangeEnd - rangeBegin) + 1;
            return sizeBasedOnRange > this.maxChunkSize
                ? this.maxChunkSize
                : sizeBasedOnRange;
        }
    }
}
