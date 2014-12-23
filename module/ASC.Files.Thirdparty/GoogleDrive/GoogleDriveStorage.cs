/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using ASC.FederatedLogin;
using ASC.FederatedLogin.Helpers;
using ASC.Common.Web;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Core;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using Google.Apis.Services;
using Google.Apis.Upload;
using Newtonsoft.Json.Linq;
using File = Google.Apis.Drive.v2.Data.File;

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
                if (_token.IsExpired) _token = OAuth20TokenHelper.RefreshToken(GoogleUrlToken, _token);
                return _token.AccessToken;
            }
        }

        private DriveService _driveService;

        public static string GoogleFolderMimeType = "application/vnd.google-apps.folder";

        public static Dictionary<string, Tuple<string, string>> GoogleFilesMimeTypes = new Dictionary<string, Tuple<string, string>>
            {
                { "application/vnd.google-apps.document", new Tuple<string, string>(".gdoc", "application/vnd.openxmlformats-officedocument.wordprocessingml.document") },
                { "application/vnd.google-apps.spreadsheet", new Tuple<string, string>(".gsheet", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") },
                { "application/vnd.google-apps.presentation", new Tuple<string, string>(".gslides", "application/vnd.openxmlformats-officedocument.presentationml.presentation") }
            };

        public const string GoogleUrlToken = "https://accounts.google.com/o/oauth2/token";
        public const string GoogleUrlUpload = "https://www.googleapis.com/upload/drive/v2/files";

        public bool IsOpened { get; private set; }

        public long MaxUploadFileSize { get; internal set; }

        public long MaxChunkedUploadFileSize { get; internal set; }

        public GoogleDriveStorage()
        {
            MaxUploadFileSize = MaxChunkedUploadFileSize = 2L*1024L*1024L*1024L;
        }

        public void Open(OAuth20Token token)
        {
            if (IsOpened)
                return;

            if (token == null) throw new UnauthorizedAccessException("Cannot create GoogleDrive session with given token");
            if (token.IsExpired) token = OAuth20TokenHelper.RefreshToken(GoogleUrlToken, token);
            _token = token;

            var tokenResponse = new TokenResponse
                {
                    AccessToken = _token.AccessToken,
                    RefreshToken = _token.RefreshToken,
                    Issued = _token.Timestamp,
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
            var about = _driveService.About.Get().Execute();

            return about.RootFolderId;
        }

        public File GetEntry(string entryId)
        {
            return _driveService.Files.Get(entryId).Execute();
        }

        public List<File> GetEntries(string folderId, bool? folders = null)
        {
            var request = _driveService.Files.List();

            var query = "'" + folderId + "' in parents";

            if (folders.HasValue)
            {
                query += " and mimeType " + (folders.Value ? "" : "!") + "= '" + GoogleFolderMimeType + "'";
            }

            request.Q = query;

            var files = new List<File>();
            do
            {
                try
                {
                    var fileList = request.Execute();

                    files.AddRange(fileList.Items);

                    request.PageToken = fileList.NextPageToken;
                }
                catch (Exception)
                {
                    request.PageToken = null;
                }
            } while (!String.IsNullOrEmpty(request.PageToken));

            return files;
        }

        public Stream DownloadStream(File file)
        {
            if (file == null) throw new ArgumentNullException("file");
            var downloadUrl = file.DownloadUrl;
            if (String.IsNullOrEmpty(downloadUrl))
            {
                if (file.ExportLinks == null)
                {
                    return null;
                }

                Tuple<string, string> mimeData;
                if (GoogleFilesMimeTypes.TryGetValue(file.MimeType, out mimeData))
                {
                    downloadUrl = file.ExportLinks[mimeData.Item2];
                }
                else
                {
                    // The file doesn't have any content stored on Drive.
                    return null;
                }
            }

            try
            {
                var request = WebRequest.Create(downloadUrl);
                request.Method = "GET";
                request.Headers.Add("Authorization", "Bearer " + AccessToken);

                var response = (HttpWebResponse)request.GetResponse();

                if (file.FileSize.HasValue && file.FileSize > 0)
                {
                    return new ResponseStream(response.GetResponseStream(), file.FileSize.Value);
                }

                var isChukedEncoding = string.Equals(response.Headers.Get("Transfer-Encoding"), "Chunked", StringComparison.OrdinalIgnoreCase);
                if (!isChukedEncoding)
                {
                    return new ResponseStream(response.GetResponseStream(), response.ContentLength);
                }

                var tempBuffer = new FileStream(Path.GetTempFileName(), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read, 8096, FileOptions.DeleteOnClose);
                var str = response.GetResponseStream();
                if (str != null)
                {
                    str.CopyTo(tempBuffer);
                    tempBuffer.Flush();
                    tempBuffer.Seek(0, SeekOrigin.Begin);
                }
                return tempBuffer;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public File InsertEntry(Stream fileStream, string title, string parentId, bool folder = false)
        {
            var mimeType = folder ? GoogleFolderMimeType : MimeMapping.GetMimeMapping(title);

            var body = FileConstructor(title, mimeType, parentId);

            try
            {
                if (folder)
                {
                    return _driveService.Files.Insert(body).Execute();
                }

                var request = _driveService.Files.Insert(body, fileStream, mimeType);
                var result = request.Upload();
                if (result.Exception != null)
                {
                    Global.Logger.Error("Error while trying to insert entity. GoogleDrive insert returned an error.", result.Exception);
                }
                return request.ResponseBody;
            }
            catch (Exception error)
            {
                Global.Logger.Error("Error while trying to insert entity.", error);
                return null;
            }
        }

        public void DeleteEntry(string entryId)
        {
            _driveService.Files.Delete(entryId).Execute();
        }

        public void InsertEntryIntoFolder(string entryId, string folderId)
        {
            var newParent = new ParentReference { Id = folderId };
            _driveService.Parents.Insert(newParent, entryId).Execute();
        }

        public void RemoveEntryFromFolder(string entryId, string folderId)
        {
            _driveService.Parents.Delete(entryId, folderId).Execute();
        }

        public File CopyEntry(string toFolderId, string originEntryId)
        {
            var body = FileConstructor(folderId: toFolderId);

            return _driveService.Files.Copy(body, originEntryId).Execute();
        }

        public File UpdateEntry(File entry)
        {
            var request = _driveService.Files.Update(entry, entry.Id);
            request.NewRevision = false;
            return request.Execute();
        }

        public File SaveStream(string fileId, Stream fileStream, string fileTitle)
        {
            var file = FileConstructor();
            var mimeType = MimeMapping.GetMimeMapping(fileTitle);

            var request = _driveService.Files.Update(file, fileId, fileStream, mimeType);
            request.NewRevision = false;
            request.Upload();

            return request.ResponseBody;
        }

        public File FileConstructor(string title = null, string mimeType = null, string folderId = null)
        {
            var file = new File();

            if (!string.IsNullOrEmpty(title)) file.Title = title;
            if (!string.IsNullOrEmpty(mimeType)) file.MimeType = mimeType;
            if (!String.IsNullOrEmpty(folderId)) file.Parents = new List<ParentReference> { new ParentReference { Id = folderId } };

            return file;
        }

        public File FileFromString(string fileJsonString)
        {
            var fileJson = JObject.Parse(fileJsonString);

            var file = new File
                {
                    Id = fileJson.Value<string>("id"),
                    Title = fileJson.Value<string>("title"),
                    MimeType = fileJson.Value<string>("mimeType"),
                    FileSize = fileJson.Value<long>("fileSize"),
                    CreatedDate = fileJson.Value<DateTime>("createdDate"),
                    ModifiedDate = fileJson.Value<DateTime>("modifiedDate"),
                };

            var folderId = (string)fileJson.SelectToken("parents[0].id");
            if (!String.IsNullOrEmpty(folderId)) file.Parents = new List<ParentReference> { new ParentReference { Id = folderId } };

            return file;
        }

        public ResumableUploadSession CreateResumableSession(File driveFile, long contentLength)
        {
            if (driveFile == null) throw new ArgumentNullException("driveFile");

            var fileId = string.Empty;
            var method = "POST";
            var body = string.Empty;
            if (driveFile.Id != null)
            {
                fileId = "/" + driveFile.Id;
                method = "PUT";
            }
            else
            {
                var parents = driveFile.Parents.FirstOrDefault();
                var folderId = parents != null ? parents.Id : null;

                var titleData = !string.IsNullOrEmpty(driveFile.Title) ? string.Format("\"title\":\"{0}\"", driveFile.Title) : "";
                var parentData = !string.IsNullOrEmpty(folderId) ? string.Format(",\"parents\":[{{\"id\":\"{0}\"}}]", folderId) : "";

                body = !string.IsNullOrEmpty(titleData + parentData) ? string.Format("{{{0}{1}}}", titleData, parentData) : "";
            }

            var request = WebRequest.Create(GoogleUrlUpload + fileId + "?uploadType=resumable&conver=false");
            request.Method = method;

            var bytes = Encoding.UTF8.GetBytes(body);
            request.ContentLength = bytes.Length;
            request.ContentType = "application/json; charset=UTF-8";
            request.Headers.Add("X-Upload-Content-Type", MimeMapping.GetMimeMapping(driveFile.Title));
            request.Headers.Add("X-Upload-Content-Length", contentLength.ToString(CultureInfo.InvariantCulture));
            request.Headers.Add("Authorization", "Bearer " + AccessToken);

            request.GetRequestStream().Write(bytes, 0, bytes.Length);

            var uploadSession = new ResumableUploadSession(driveFile, contentLength);
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
                if (exception.Status == WebExceptionStatus.ProtocolError && exception.Response != null && exception.Response.Headers.AllKeys.Contains("Range"))
                {
                    response = (HttpWebResponse)exception.Response;
                }
                else
                {
                    throw;
                }
            }

            if (response.StatusCode != HttpStatusCode.Created && response.StatusCode != HttpStatusCode.OK)
            {
                var uplSession = googleDriveSession;
                uplSession.BytesTransfered += chunkLength;

                var locationHeader = response.Headers["Location"];
                if (!string.IsNullOrEmpty(locationHeader))
                {
                    uplSession.Location = locationHeader;
                }
            }
            else
            {
                googleDriveSession.Status = ResumableUploadSessionStatus.Completed;

                using (var responseStream = response.GetResponseStream())
                {
                    if (responseStream == null) return;

                    var respFile = FileFromString(new StreamReader(responseStream).ReadToEnd());
                    var initFile = googleDriveSession.File;

                    initFile.Id = respFile.Id;
                    initFile.Title = respFile.Title;
                    initFile.MimeType = respFile.MimeType;
                    initFile.FileSize = respFile.FileSize;
                    initFile.CreatedDate = respFile.CreatedDate;
                    initFile.ModifiedDate = respFile.ModifiedDate;
                    initFile.Parents = respFile.Parents;
                }
            }

            response.Close();
        }
    }

    public enum ResumableUploadSessionStatus
    {
        None,
        Started,
        Completed,
        Aborted
    }

    internal class ResumableUploadSession
    {
        private readonly DateTime _createdOn = DateTime.UtcNow;

        public long BytesToTransfer { get; set; }

        public long BytesTransfered { get; set; }

        public File File { get; set; }

        public ResumableUploadSessionStatus Status { get; set; }

        public DateTime CreatedOn
        {
            get { return _createdOn; }
        }

        public string Location { get; set; }

        public ResumableUploadSession(File file, long bytesToTransfer)
        {
            File = file;
            BytesToTransfer = bytesToTransfer;
            Status = ResumableUploadSessionStatus.None;
        }
    }
}