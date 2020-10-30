// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Test.OneDrive.Sdk.Mocks
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public class TestHttpMessageHandler : HttpMessageHandler
    {
        private Dictionary<string, HttpResponseMessage> responseMessages;

        public TestHttpMessageHandler()
        {
            this.responseMessages = new Dictionary<string, HttpResponseMessage>();
        }

        public void AddResponseMapping(string requestUrl, HttpResponseMessage responseMessage)
        {
            this.responseMessages.Add(requestUrl, responseMessage);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage responseMessage;

            if (this.responseMessages.TryGetValue(request.RequestUri.ToString(), out responseMessage))
            {
                responseMessage.RequestMessage = request;
                return Task.FromResult(responseMessage);
            }

            return Task.FromResult<HttpResponseMessage>(null);
        }
    }
}
