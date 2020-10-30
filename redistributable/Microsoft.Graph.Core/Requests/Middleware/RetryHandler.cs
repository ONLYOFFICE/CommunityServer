// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------
namespace Microsoft.Graph
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A <see cref="DelegatingHandler"/> implementation using standard .NET libraries.
    /// </summary>
    public class RetryHandler : DelegatingHandler
    {
        private const string RETRY_AFTER = "Retry-After";
        private const string RETRY_ATTEMPT = "Retry-Attempt";
        private double m_pow = 1;

        /// <summary>
        /// RetryOption property
        /// </summary>
        internal RetryHandlerOption RetryOption { get; set; }

        /// <summary>
        /// Construct a new <see cref="RetryHandler"/>
        /// </summary>
        /// <param name="retryOption">An OPTIONAL <see cref="Microsoft.Graph.RetryHandlerOption"/> to configure <see cref="RetryHandler"/></param>
        public RetryHandler(RetryHandlerOption retryOption = null)
        {
            RetryOption = retryOption ?? new RetryHandlerOption();
        }

        /// <summary>
        /// Construct a new <see cref="RetryHandler"/>
        /// </summary>
        /// <param name="innerHandler">An HTTP message handler to pass to the <see cref="HttpMessageHandler"/> for sending requests.</param>
        /// <param name="retryOption">An OPTIONAL <see cref="Microsoft.Graph.RetryHandlerOption"/> to configure <see cref="RetryHandler"/></param>
        public RetryHandler(HttpMessageHandler innerHandler, RetryHandlerOption retryOption = null)
            : this(retryOption)
        {
            InnerHandler = innerHandler;
        }

        /// <summary>
        /// Send a HTTP request 
        /// </summary>
        /// <param name="httpRequest">The HTTP request<see cref="HttpRequestMessage"/>needs to be sent.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request.</param>
        /// <returns></returns>
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequest, CancellationToken cancellationToken)
        {
            RetryOption = httpRequest.GetMiddlewareOption<RetryHandlerOption>() ?? RetryOption;

            var response = await base.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);

            // Check whether retries are permitted and that the MaxRetry value is a non - negative, non - zero value
            if (ShouldRetry(response) && httpRequest.IsBuffered() && RetryOption.MaxRetry > 0 && RetryOption.ShouldRetry(RetryOption.Delay, 0, response))
            {
                response = await SendRetryAsync(response, cancellationToken);
            }

            return response;
        }

        /// <summary>
        /// Retry sending the HTTP request 
        /// </summary>
        /// <param name="response">The <see cref="HttpResponseMessage"/> which is returned and includes the HTTP request needs to be retried.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the retry.</param>
        /// <returns></returns>
        private async Task<HttpResponseMessage> SendRetryAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            int retryCount = 0;
            TimeSpan cumulativeDelay = TimeSpan.Zero;

            while (retryCount < RetryOption.MaxRetry)
            {
                // Drain response content to free responses.
                if (response.Content != null)
                {
                    await response.Content.ReadAsByteArrayAsync();
                }

                // Call Delay method to get delay time from response's Retry-After header or by exponential backoff 
                Task delay = Delay(response, retryCount, RetryOption.Delay, out double delayInSeconds, cancellationToken);

                // If client specified a retries time limit, let's honour it
                if (RetryOption.RetriesTimeLimit > TimeSpan.Zero)
                {
                    // Get the cumulative delay time
                    cumulativeDelay += TimeSpan.FromSeconds(delayInSeconds);

                    // Check whether delay will exceed the client-specified retries time limit value 
                    if (cumulativeDelay > RetryOption.RetriesTimeLimit)
                    {
                        return response;
                    }
                }

                // general clone request with internal CloneAsync (see CloneAsync for details) extension method 
                var request = await response.RequestMessage.CloneAsync();

                // Increase retryCount and then update Retry-Attempt in request header
                retryCount++;
                AddOrUpdateRetryAttempt(request, retryCount);

                // Delay time
                await delay;

                // Call base.SendAsync to send the request
                response = await base.SendAsync(request, cancellationToken);

                if (!ShouldRetry(response) || !request.IsBuffered() || !RetryOption.ShouldRetry(RetryOption.Delay, retryCount, response))
                {
                    return response;
                }
            }
            throw new ServiceException(
                         new Error
                         {
                             Code = ErrorConstants.Codes.TooManyRetries,
                             Message = string.Format(ErrorConstants.Messages.TooManyRetriesFormatString, retryCount)
                         });

        }

        /// <summary>
        /// Update Retry-Attempt header in the HTTP request
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/>needs to be sent.</param>
        /// <param name="retry_count">Retry times</param>
        private void AddOrUpdateRetryAttempt(HttpRequestMessage request, int retry_count)
        {
            if (request.Headers.Contains(RETRY_ATTEMPT))
            {
                request.Headers.Remove(RETRY_ATTEMPT);
            }
            request.Headers.Add(RETRY_ATTEMPT, retry_count.ToString());
        }

        /// <summary>
        /// Delay task operation for timed-retries based on Retry-After header in the response or exponential backoff
        /// </summary>
        /// <param name="response">The <see cref="HttpResponseMessage"/>returned.</param>
        /// <param name="retry_count">The retry counts</param>
        /// <param name="delay">Delay value in seconds.</param>
        /// <param name="delayInSeconds"></param>
        /// <param name="cancellationToken">The cancellationToken for the Http request</param>
        /// <returns>The <see cref="Task"/> for delay operation.</returns>
        internal Task Delay(HttpResponseMessage response, int retry_count, int delay, out double delayInSeconds, CancellationToken cancellationToken)
        {
            HttpHeaders headers = response.Headers;
            delayInSeconds = delay;
            if (headers.TryGetValues(RETRY_AFTER, out IEnumerable<string> values))
            {
                string retry_after = values.First();
                if (Int32.TryParse(retry_after, out int delay_seconds))
                {
                    delayInSeconds = delay_seconds;
                }
            }
            else
            {
                m_pow = Math.Pow(2, retry_count);
                delayInSeconds = m_pow * delay;
            }

            TimeSpan delayTimeSpan = TimeSpan.FromSeconds(Math.Min(delayInSeconds, RetryHandlerOption.MAX_DELAY));

            return Task.Delay(delayTimeSpan, cancellationToken);

        }

        /// <summary>
        /// Check the HTTP response's status to determine whether it should be retried or not.
        /// </summary>
        /// <param name="response">The <see cref="HttpResponseMessage"/>returned.</param>
        /// <returns></returns>
        private bool ShouldRetry(HttpResponseMessage response)
        {
            return (response.StatusCode == HttpStatusCode.ServiceUnavailable ||
                response.StatusCode == HttpStatusCode.GatewayTimeout ||
                response.StatusCode == (HttpStatusCode)429);
        }
    }
}
