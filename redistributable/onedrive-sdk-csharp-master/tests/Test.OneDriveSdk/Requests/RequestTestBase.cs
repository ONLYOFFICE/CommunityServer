// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Test.OneDrive.Sdk.Requests
{
    using System.Net.Http;

    using Microsoft.OneDrive.Sdk;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mocks;

    [TestClass]
    public class RequestTestBase
    {
        protected MockAuthenticationProvider authenticationProvider;
        protected MockHttpProvider httpProvider;
        protected HttpResponseMessage httpResponseMessage;
        protected IOneDriveClient oneDriveClient;
        protected MockSerializer serializer;

        [TestInitialize]
        public void Setup()
        {
            this.authenticationProvider = new MockAuthenticationProvider();
            this.serializer = new MockSerializer();
            this.httpResponseMessage = new HttpResponseMessage();
            this.httpProvider = new MockHttpProvider(this.httpResponseMessage, this.serializer.Object);

            this.oneDriveClient = new OneDriveClient(
                "https://api.onedrive.com/v1.0",
                this.authenticationProvider.Object,
                this.httpProvider.Object);
        }

        [TestCleanup]
        public void Teardown()
        {
            this.httpResponseMessage.Dispose();
        }
    }
}
