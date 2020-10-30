// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Test.OneDrive.Sdk.Requests
{
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
    
    [TestClass]
    public class ItemRequestTests : RequestTestBase
    {
        [TestMethod]
        public async Task GetAsync_InitializeCollectionProperties()
        {
            using (var httpResponseMessage = new HttpResponseMessage())
            using (var responseStream = new MemoryStream())
            using (var streamContent = new StreamContent(responseStream))
            {
                httpResponseMessage.Content = streamContent;

                var requestUrl = "https://api.onedrive.com/v1.0/drive/items/id";
                this.httpProvider.Setup(
                    provider => provider.SendAsync(
                        It.Is<HttpRequestMessage>(
                            request => request.RequestUri.ToString().Equals(requestUrl)),
                        HttpCompletionOption.ResponseContentRead,
                        CancellationToken.None))
                    .Returns(Task.FromResult<HttpResponseMessage>(httpResponseMessage));

                var expectedChildrenPage = new ItemChildrenCollectionPage
                {
                    new Item { Id = "id" }
                };

                var expectedItemResponse = new Item
                {
                    AdditionalData = new Dictionary<string, object>
                    {
                        { "children@odata.nextLink", requestUrl + "/next" }
                    },
                    Children = expectedChildrenPage,
                };

                this.serializer.Setup(
                    serializer => serializer.DeserializeObject<Item>(It.IsAny<string>()))
                    .Returns(expectedItemResponse);

                var item = await this.oneDriveClient.Drive.Items["id"].Request().GetAsync();

                Assert.IsNotNull(item, "Item not returned.");
                Assert.IsNotNull(item.Children, "Item children not returned.");
                Assert.AreEqual(1, item.Children.CurrentPage.Count, "Unexpected number of children in page.");
                Assert.AreEqual("id", item.Children.CurrentPage[0].Id, "Unexpected child ID in page.");
                Assert.AreEqual(expectedItemResponse.AdditionalData, item.Children.AdditionalData, "Additional data not initialized correctly.");
                var nextPageRequest = item.Children.NextPageRequest as ItemChildrenCollectionRequest;
                Assert.IsNotNull(nextPageRequest, "Children next page request not initialized correctly.");
                Assert.AreEqual(new Uri(requestUrl + "/next"), new Uri(nextPageRequest.RequestUrl), "Unexpected request URL for next page request.");
            }
        }

        [TestMethod]
        public void ItemById_BuildRequest()
        {
            var expectedRequestUri = new Uri("https://api.onedrive.com/v1.0/drive/items/id");
            var itemRequestBuilder = this.oneDriveClient.Drive.Items["id"] as ItemRequestBuilder;

            Assert.IsNotNull(itemRequestBuilder, "Unexpected request builder.");
            Assert.AreEqual(expectedRequestUri, new Uri(itemRequestBuilder.RequestUrl), "Unexpected request URL.");

            var itemRequest = itemRequestBuilder.Request() as ItemRequest;
            Assert.IsNotNull(itemRequest, "Unexpected request.");
            Assert.AreEqual(expectedRequestUri, new Uri(itemRequest.RequestUrl), "Unexpected request URL.");
        }

        [TestMethod]
        public void ItemByPath_BuildRequest()
        {
            var expectedRequestUri = new Uri("https://api.onedrive.com/v1.0/drive/root:/item/with/path:");
            var itemRequestBuilder = this.oneDriveClient.Drive.Root.ItemWithPath("item/with/path") as ItemRequestBuilder;

            Assert.IsNotNull(itemRequestBuilder, "Unexpected request builder.");
            Assert.AreEqual(expectedRequestUri, new Uri(itemRequestBuilder.RequestUrl), "Unexpected request URL.");

            var itemRequest = itemRequestBuilder.Request() as ItemRequest;
            Assert.IsNotNull(itemRequest, "Unexpected request.");
            Assert.AreEqual(expectedRequestUri, new Uri(itemRequest.RequestUrl), "Unexpected request URL.");
        }

        [TestMethod]
        public void ItemByPath_BuildRequestWithLeadingSlash()
        {
            var expectedRequestUri = new Uri("https://api.onedrive.com/v1.0/drive/root:/item/with/path:");
            var itemRequestBuilder = this.oneDriveClient.Drive.Root.ItemWithPath("/item/with/path") as ItemRequestBuilder;

            Assert.IsNotNull(itemRequestBuilder, "Unexpected request builder.");
            Assert.AreEqual(expectedRequestUri, new Uri(itemRequestBuilder.RequestUrl), "Unexpected request URL.");

            var itemRequest = itemRequestBuilder.Request() as ItemRequest;
            Assert.IsNotNull(itemRequest, "Unexpected request.");
            Assert.AreEqual(expectedRequestUri, new Uri(itemRequest.RequestUrl), "Unexpected request URL.");
        }

        [TestMethod]
        public async Task ItemRequest_CreateAsync()
        {
            await this.RequestWithItemInBody(false);
        }

        [TestMethod]
        public async Task ItemRequest_DeleteAsync()
        {
            using (var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.NoContent))
            {
                var requestUrl = "https://api.onedrive.com/v1.0/drive/items/id";
                this.httpProvider.Setup(
                    provider => provider.SendAsync(
                        It.Is<HttpRequestMessage>(
                            request =>
                                request.Method == HttpMethod.Delete
                                && request.RequestUri.ToString().Equals(requestUrl)),
                        HttpCompletionOption.ResponseContentRead,
                        CancellationToken.None))
                    .Returns(Task.FromResult(httpResponseMessage));

                await this.oneDriveClient.Drive.Items["id"].Request().DeleteAsync();
            }
        }

        [TestMethod]
        public void ItemRequest_Expand()
        {
            var expectedRequestUri = new Uri("https://api.onedrive.com/v1.0/drive/items/id");
            var itemRequest = this.oneDriveClient.Drive.Items["id"].Request().Expand("value") as ItemRequest;

            Assert.IsNotNull(itemRequest, "Unexpected request.");
            Assert.AreEqual(expectedRequestUri, new Uri(itemRequest.RequestUrl), "Unexpected request URL.");
            Assert.AreEqual(1, itemRequest.QueryOptions.Count, "Unexpected query options present.");
            Assert.AreEqual("$expand", itemRequest.QueryOptions[0].Name, "Unexpected expand query name.");
            Assert.AreEqual("value", itemRequest.QueryOptions[0].Value, "Unexpected expand query value.");
        }

        [TestMethod]
        public void ItemRequest_Select()
        {
            var expectedRequestUri = new Uri("https://api.onedrive.com/v1.0/drive/items/id");
            var itemRequest = this.oneDriveClient.Drive.Items["id"].Request().Select("value") as ItemRequest;

            Assert.IsNotNull(itemRequest, "Unexpected request.");
            Assert.AreEqual(expectedRequestUri, new Uri(itemRequest.RequestUrl), "Unexpected request URL.");
            Assert.AreEqual(1, itemRequest.QueryOptions.Count, "Unexpected query options present.");
            Assert.AreEqual("$select", itemRequest.QueryOptions[0].Name, "Unexpected select query name.");
            Assert.AreEqual("value", itemRequest.QueryOptions[0].Value, "Unexpected select query value.");
        }

        [TestMethod]
        public async Task ItemRequest_UpdateAsync()
        {
            await this.RequestWithItemInBody(true);
        }
        
        private async Task RequestWithItemInBody(bool isUpdate)
        {
            using (var httpResponseMessage = new HttpResponseMessage())
            using (var responseStream = new MemoryStream())
            using (var streamContent = new StreamContent(responseStream))
            {
                httpResponseMessage.Content = streamContent;

                var requestUrl = "https://api.onedrive.com/v1.0/drive/items/id";
                this.httpProvider.Setup(
                        provider => provider.SendAsync(
                            It.Is<HttpRequestMessage>(
                                request =>
                                    string.Equals(request.Method.ToString().ToUpperInvariant(), isUpdate ? "PATCH" : "PUT")
                                    && string.Equals(request.Content.Headers.ContentType.ToString(), "application/json")
                                    && request.RequestUri.ToString().Equals(requestUrl)),
                            HttpCompletionOption.ResponseContentRead,
                            CancellationToken.None))
                        .Returns(Task.FromResult(httpResponseMessage));

                this.serializer.Setup(serializer => serializer.SerializeObject(It.IsAny<Item>())).Returns("body");
                this.serializer.Setup(serializer => serializer.DeserializeObject<Item>(It.IsAny<string>())).Returns(new Item { Id = "id" });

                var itemResponse = isUpdate
                    ? await this.oneDriveClient.Drive.Items["id"].Request().UpdateAsync(new Item())
                    : await this.oneDriveClient.Drive.Items["id"].Request().CreateAsync(new Item());

                Assert.AreEqual("id", itemResponse.Id, "Unexpected item returned.");
            }
        }
    }
}
