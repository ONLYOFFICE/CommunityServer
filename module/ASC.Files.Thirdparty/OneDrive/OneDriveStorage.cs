/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
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
                    _token = OAuth20TokenHelper.RefreshToken(OneDriveLoginProvider.OneDriveOauthTokenUrl, _token);
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

        public long MaxUploadFileSize { get; internal set; }

        public long MaxChunkedUploadFileSize { get; internal set; }

        public OneDriveStorage()
        {
            MaxUploadFileSize = MaxChunkedUploadFileSize = 10L*1024L*1024L*1024L;
        }

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
            return GetItemRequest(itemId).Request().GetAsync().Result;
        }

        public List<Item> GetItems(string folderId, int limit = 500)
        {
            return new List<Item>(GetItemRequest(folderId).Children.Request().GetAsync().Result);
        }

        public Stream DownloadStream(Item file)
        {
            if (file == null || file.File == null) throw new ArgumentNullException("file");

            return OnedriveClient
                .Drive
                .Items[file.Id]
                .Content
                .Request()
                .GetAsync()
                .Result;
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

        public Item MoveItem(string itemId, string toFolderId)
        {
            var updateItem = new Item { ParentReference = new ItemReference { Id = toFolderId } };

            return OnedriveClient
                .Drive
                .Items[itemId]
                .Request()
                .UpdateAsync(updateItem)
                .Result;
        }

        public Item CopyItem(Item item, string toFolderId)
        {
            var copyMonitor = OnedriveClient
                .Drive
                .Items[item.Id]
                .Copy(item.Name, new ItemReference { Id = toFolderId })
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