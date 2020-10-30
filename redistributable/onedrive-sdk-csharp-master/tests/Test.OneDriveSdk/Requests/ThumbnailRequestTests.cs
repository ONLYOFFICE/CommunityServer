// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------
namespace Test.OneDrive.Sdk.Requests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.OneDrive.Sdk;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ThumbnailRequestTests : RequestTestBase
    {
        [TestMethod]
        public void ThumbnailContentRequest_BuildRequest()
        {
            var expectedRequestUri = new Uri("https://api.onedrive.com/v1.0/drive/items/id/thumbnails/0/id/content");
            var thumbnailContentRequestBuilder = this.oneDriveClient.Drive.Items["id"].Thumbnails["0"]["id"].Content as ThumbnailContentRequestBuilder;

            Assert.IsNotNull(thumbnailContentRequestBuilder, "Unexpected request builder.");
            Assert.AreEqual(expectedRequestUri, new Uri(thumbnailContentRequestBuilder.RequestUrl), "Unexpected request URL.");

            var thumbnailContentRequest = thumbnailContentRequestBuilder.Request() as ThumbnailContentRequest;
            Assert.IsNotNull(thumbnailContentRequest, "Unexpected request.");
            Assert.AreEqual(expectedRequestUri, new Uri(thumbnailContentRequest.RequestUrl), "Unexpected request URL.");
        }

        [TestMethod]
        public async Task ThumbnailContentRequest_GetAsync()
        {
            using (var httpResponseMessage = new HttpResponseMessage())
            using (var stringContent = new StringContent("body"))
            {
                httpResponseMessage.Content = stringContent;

                var requestUrl = "https://api.onedrive.com/v1.0/drive/items/id/thumbnails/0/id/content";
                this.httpProvider.Setup(
                    provider => provider.SendAsync(
                        It.Is<HttpRequestMessage>(
                            request => request.RequestUri.ToString().StartsWith(requestUrl)),
                        HttpCompletionOption.ResponseContentRead,
                        CancellationToken.None))
                    .Returns(Task.FromResult(httpResponseMessage));

                using (var response = await this.oneDriveClient.Drive.Items["id"].Thumbnails["0"]["id"].Content.Request().GetAsync())
                {
                    Assert.IsNotNull(response, "Response stream not returned.");

                    using (var streamReader = new StreamReader(response))
                    {
                        var responseString = await streamReader.ReadToEndAsync();
                        Assert.AreEqual("body", responseString, "Unexpected response returned.");
                    }
                }
            }
        }

        [TestMethod]
        public async Task ThumbnailContentRequest_PutAsync()
        {
            using (var requestStream = new MemoryStream())
            using (var httpResponseMessage = new HttpResponseMessage())
            using (var responseStream = new MemoryStream())
            using (var streamContent = new StreamContent(responseStream))
            {
                httpResponseMessage.Content = streamContent;

                var requestUrl = "https://api.onedrive.com/v1.0/drive/items/id/thumbnails/0/id/content";
                this.httpProvider.Setup(
                    provider => provider.SendAsync(
                        It.Is<HttpRequestMessage>(
                            request => request.RequestUri.ToString().StartsWith(requestUrl)),
                        HttpCompletionOption.ResponseContentRead,
                        CancellationToken.None))
                    .Returns(Task.FromResult(httpResponseMessage));

                var expectedThumbnail = new Thumbnail { Url = "https://localhost" };

                this.serializer.Setup(
                    serializer => serializer.DeserializeObject<Thumbnail>(It.IsAny<string>()))
                    .Returns(expectedThumbnail);

                var responseThumbnail = await this.oneDriveClient.Drive.Items["id"].Thumbnails["0"]["id"].Content.Request().PutAsync<Thumbnail>(requestStream);

                Assert.IsNotNull(responseThumbnail, "Thumbnail not returned.");
                Assert.AreEqual(expectedThumbnail, responseThumbnail, "Unexpected thumbnail returned.");
            }
        }

        [TestMethod]
        public void ThumbnailSetExtensions_AdditionalDataNull()
        {
            var thumbnailSet = new ThumbnailSet();

            var thumbnail = thumbnailSet["custom"];

            Assert.IsNull(thumbnail, "Unexpected thumbnail returned.");
        }

        [TestMethod]
        public void ThumbnailSetExtensions_CustomThumbnail()
        {
            var expectedThumbnail = new Thumbnail { Url = "https://localhost" };
            var thumbnailSet = new ThumbnailSet
            {
                AdditionalData = new Dictionary<string, object>
                {
                    { "custom", expectedThumbnail }
                }
            };

            var thumbnail = thumbnailSet["custom"];

            Assert.IsNotNull(thumbnail, "Custom thumbnail not returned.");
            Assert.AreEqual(expectedThumbnail.Url, thumbnail.Url, "Unexpected thumbnail returned.");
        }

        [TestMethod]
        public void ThumbnailSetExtensions_CustomThumbnailNotFound()
        {
            var expectedThumbnail = new Thumbnail { Url = "https://localhost" };
            var thumbnailSet = new ThumbnailSet
            {
                AdditionalData = new Dictionary<string, object>
                {
                    { "custom", expectedThumbnail }
                }
            };

            var thumbnail = thumbnailSet["custom2"];

            Assert.IsNull(thumbnail, "Unexpected thumbnail returned.");
        }
    }
}
