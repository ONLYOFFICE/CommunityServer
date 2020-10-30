// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A <see cref="DelegatingHandler"/> implementation that is used for simulating server failures.
    /// </summary>
    public class ChaosHandler : DelegatingHandler
    {
        private DiagnosticSource _logger = new DiagnosticListener("Microsoft.Graph.ChaosHandler");

        private Random _random;
        private ChaosHandlerOption _globalChaosHandlerOptions;
        private List<HttpResponseMessage> _KnownGraphFailures;

        /// <summary>
        /// Create a ChaosHandler.  
        /// </summary>
        /// <param name="chaosHandlerOptions">Optional parameter to change default behavior of handler.</param>
        public ChaosHandler(ChaosHandlerOption chaosHandlerOptions = null)
        {
            _globalChaosHandlerOptions = chaosHandlerOptions ?? new ChaosHandlerOption();
            _random = new Random(DateTime.Now.Millisecond);
            LoadKnownGraphFailures(_globalChaosHandlerOptions.KnownChaos);
        }

        /// <summary>
        /// Sends the request
        /// </summary>
        /// <param name="request">The request to send.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request.</param>
        /// <returns></returns>
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Select global or per request options
            var chaosHandlerOptions = GetPerRequestOptions(request) ??_globalChaosHandlerOptions;  

            HttpResponseMessage response = null;
            // Planned Chaos or Random?
            if (chaosHandlerOptions.PlannedChaosFactory != null)
            {
                response = chaosHandlerOptions.PlannedChaosFactory(request);
                if (response != null) 
                { 
                    response.RequestMessage = request;
                    if (_logger.IsEnabled("PlannedChaosResponse"))
                        _logger.Write("PlannedChaosResponse", response);
                }
            } 
            else 
            {
                if (_random.Next(100) < chaosHandlerOptions.ChaosPercentLevel)
                {
                    response = CreateChaosResponse(chaosHandlerOptions.KnownChaos ?? _KnownGraphFailures);
                    response.RequestMessage = request;
                    if (_logger.IsEnabled("ChaosResponse"))
                        _logger.Write("ChaosResponse", response);
                }
            }

            if (response == null)
            {
                response = await base.SendAsync(request, cancellationToken);
            }
            return response;
        }

        private ChaosHandlerOption GetPerRequestOptions(HttpRequestMessage request)
        {
            request.Properties.TryGetValue("ChaosRequestOptions", out var optionsObject);
            return (ChaosHandlerOption)optionsObject;
        }

        private HttpResponseMessage CreateChaosResponse(List<HttpResponseMessage> knownFailures)
        {
            var responseIndex = _random.Next(knownFailures.Count);
            return knownFailures[responseIndex];
        }

        private void LoadKnownGraphFailures(List<HttpResponseMessage> knownFailures)
        {
            if (knownFailures != null && knownFailures.Count > 0)
            {
                _KnownGraphFailures = knownFailures;
            } 
            else
            {
                _KnownGraphFailures = new List<HttpResponseMessage>();
                _KnownGraphFailures.Add(Create429TooManyRequestsResponse(new TimeSpan(0, 0, 3)));
                _KnownGraphFailures.Add(Create503Response(new TimeSpan(0, 0, 3)));
                _KnownGraphFailures.Add(Create504GatewayTimeoutResponse(new TimeSpan(0, 0, 3)));
            }
        }

        /// <summary>
        /// Create a HTTP status 429 response message
        /// </summary>
        /// <param name="retry"><see cref="TimeSpan"/> for retry condition header value</param>
        /// <returns>A <see cref="HttpResponseMessage"/> object simulating a 429 response</returns>
        public static HttpResponseMessage Create429TooManyRequestsResponse(TimeSpan retry)
        {
            var serializer = new Serializer();
            var throttleResponse = new HttpResponseMessage()
            {
                StatusCode = (HttpStatusCode)429,
                Content = serializer.SerializeAsJsonContent(new { error = new Error() { Code = "activityLimitReached", Message= "Client application has been throttled and should not attempt to repeat the request until an amount of time has elapsed." } })
            };
            throttleResponse.Headers.RetryAfter = new RetryConditionHeaderValue(retry);
            return throttleResponse;
        }

        /// <summary>
        /// Create a HTTP status 503 response message
        /// </summary>
        /// <param name="retry"><see cref="TimeSpan"/> for retry condition header value</param>
        /// <returns>A <see cref="HttpResponseMessage"/> object simulating a 503 response</returns>
        public static HttpResponseMessage Create503Response(TimeSpan retry)
        {
            var serializer = new Serializer();
            var serverUnavailableResponse = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.ServiceUnavailable,
                Content = serializer.SerializeAsJsonContent(new { error = new Error() { Code = "serviceNotAvailable", Message= "The service is temporarily unavailable for maintenance or is overloaded. You may repeat the request after a delay, the length of which may be specified in a Retry-After header." } })
            };
            serverUnavailableResponse.Headers.RetryAfter = new RetryConditionHeaderValue(retry);
            return serverUnavailableResponse;
        }

        /// <summary>
        /// Create a HTTP status 502 response message
        /// </summary>
        /// <returns>A <see cref="HttpResponseMessage"/> object simulating a 502 Response</returns>
        public static HttpResponseMessage Create502BadGatewayResponse()
        {
            var serializer = new Serializer();
            var badGatewayResponse = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.BadGateway,
                Content = serializer.SerializeAsJsonContent(new { error = new Error() { Code = "502" } })
            };
            return badGatewayResponse;
        }

        /// <summary>
        /// Create a HTTP status 500 response message
        /// </summary>
        /// <returns>A <see cref="HttpResponseMessage"/> object simulating a 500 Response</returns>
        public static HttpResponseMessage Create500InternalServerErrorResponse()
        {
            var serializer = new Serializer();
            var internalServerError = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = serializer.SerializeAsJsonContent(new { error = new Error() { Code = "generalException", Message= "There was an internal server error while processing the request." } })
            };
            return internalServerError;
        }

        /// <summary>
        /// Create a HTTP status 504 response message
        /// </summary>
        /// <param name="retry"><see cref="TimeSpan"/> for retry condition header value</param>
        /// <returns>A <see cref="HttpResponseMessage"/> object simulating a 504 response</returns>
        public static HttpResponseMessage Create504GatewayTimeoutResponse(TimeSpan retry)
        {
            var serializer = new Serializer();
            var gatewayTimeoutResponse = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.GatewayTimeout,
                Content = serializer.SerializeAsJsonContent(new { error = new Error() { Code = "504", Message = "The server, while acting as a proxy, did not receive a timely response from the upstream server it needed to access in attempting to complete the request. May occur together with 503." } })
            };
            gatewayTimeoutResponse.Headers.RetryAfter = new RetryConditionHeaderValue(retry);
            return gatewayTimeoutResponse;
        }

    }
}
