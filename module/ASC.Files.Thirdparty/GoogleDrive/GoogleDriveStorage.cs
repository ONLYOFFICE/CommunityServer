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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Web;
using ASC.FederatedLogin;
using ASC.FederatedLogin.Helpers;
using ASC.FederatedLogin.LoginProviders;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Core;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Newtonsoft.Json.Linq;
using DriveFile = Google.Apis.Drive.v3.Data.File;
using MimeMapping = ASC.Common.Web.MimeMapping;

namespace ASC.Files.Thirdparty.GoogleDrive
{
    internal class GoogleDriveStorage
    {
        private OAuth20Token _token;

        private string AccessToken
        {
            get
            {
                if (_token == null) throw new Exception("Cannot create GoogleDrive session with given token");
                if (_token.IsExpired) _token = OAuth20TokenHelper.RefreshToken<GoogleLoginProvider>(_token);
                return _token.AccessToken;
            }
        }

        private DriveService _driveService;

        public bool IsOpened { get; private set; }

        public const long MaxChunkedUploadFileSize = 2L*1024L*1024L*1024L;

        public void Open(OAuth20Token token)
        {
            if (IsOpened)
                return;

            if (token == null) throw new UnauthorizedAccessException("Cannot create GoogleDrive session with given token");
            _token = token;

            var tokenResponse = new TokenResponse
                {
                    AccessToken = _token.AccessToken,
                    RefreshToken = _token.RefreshToken,
                    IssuedUtc = _token.Timestamp,
                    ExpiresInSeconds = _token.ExpiresIn,
                    TokenType = "Bearer"
                };

            var apiCodeFlow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = new ClientSecrets
                        {
                            ClientId = _token.ClientID,
                            ClientSecret = _token.ClientSecret
                        },
                    Scopes = new[] { DriveService.Scope.Drive }
                });

            _driveService = new DriveService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = new UserCredential(apiCodeFlow, string.Empty, tokenResponse)
                });

            IsOpened = true;
        }

        public void Close()
        {
            _driveService.Dispose();

            IsOpened = false;
        }


        public string GetRootFolderId()
        {
            var rootFolder = _driveService.Files.Get("root").Execute();

            return rootFolder.Id;
        }

        public DriveFile GetEntry(string entryId)
        {
            try
            {
                var request = _driveService.Files.Get(entryId);

                request.Fields = GoogleLoginProvider.FilesFields;

                return request.Execute();
            }
            catch (GoogleApiException ex)
            {
                if (ex.HttpStatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                throw;
            }
        }

        public List<DriveFile> GetEntries(string folderId, bool? folders = null)
        {
            var request = _driveService.Files.List();

            var query = "'" + folderId + "' in parents and trashed=false";

            if (folders.HasValue)
            {
                query += " and mimeType " + (folders.Value ? "" : "!") + "= '" + GoogleLoginProvider.GoogleDriveMimeTypeFolder + "'";
            }

            request.Q = query;

            request.Fields = "nextPageToken, files(" + GoogleLoginProvider.FilesFields + ")";

            var files = new List<DriveFile>();
            do
            {
                try
                {
                    var fileList = request.Execute();

                    files.AddRange(fileList.Files);

                    request.PageToken = fileList.NextPageToken;
                }
                catch (Exception)
                {
                    request.PageToken = null;
                }
            } while (!String.IsNullOrEmpty(request.PageToken));

            return files;
        }

        public Stream DownloadStream(DriveFile file, int offset = 0)
        {
            if (file == null) throw new ArgumentNullException("file");

            var downloadArg = string.Format("{0}?alt=media", file.Id);

            var ext = MimeMapping.GetExtention(file.MimeType);
            if (GoogleLoginProvider.GoogleDriveExt.Contains(ext))
            {
                var internalExt = FileUtility.GetGoogleDownloadableExtension(ext);
                var requiredMimeType = MimeMapping.GetMimeMapping(internalExt);

                downloadArg = string.Format("{0}/export?mimeType={1}",
                                            file.Id,
                                            HttpUtility.UrlEncode(requiredMimeType));
            }

            var request = WebRequest.Create(GoogleLoginProvider.GoogleUrlFile + downloadArg);
            request.Method = "GET";
            request.Headers.Add("Authorization", "Bearer " + AccessToken);

            var response = (HttpWebResponse)request.GetResponse();

            if (offset == 0 && file.Size.HasValue && file.Size > 0)
            {
                return new ResponseStream(response.GetResponseStream(), file.Size.Value);
            }

            var tempBuffer = new FileStream(Path.GetTempFileName(), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read, 8096, FileOptions.DeleteOnClose);
            using (var str = response.GetResponseStream())
            {
                if (str != null)
                {
                    str.CopyTo(tempBuffer);
                    tempBuffer.Flush();
                    tempBuffer.Seek(offset, SeekOrigin.Begin);
                }
            }

            return tempBuffer;
        }

        public DriveFile InsertEntry(Stream fileStream, string title, string parentId, bool folder = false)
        {
            var mimeType = folder ? GoogleLoginProvider.GoogleDriveMimeTypeFolder : MimeMapping.GetMimeMapping(title);

            var body = FileConstructor(title, mimeType, parentId);

            if (folder)
            {
                var requestFolder = _driveService.Files.Create(body);
                requestFolder.Fields = GoogleLoginProvider.FilesFields;
                return requestFolder.Execute();
            }

            var request = _driveService.Files.Create(body, fileStream, mimeType);
            request.Fields = GoogleLoginProvider.FilesFields;

            var result = request.Upload();
            if (result.Exception != null)
            {
                if (request.ResponseBody == null) throw result.Exception;
                Global.Logger.Error("Error while trying to insert entity. GoogleDrive insert returned an error.", result.Exception);
            }
            return request.ResponseBody;
        }

        public void DeleteEntry(string entryId)
        {
            _driveService.Files.Delete(entryId).Execute();
        }

        public DriveFile InsertEntryIntoFolder(DriveFile entry, string folderId)
        {
            var request = _driveService.Files.Update(FileConstructor(), entry.Id);
            request.AddParents = folderId;
            request.Fields = GoogleLoginProvider.FilesFields;
            return request.Execute();
        }

        public DriveFile RemoveEntryFromFolder(DriveFile entry, string folderId)
        {
            var request = _driveService.Files.Update(FileConstructor(), entry.Id);
            request.RemoveParents = folderId;
            request.Fields = GoogleLoginProvider.FilesFields;
            return request.Execute();
        }

        public DriveFile CopyEntry(string toFolderId, string originEntryId)
        {
            var body = FileConstructor(folderId: toFolderId);
            try
            {
                var request = _driveService.Files.Copy(body, originEntryId);
                request.Fields = GoogleLoginProvider.FilesFields;
                return request.Execute();
            }
            catch (GoogleApiException ex)
            {
                if (ex.HttpStatusCode == HttpStatusCode.Forbidden)
                {
                    throw new SecurityException(ex.Error.Message);
                }
                throw;
            }
        }

        public DriveFile RenameEntry(string fileId, string newTitle)
        {
            var request = _driveService.Files.Update(FileConstructor(newTitle), fileId);
            request.Fields = GoogleLoginProvider.FilesFields;
            return request.Execute();
        }

        public DriveFile SaveStream(string fileId, Stream fileStream, string fileTitle)
        {
            var mimeType = MimeMapping.GetMimeMapping(fileTitle);
            var file = FileConstructor(fileTitle, mimeType);

            var request = _driveService.Files.Update(file, fileId, fileStream, mimeType);
            request.Fields = GoogleLoginProvider.FilesFields;
            var result = request.Upload();
            if (result.Exception != null)
            {
                if (request.ResponseBody == null) throw result.Exception;
                Global.Logger.Error("Error while trying to insert entity. GoogleDrive save returned an error.", result.Exception);
            }

            return request.ResponseBody;
        }

        public DriveFile FileConstructor(string title = null, string mimeType = null, string folderId = null)
        {
            var file = new DriveFile();

            if (!string.IsNullOrEmpty(title)) file.Name = title;
            if (!string.IsNullOrEmpty(mimeType)) file.MimeType = mimeType;
            if (!String.IsNullOrEmpty(folderId)) file.Parents = new List<string> { folderId };

            return file;
        }

        public ResumableUploadSession CreateResumableSession(DriveFile driveFile, long contentLength)
        {
            if (driveFile == null) throw new ArgumentNullException("driveFile");

            var fileId = string.Empty;
            var method = "POST";
            var body = string.Empty;
            var folderId = driveFile.Parents.FirstOrDefault();

            if (driveFile.Id != null)
            {
                fileId = "/" + driveFile.Id;
                method = "PATCH";
            }
            else
            {
                var titleData = !string.IsNullOrEmpty(driveFile.Name) ? string.Format("\"name\":\"{0}\"", driveFile.Name) : "";
                var parentData = !string.IsNullOrEmpty(folderId) ? string.Format(",\"parents\":[\"{0}\"]", folderId) : "";

                body = !string.IsNullOrEmpty(titleData + parentData) ? string.Format("{{{0}{1}}}", titleData, parentData) : "";
            }

            var request = WebRequest.Create(GoogleLoginProvider.GoogleUrlFileUpload + fileId + "?uploadType=resumable");
            request.Method = method;

            var bytes = Encoding.UTF8.GetBytes(body);
            request.ContentLength = bytes.Length;
            request.ContentType = "application/json; charset=UTF-8";
            request.Headers.Add("X-Upload-Content-Type", MimeMapping.GetMimeMapping(driveFile.Name));
            request.Headers.Add("X-Upload-Content-Length", contentLength.ToString(CultureInfo.InvariantCulture));
            request.Headers.Add("Authorization", "Bearer " + AccessToken);

            request.GetRequestStream().Write(bytes, 0, bytes.Length);

            var uploadSession = new ResumableUploadSession(driveFile.Id, folderId, contentLength);
            using (var response = request.GetResponse())
            {
                uploadSession.Location = response.Headers["Location"];
            }
            uploadSession.Status = ResumableUploadSessionStatus.Started;

            return uploadSession;
        }

        public void Transfer(ResumableUploadSession googleDriveSession, Stream stream, long chunkLength)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            if (googleDriveSession.Status != ResumableUploadSessionStatus.Started)
                throw new InvalidOperationException("Can't upload chunk for given upload session.");

            var request = WebRequest.Create(googleDriveSession.Location);
            request.Method = "PUT";
            request.ContentLength = chunkLength;
            request.Headers.Add("Authorization", "Bearer " + AccessToken);
            request.Headers.Add("Content-Range", string.Format("bytes {0}-{1}/{2}",
                                                               googleDriveSession.BytesTransfered,
                                                               googleDriveSession.BytesTransfered + chunkLength - 1,
                                                               googleDriveSession.BytesToTransfer));

            using (var requestStream = request.GetRequestStream())
            {
                stream.CopyTo(requestStream);
            }

            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException exception)
            {
                if (exception.Status == WebExceptionStatus.ProtocolError)
                {
                    if (exception.Response != null && exception.Response.Headers.AllKeys.Contains("Range"))
                    {
                        response = (HttpWebResponse)exception.Response;
                    }
                    else if (exception.Message.Equals("Invalid status code: 308", StringComparison.InvariantCulture)) //response is null (unix)
                    {
                        response = null;
                    }
                    else
                    {
                        throw;
                    }
                }
                else
                {
                    throw;
                }
            }

            if (response == null || response.StatusCode != HttpStatusCode.Created && response.StatusCode != HttpStatusCode.OK)
            {
                var uplSession = googleDriveSession;
                uplSession.BytesTransfered += chunkLength;

                if (response != null)
                {
                    var locationHeader = response.Headers["Location"];
                    if (!string.IsNullOrEmpty(locationHeader))
                    {
                        uplSession.Location = locationHeader;
                    }
                }
            }
            else
            {
                googleDriveSession.Status = ResumableUploadSessionStatus.Completed;

                using (var responseStream = response.GetResponseStream())
                {
                    if (responseStream == null) return;
                    string responseString;
                    using (var readStream = new StreamReader(responseStream))
                    {
                        responseString = readStream.ReadToEnd();
                    }
                    var responseJson = JObject.Parse(responseString);

                    googleDriveSession.FileId = responseJson.Value<string>("id");
                }
            }

            if (response != null)
            {
                response.Close();
            }
        }

        public long GetMaxUploadSize()
        {
            var request = _driveService.About.Get();
            request.Fields = "maxUploadSize";
            var about = request.Execute();

            return about.MaxUploadSize.HasValue ? about.MaxUploadSize.Value : MaxChunkedUploadFileSize;
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