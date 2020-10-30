using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.OneDrive.Sdk;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using IAuthenticationProvider = Microsoft.Graph.IAuthenticationProvider;

namespace Test.OneDrive.Sdk.Requests
{
    using System.Net.Configuration;
    using System.Text;
    using Microsoft.Graph;
    using Test.OneDrive.Sdk.Mocks;

    [TestClass]
    public class UploadChunkRequestTest : RequestTestBase
    {
        [TestMethod]
        public async Task PutAsync_ReturnOK()
        {
            var responseString = "";
            using (var httpResponseMessage = new HttpResponseMessage())
            using (var uploadStream = new MemoryStream(new byte[100]))
            using (var responseStream = new MemoryStream(Encoding.UTF8.GetBytes(responseString)))
            using (var streamContent = new StreamContent(responseStream))
            {
                httpResponseMessage.Content = streamContent;
                httpResponseMessage.StatusCode = HttpStatusCode.OK;
                const string AuthorizationHeaderName = "Authorization";
                const string AuthorizationHeaderValue = "token";
                var sessionUrl = "https://api.onedrive.com/v1.0/up/123";
                var item = new Item() { Name = "uploaded" };

                this.serializer.Setup(s => s.DeserializeObject<Item>(It.Is<string>(str => str.Equals(responseString))))
                    .Returns(item);

                this.httpProvider.Setup(
                    provider => provider.SendAsync(
                        It.Is<HttpRequestMessage>(
                            request => request.RequestUri.ToString().Equals(sessionUrl)),
                        HttpCompletionOption.ResponseContentRead,
                        CancellationToken.None)).Returns(Task.FromResult<HttpResponseMessage>(httpResponseMessage));

                this.authenticationProvider.Setup(
                    provider => provider.AuthenticateRequestAsync(
                        It.IsAny<HttpRequestMessage>())).Returns(
                            (HttpRequestMessage msg) =>
                            {
                                msg.Headers.Add(AuthorizationHeaderName, AuthorizationHeaderValue);
                                return Task.FromResult(0);
                            });

                UploadChunkRequest uploadRequest = new UploadChunkRequest(sessionUrl, this.oneDriveClient, null, 0, 100, 200);

                var result = await uploadRequest.PutAsync(uploadStream);
                Assert.IsNotNull(result.ItemResponse, "result item is null");
                Assert.AreEqual(item, result.ItemResponse, "result returned is not expected");
                // make sure no auth header is added to the upload request
                Assert.IsFalse(uploadRequest.Headers.Contains(new HeaderOption(AuthorizationHeaderName, AuthorizationHeaderValue)));
            }
        }
    }
}