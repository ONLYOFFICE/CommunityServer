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
    /// An <see cref="DelegatingHandler"/> implementation using standard .NET libraries.
    /// </summary>
    public class RedirectHandler : DelegatingHandler
    {
        /// <summary>
        /// RedirectOption property
        /// </summary>
        internal RedirectHandlerOption RedirectOption { get; set; }

        /// <summary>
        /// Constructs a new <see cref="RedirectHandler"/> 
        /// </summary>
        /// <param name="redirectOption">An OPTIONAL <see cref="Microsoft.Graph.RedirectHandlerOption"/> to configure <see cref="RedirectHandler"/></param>
        public RedirectHandler(RedirectHandlerOption redirectOption = null)
        {
            RedirectOption = redirectOption ?? new RedirectHandlerOption();
        }

        /// <summary>
        /// Constructs a new <see cref="RedirectHandler"/> 
        /// </summary>
        /// <param name="innerHandler">An HTTP message handler to pass to the <see cref="HttpMessageHandler"/> for sending requests.</param>
        /// <param name="redirectOption">An OPTIONAL <see cref="Microsoft.Graph.RedirectHandlerOption"/> to configure <see cref="RedirectHandler"/></param>
        public RedirectHandler(HttpMessageHandler innerHandler, RedirectHandlerOption redirectOption = null)
            : this(redirectOption)
        {
            InnerHandler = innerHandler;
        }

        /// <summary>
        /// Sends the Request 
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/> to send.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>for the request.</param>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RedirectOption = request.GetMiddlewareOption<RedirectHandlerOption>() ?? RedirectOption;

            // send request first time to get response
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            // check response status code and redirect handler option
            if (IsRedirect(response.StatusCode) && RedirectOption.ShouldRedirect(response) && RedirectOption.MaxRedirect > 0)
            {
                if (response.Headers.Location == null)
                {
                    throw new ServiceException(
                        new Error
                        {
                            Code = ErrorConstants.Codes.GeneralException,
                            Message = ErrorConstants.Messages.LocationHeaderNotSetOnRedirect,
                        });
                }

                var redirectCount = 0;

                while (redirectCount < RedirectOption.MaxRedirect)
                {
                    // Drain response content to free responses.
                    if (response.Content != null)
                    {
                        await response.Content.ReadAsByteArrayAsync();
                    }

                    // general clone request with internal CloneAsync (see CloneAsync for details) extension method 
                    var newRequest = await response.RequestMessage.CloneAsync();

                    // status code == 303: change request method from post to get and content to be null
                    if (response.StatusCode == HttpStatusCode.SeeOther)
                    {
                        newRequest.Content = null;
                        newRequest.Method = HttpMethod.Get;
                    }

                    // Set newRequestUri from response
                    newRequest.RequestUri = response.Headers.Location;

                    // Remove Auth if http request's scheme or host changes
                    if (String.CompareOrdinal(newRequest.RequestUri.Host, request.RequestUri.Host) != 0 ||
                        String.CompareOrdinal(newRequest.RequestUri.Scheme, request.RequestUri.Scheme) != 0)
                    {
                        newRequest.Headers.Authorization = null;
                    }

                    // Send redirect request to get reponse      
                    response = await base.SendAsync(newRequest, cancellationToken);

                    // Check response status code
                    if (!IsRedirect(response.StatusCode))
                    {
                        return response;
                    }
                    redirectCount++;
                }

                throw new ServiceException(
                        new Error
                        {
                            Code = ErrorConstants.Codes.TooManyRedirects,
                            Message = string.Format(ErrorConstants.Messages.TooManyRedirectsFormatString, redirectCount)
                        });

            }
            return response;
        }

        /// <summary>
        /// Checks whether <see cref="HttpStatusCode"/> is redirected
        /// </summary>
        /// <param name="statusCode">The <see cref="HttpStatusCode"/>.</param>
        /// <returns>Bool value for redirection or not</returns>
        private bool IsRedirect(HttpStatusCode statusCode)
        {
            if (statusCode == HttpStatusCode.MovedPermanently ||
                statusCode == HttpStatusCode.Found ||
                statusCode == HttpStatusCode.SeeOther ||
                statusCode == HttpStatusCode.TemporaryRedirect ||
                statusCode == (HttpStatusCode)308
                )
            {
                return true;
            }
            return false;
        }


    }

}
