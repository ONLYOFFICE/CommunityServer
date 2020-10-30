// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Test.OneDrive.Sdk.Mocks
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public class ExceptionHttpMessageHandler : HttpMessageHandler
    {
        private Exception exceptionToThrow;

        public ExceptionHttpMessageHandler(Exception exceptionToThrow)
        {
            this.exceptionToThrow = exceptionToThrow;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            throw exceptionToThrow;
        }
    }
}
