/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using ASC.FederatedLogin;
using ASC.FederatedLogin.Helpers;
using ASC.FederatedLogin.LoginProviders;
using Microsoft.Graph;
using Microsoft.OneDrive.Sdk;
using Newtonsoft.Json.Linq;

namespace ASC.Files.Thirdparty.OneDrive
{
    internal class OneDriveStorage
    {
        private OAuth20Token _token;

        private string AccessToken
        {
            get
            {
                if (_token == null) throw new Exception("Cannot create OneDrive session with given token");
                if (_token.IsExpired)
                {
                    _token = OAuth20TokenHelper.RefreshToken<OneDriveLoginProvider>(_token);
                    _onedriveClientCache = null;
                }
                return _token.AccessToken;
            }
        }

        private OneDriveClient _onedriveClientCache;

        private OneDriveClient OnedriveClient
        {
            get { return _onedriveClientCache ?? (_onedriveClientCache = new OneDriveClient(new OneDriveAuthProvider(AccessToken))); }
        }

        public bool IsOpened { get; private set; }

        public long MaxChunkedUploadFileSize = 10L*1024L*1024L*1024L;

        public void Open(OAuth20Token token)
        {
            if (IsOpened)
                return;

            _token = token;

            IsOpened = true;
        }

        public void Close()
        {
            IsOpened = false;
        }

        public bool CheckAccess()
        {
            return OnedriveClient
                       .Drive
                       .Request()
                       .GetAsync()
                       .Result != null;
        }


        public static string RootPath = "/drive/root:";
        public static string ApiVersion = "v1.0";

        public static string MakeOneDrivePath(string parentPath, string name)
        {
            return (parentPath ?? "") + "/" + (name ?? "");
        }

        public Item GetItem(string itemId)
        {
            try
            {
                return GetItemRequest(itemId).Request().GetAsync().Result;
            }
            catch (Exception ex)
            {
                var serviceException = (ServiceException)ex.InnerException;
                if (serviceException != null && serviceException.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                throw;
            }
        }

        public List<Item> GetItems(string folderId, int limit = 500)
        {
            return new List<Item>(GetItemRequest(folderId).Children.Request().GetAsync().Result);
        }

        public Stream DownloadStream(Item file, int offset = 0)
        {
            if (file == null || file.File == null) throw new ArgumentNullException("file");

            var fileStream = OnedriveClient
                .Drive
                .Items[file.Id]
                .Content
                .Request()
                .GetAsync()
                .Result;

            if (fileStream != null && offset > 0)
                fileStream.Seek(offset, SeekOrigin.Begin);

            return fileStream;
        }

        public Item CreateFolder(string title, string parentId)
        {
            var newFolderItem = new Item
                {
                    Folder = new Folder(),
                    Name = title
                };

            return GetItemRequest(parentId)
                .Children
                .Request()
                .AddAsync(newFolderItem)
                .Result;
        }

        public Item CreateFile(Stream fileStream, string title, string parentPath)
        {
            return OnedriveClient
                .Drive
                .Root
                .ItemWithPath(MakeOneDrivePath(parentPath, title))
                .Content
                .Request()
                .PutAsync<Item>(fileStream)
                .Result;
        }

        public void DeleteItem(Item item)
        {
            OnedriveClient
                .Drive
                .Items[item.Id]
                .Request()
                .DeleteAsync();
        }

        public Item MoveItem(string itemId, string newItemName, string toFolderId)
        {
            var updateItem = new Item { ParentReference = new ItemReference { Id = toFolderId }, Name = newItemName };

            return OnedriveClient
                .Drive
                .Items[itemId]
                .Request()
                .UpdateAsync(updateItem)
                .Result;
        }

        public Item CopyItem(string itemId, string newItemName, string toFolderId)
        {
            var copyMonitor = OnedriveClient
                .Drive
                .Items[itemId]
                .Copy(newItemName, new ItemReference { Id = toFolderId })
                .Request()
                .PostAsync()
                .Result;

            return copyMonitor.PollForOperationCompletionAsync(null, CancellationToken.None).Result;
        }

        public Item RenameItem(string itemId, string newName)
        {
            var updateItem = new Item { Name = newName };
            return OnedriveClient
                .Drive
                .Items[itemId]
                .Request()
                .UpdateAsync(updateItem)
                .Result;
        }

        public Item SaveStream(string fileId, Stream fileStream)
        {
            return OnedriveClient
                .Drive
                .Items[fileId]
                .Content
                .Request()
                .PutAsync<Item>(fileStream)
                .Result;
        }

        private IItemRequestBuilder GetItemRequest(string itemId)
        {
            return string.IsNullOrEmpty(itemId)
                       ? OnedriveClient.Drive.Root
                       : OnedriveClient.Drive.Items[itemId];
        }


        public ResumableUploadSession CreateResumableSession(Item onedriveFile, long contentLength)
        {
            if (onedriveFile == null) throw new ArgumentNullException("onedriveFile");

            var folderId = onedriveFile.ParentReference.Id;
            var fileName = onedriveFile.Name;

            var uploadUriBuilder = new UriBuilder(OneDriveLoginProvider.OneDriveApiUrl)
                {
                    Path = "/" + ApiVersion + "/drive/items/" + folderId + ":/" + fileName + ":/oneDrive.createUploadSession"
                };

            var request = WebRequest.Create(uploadUriBuilder.Uri);
            request.Method = "POST";
            request.ContentLength = 0;

            request.ContentType = "application/json; charset=UTF-8";
            request.Headers.Add("Authorization", "Bearer " + AccessToken);

            var uploadSession = new ResumableUploadSession(onedriveFile.Id, folderId, contentLength);
            using (var response = request.GetResponse())
            using (var responseStream = response.GetResponseStream())
            {
                if (responseStream != null)
                {
                    using (var readStream = new StreamReader(responseStream))
                    {
                        var responseString = readStream.ReadToEnd();
                        var responseJson = JObject.Parse(responseString);
                        uploadSession.Location = responseJson.Value<string>("uploadUrl");
                    }
                }
            }

            uploadSession.Status = ResumableUploadSessionStatus.Started;

            return uploadSession;
        }

        public void Transfer(ResumableUploadSession oneDriveSession, Stream stream, long chunkLength)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            if (oneDriveSession.Status != ResumableUploadSessionStatus.Started)
                throw new InvalidOperationException("Can't upload chunk for given upload session.");

            var request = WebRequest.Create(oneDriveSession.Location);
            request.Method = "PUT";
            request.ContentLength = chunkLength;
            request.Headers.Add("Authorization", "Bearer " + AccessToken);
            request.Headers.Add("Content-Range", string.Format("bytes {0}-{1}/{2}",
                                                               oneDriveSession.BytesTransfered,
                                                               oneDriveSession.BytesTransfered + chunkLength - 1,
                                                               oneDriveSession.BytesToTransfer));

            using (var requestStream = request.GetRequestStream())
            {
                stream.CopyTo(requestStream);
            }

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode != HttpStatusCode.Created && response.StatusCode != HttpStatusCode.OK)
                {
                    oneDriveSession.BytesTransfered += chunkLength;
                }
                else
                {
                    oneDriveSession.Status = ResumableUploadSessionStatus.Completed;

                    using (var responseStream = response.GetResponseStream())
                    {
                        if (responseStream == null) return;
                        using (var readStream = new StreamReader(responseStream))
                        {
                            var responseString = readStream.ReadToEnd();
                            var responseJson = JObject.Parse(responseString);

                            oneDriveSession.FileId = responseJson.Value<string>("id");
                        }
                    }
                }
            }
        }

        public void CancelTransfer(ResumableUploadSession oneDriveSession)
        {
            var request = WebRequest.Create(oneDriveSession.Location);
            request.Method = "DELETE";
            using (request.GetResponse())
            {
            }
        }
    }



    public class OneDriveAuthProvider : IAuthenticationProvider
    {
        private readonly string _accessToken;

        public OneDriveAuthProvider(string accessToken)
        {
            _accessToken = accessToken;
        }

        public Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("bearer", _accessToken);
            return Task.WhenAll();
        }
    }

    public enum ResumableUploadSessionStatus
    {
        None,
        Started,
        Completed,
        Aborted
    }

    [Serializable]
    internal class ResumableUploadSession
    {
        public long BytesToTransfer { get; set; }

        public long BytesTransfered { get; set; }

        public string FileId { get; set; }

        public string FolderId { get; set; }

        public ResumableUploadSessionStatus Status { get; set; }

        public string Location { get; set; }

        public ResumableUploadSession(string fileId, string folderId, long bytesToTransfer)
        {
            FileId = fileId;
            FolderId = folderId;
            BytesToTransfer = bytesToTransfer;
            Status = ResumableUploadSessionStatus.None;
        }
    }
}