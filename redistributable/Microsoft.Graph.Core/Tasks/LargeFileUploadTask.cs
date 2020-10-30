// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;

    /// <summary>
    /// Task to help with resume able large file uploads.
    /// </summary>
    public class LargeFileUploadTask<T>
    {
        private const int DefaultMaxSliceSize = 4 * 1024 * 1024;
        private const int RequiredSliceSizeIncrement = 320 * 1024;
        private IUploadSession Session { get; set; }
        private readonly IBaseClient _client;
        private readonly Stream _uploadStream;
        private readonly int _maxSliceSize;
        private List<Tuple<long, long>> _rangesRemaining;
        private long TotalUploadLength => _uploadStream.Length;

        /// <summary>
        /// Task to help with resume able large file uploads. Generates slices based on <paramref name="uploadSession"/>
        /// information, and can control uploading of requests/>
        /// </summary>
        /// <param name="uploadSession">Session information of type <see cref="IUploadSession"/>></param>
        /// <param name="uploadStream">Readable, seekable stream to be uploaded. Length of session is determined via uploadStream.Length</param>
        /// <param name="maxSliceSize">Max size of each slice to be uploaded. Multiple of 320 KiB (320 * 1024) is required.</param>
        /// <param name="baseClient"><see cref="IBaseClient"/> to use for making upload requests. The client should not set Auth headers as upload urls do not need them.
        /// If less than 0, default value of 5 MiB is used. .</param>
        public LargeFileUploadTask(IUploadSession uploadSession, Stream uploadStream,  int maxSliceSize = -1, IBaseClient baseClient = null)
        {
            if (!uploadStream.CanRead || !uploadStream.CanSeek)
            {
                throw new ArgumentException("Must provide stream that can read and seek");
            }

            this.Session = uploadSession;
            this._client = baseClient ?? this.InitializeClient(uploadSession.UploadUrl);
            this._uploadStream = uploadStream;
            this._rangesRemaining = this.GetRangesRemaining(uploadSession);
            this._maxSliceSize = maxSliceSize < 0 ? DefaultMaxSliceSize : maxSliceSize;
            if (this._maxSliceSize % RequiredSliceSizeIncrement != 0)
            {
                throw new ArgumentException("Max slice size must be a multiple of 320 KiB", nameof(maxSliceSize));
            }
        }

        /// <summary>
        /// Initialize a baseClient to use for the upload that does not have Auth enabled as the upload URLs explicitly do not need authentication.
        /// </summary>
        /// <param name="uploadUrl">Url to perform the upload to from the session</param>
        /// <returns></returns>
        private IBaseClient InitializeClient(string uploadUrl)
        {
            HttpClient httpClient = GraphClientFactory.Create(authenticationProvider: null); //no auth
            httpClient.SetFeatureFlag(FeatureFlag.FileUploadTask);
            return new BaseClient(uploadUrl, httpClient);
        }

        /// <summary>
        /// Write a slice of data using the UploadSliceRequest.
        /// </summary>
        /// <param name="uploadSliceRequest">The UploadSliceRequest to make the request with.</param>
        /// <param name="exceptionTrackingList">A list of exceptions to use to track progress. SlicedUpload may retry.</param>
        private async Task<UploadResult<T>> UploadSliceAsync(UploadSliceRequest<T> uploadSliceRequest, ICollection<Exception> exceptionTrackingList)
        {
            var firstAttempt = true;
            this._uploadStream.Seek(uploadSliceRequest.RangeBegin, SeekOrigin.Begin);

            while (true)
            {
                using (var requestBodyStream = new ReadOnlySubStream(this._uploadStream, uploadSliceRequest.RangeBegin, uploadSliceRequest.RangeLength))
                {
                    try
                    {
                        return await uploadSliceRequest.PutAsync(requestBodyStream).ConfigureAwait(false);
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
                            return new UploadResult<T>();
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get the series of requests needed to complete the upload session. Call <see cref="UpdateSessionStatusAsync"/>
        /// first to update the internal session information.
        /// </summary>
        /// <returns>All requests currently needed to complete the upload session.</returns>
        internal IEnumerable<UploadSliceRequest<T>> GetUploadSliceRequests()
        {
            foreach (var (item1, item2) in this._rangesRemaining)
            {
                var currentRangeBegins = item1;

                while (currentRangeBegins <= item2)
                {
                    var nextSliceSize = NextSliceSize(currentRangeBegins, item2);
                    var uploadRequest = new UploadSliceRequest<T>(
                        this.Session.UploadUrl,
                        this._client,
                        currentRangeBegins,
                        currentRangeBegins + nextSliceSize - 1,
                        this.TotalUploadLength);

                    yield return uploadRequest;

                    currentRangeBegins += nextSliceSize;
                }
            }
        }

        /// <summary>
        /// Upload the whole session.
        /// </summary>
        /// <param name="maxTries">Number of times to retry entire session before giving up.</param>
        /// <param name="progress">IProgress object to monitor the progress of the upload.</param>
        /// <returns>Item information returned by server.</returns>
        public async Task<UploadResult<T>> UploadAsync(IProgress<long> progress = null, int maxTries = 3)
        {
            var uploadTries = 0;
            var trackedExceptions = new List<Exception>();

            while (uploadTries < maxTries)
            {
                var sliceRequests = this.GetUploadSliceRequests();

                foreach (var request in sliceRequests)
                {
                    var uploadResult = await this.UploadSliceAsync(request, trackedExceptions).ConfigureAwait(false);
                    
                    progress?.Report(request.RangeBegin);//report the progress of upload

                    if (uploadResult.UploadSucceeded)
                    {
                        return uploadResult;
                    }
                }

                await this.UpdateSessionStatusAsync().ConfigureAwait(false);
                uploadTries += 1;
                if (uploadTries < maxTries)
                {
                    // Exponential back off in case of failures.
                    await Task.Delay(2000 * uploadTries * uploadTries).ConfigureAwait(false);
                }
            }
            
            throw new TaskCanceledException("Upload failed too many times. See InnerException for list of exceptions that occured.", new AggregateException(trackedExceptions.ToArray()));
        }

        /// <summary>
        /// Get info about the upload session and resume from where it left off.
        /// </summary>
        /// <param name="maxTries">Number of times to retry entire session before giving up.</param>
        /// <param name="progress">IProgress object to monitor the progress of the upload.</param>
        /// <returns>Item information returned by server.</returns>
        public async Task<UploadResult<T>> ResumeAsync(IProgress<long> progress = null, int maxTries = 3)
        {
            var uploadSession = await this.UpdateSessionStatusAsync().ConfigureAwait(false);
            var uploadExpirationTime = uploadSession.ExpirationDateTime ?? DateTimeOffset.Now;
            // validate that the upload can still be resumed.
            if (DateTimeOffset.Compare(uploadExpirationTime, DateTimeOffset.Now) <= 0)
            {
                throw new ClientException(
                    new Error
                    {
                        Code = ErrorConstants.Codes.Timeout,
                        Message = ErrorConstants.Messages.ExpiredUploadSession
                    });
            }
            return await this.UploadAsync(progress, maxTries).ConfigureAwait(false);
        }

        /// <summary>
        /// Get the status of the session. Stores returned session internally.
        /// Updates internal list of ranges remaining to be uploaded (according to the server).
        /// </summary>
        /// <returns><see cref="IUploadSession"/>> returned by the server.</returns>
        public async Task<IUploadSession> UpdateSessionStatusAsync()
        {
            var request = new UploadSessionRequest(this.Session, this._client);
            var newSession = await request.GetAsync().ConfigureAwait(false);

            var newRangesRemaining = this.GetRangesRemaining(newSession);

            this._rangesRemaining = newRangesRemaining;
            newSession.UploadUrl = this.Session.UploadUrl; // Sometimes the UploadUrl is not returned
            this.Session = newSession;
            return newSession;
        }

        /// <summary>
        /// Delete the session.
        /// </summary>
        /// <returns>Once returned task is complete, the session has been deleted.</returns>
        public async Task DeleteSessionAsync()
        {
            // validate that the upload can still be deleted.
            var uploadExpirationTime = this.Session.ExpirationDateTime ?? DateTimeOffset.Now;
            if (DateTimeOffset.Compare(uploadExpirationTime, DateTimeOffset.Now) <= 0)
            {
                throw new ClientException(
                    new Error
                    {
                        Code = ErrorConstants.Codes.Timeout,
                        Message = ErrorConstants.Messages.ExpiredUploadSession
                    });
            }
            var request = new UploadSessionRequest(this.Session, this._client);
            await request.DeleteAsync().ConfigureAwait(false);
        }

        private List<Tuple<long, long>> GetRangesRemaining(IUploadSession session)
        {
            // nextExpectedRanges: https://dev.onedrive.com/items/upload_large_files.htm
            // Sample: ["12345-55232","77829-99375"]
            // Also, second number in range can be blank, which means 'until the end'
            var newRangesRemaining = new List<Tuple<long, long>>();
            foreach (var range in session.NextExpectedRanges)
            {
                var rangeSpecifiers = range.Split('-');
                newRangesRemaining.Add(new Tuple<long, long>(long.Parse(rangeSpecifiers[0]),
                    string.IsNullOrEmpty(rangeSpecifiers[1]) ? this.TotalUploadLength - 1 : long.Parse(rangeSpecifiers[1])));
            }

            return newRangesRemaining;
        }

        private long NextSliceSize(long rangeBegin, long rangeEnd)
        {
            var sizeBasedOnRange = rangeEnd - rangeBegin + 1;
            return sizeBasedOnRange > this._maxSliceSize
                ? this._maxSliceSize
                : sizeBasedOnRange;
        }
    }
}