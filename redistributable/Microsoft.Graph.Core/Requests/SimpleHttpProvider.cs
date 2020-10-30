// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// An <see cref="IHttpProvider"/> implementation using standard .NET libraries.
    /// </summary>
    public class SimpleHttpProvider : IHttpProvider
    {
        internal readonly HttpClient httpClient;

        /// <summary>
        /// Constructs a new <see cref="SimpleHttpProvider"/>.
        /// </summary>
        /// <param name="httpClient">Custom http client to be used for making requests</param>
        /// <param name="serializer">A serializer for serializing and deserializing JSON objects.</param>
        public SimpleHttpProvider(HttpClient httpClient, ISerializer serializer = null)
        {
            // Null authProvider addresses https://github.com/microsoftgraph/msgraph-sdk-dotnet/issues/605.
            // We're reenabling this functionality that allowed setting a null authprovider.
            this.httpClient = httpClient ?? GraphClientFactory.Create(authenticationProvider: null);
            Serializer = serializer ?? new Serializer();
        }

        /// <summary>
        /// Gets a serializer for serializing and deserializing JSON objects.
        /// </summary>
        public ISerializer Serializer { get; private set; }

        /// <summary>
        /// Gets or sets the overall request timeout.
        /// </summary>
        public TimeSpan OverallTimeout
        {
            get => this.httpClient.Timeout;

            set
            {
                try
                {
                    this.httpClient.Timeout = value;
                }
                catch (InvalidOperationException exception)
                {
                    throw new ServiceException(
                        new Error
                        {
                            Code = ErrorConstants.Codes.NotAllowed,
                            Message = ErrorConstants.Messages.OverallTimeoutCannotBeSet,
                        },
                        exception);
                }
            }
        }

        /// <summary>
        /// Sends the request.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/> to send.</param>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            return this.SendAsync(request, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
        }

        /// <summary>
        /// Sends the request.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/> to send.</param>
        /// <param name="completionOption">The <see cref="HttpCompletionOption"/> to pass to the <see cref="IHttpProvider"/> on send.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request.</param>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        public async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, 
            HttpCompletionOption completionOption, 
            CancellationToken cancellationToken)
        {
            var response = await this.SendRequestAsync(request, completionOption, cancellationToken).ConfigureAwait(false);

            // check if the response is of a successful nature.
            if (!response.IsSuccessStatusCode)
            {
                using (response)
                {
                    if (null != response.Content)
                    {
                        await response.Content.LoadIntoBufferAsync().ConfigureAwait(false);
                    }

                    var errorResponse = await this.ConvertErrorResponseAsync(response).ConfigureAwait(false);
                    Error error;

                    if (errorResponse == null || errorResponse.Error == null)
                    {
                        if (response.StatusCode == HttpStatusCode.NotFound)
                        {
                            error = new Error { Code = ErrorConstants.Codes.ItemNotFound };
                        }
                        else
                        {
                            error = new Error
                            {
                                Code = ErrorConstants.Codes.GeneralException,
                                Message = ErrorConstants.Messages.UnexpectedExceptionResponse,
                            };
                        }
                    }
                    else
                    {
                        error = errorResponse.Error;
                    }

                    if (string.IsNullOrEmpty(error.ThrowSite))
                    {
                        if (response.Headers.TryGetValues(CoreConstants.Headers.ThrowSiteHeaderName, out var throwSiteValues))
                        {
                            error.ThrowSite = throwSiteValues.FirstOrDefault();
                        }
                    }

                    if (string.IsNullOrEmpty(error.ClientRequestId))
                    {
                        if (response.Headers.TryGetValues(CoreConstants.Headers.ClientRequestId, out var clientRequestId))
                        {
                            error.ClientRequestId = clientRequestId.FirstOrDefault();
                        }
                    }

                    if (response.Content?.Headers.ContentType.MediaType == "application/json")
                    {
                        string rawResponseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                        throw new ServiceException(error,
                            response.Headers,
                            response.StatusCode,
                            rawResponseBody);
                    }
                    else
                    {
                        // Pass through the response headers and status code to the ServiceException.
                        // System.Net.HttpStatusCode does not support RFC 6585, Additional HTTP Status Codes.
                        // Throttling status code 429 is in RFC 6586. The status code 429 will be passed through.
                        throw new ServiceException(error, response.Headers, response.StatusCode);
                    }
                }
            }

            return response;
        }

        /// <summary>
        /// Disposes the HttpClient and HttpClientHandler instances.
        /// </summary>
        public void Dispose()
        {
            httpClient?.Dispose();
        }

        /// <summary>
        /// Converts the <see cref="HttpRequestException"/> into an <see cref="ErrorResponse"/> object;
        /// </summary>
        /// <param name="response">The <see cref="HttpResponseMessage"/> to convert.</param>
        /// <returns>The <see cref="ErrorResponse"/> object.</returns>
        private async Task<ErrorResponse> ConvertErrorResponseAsync(HttpResponseMessage response)
        {
            try
            {
                using (var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                {
                    return this.Serializer.DeserializeObject<ErrorResponse>(responseStream);
                }
            }
            catch (Exception)
            {
                // If there's an exception deserializing the error response return null and throw a generic
                // ServiceException later.
                return null;
            }
        }

        private async Task<HttpResponseMessage> SendRequestAsync(
            HttpRequestMessage request,
            HttpCompletionOption completionOption,
            CancellationToken cancellationToken)
        {
            try
            {
                return await this.httpClient.SendAsync(request, completionOption, cancellationToken).ConfigureAwait(false);
            }
            catch (TaskCanceledException exception)
            {
                throw new ServiceException(
                    new Error
                    {
                        Code = ErrorConstants.Codes.Timeout,
                        Message = ErrorConstants.Messages.RequestTimedOut,
                    },
                    exception);
            }
            catch (ServiceException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new ServiceException(
                    new Error
                    {
                        Code = ErrorConstants.Codes.GeneralException,
                        Message = ErrorConstants.Messages.UnexpectedExceptionOnSend,
                    },
                    exception);
            }
        }

    }
}
