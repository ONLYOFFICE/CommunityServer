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
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading;
using System.Web;
using ASC.Core;
using ASC.Core.Common.Configuration;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.FederatedLogin;
using ASC.FederatedLogin.Helpers;
using ASC.FederatedLogin.LoginProviders;
using ASC.FederatedLogin.Profile;
using ASC.Files.Core;
using ASC.MessagingSystem;
using ASC.Security.Cryptography;
using ASC.Web.Core;
using ASC.Web.Core.Files;
using ASC.Web.Core.Users;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Core;
using ASC.Web.Files.HttpHandlers;
using ASC.Web.Files.Resources;
using ASC.Web.Files.Services.DocumentService;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.UserControls.Common;
using ASC.Web.Studio.Utility;
using Newtonsoft.Json.Linq;
using File = ASC.Files.Core.File;
using MimeMapping = ASC.Common.Web.MimeMapping;
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Web.Files.ThirdPartyApp
{
    public class GoogleDriveApp : Consumer, IThirdPartyApp, IOAuthProvider
    {
        public const string AppAttr = "gdrive";

        public string Scopes { get { return ""; } }
        public string CodeUrl { get { return ""; } }
        public string AccessTokenUrl { get { return GoogleLoginProvider.Instance.AccessTokenUrl; } }
        public string RedirectUri { get { return this["googleDriveAppRedirectUrl"]; } }
        public string ClientID { get { return this["googleDriveAppClientId"]; } }
        public string ClientSecret { get { return this["googleDriveAppSecretKey"]; } }

        public bool IsEnabled
        {
            get
            {
                return !string.IsNullOrEmpty(ClientID) &&
                       !string.IsNullOrEmpty(ClientSecret);
            }
        }

        public GoogleDriveApp() { }

        public GoogleDriveApp(string name, int order, Dictionary<string, string> additional)
            : base(name, order, additional)
        {
        }

        public bool Request(HttpContext context)
        {
            switch ((context.Request[FilesLinkUtility.Action] ?? "").ToLower())
            {
                case "stream":
                    StreamFile(context);
                    return true;
                case "convert":
                    ConfirmConvertFile(context);
                    return true;
                case "create":
                    CreateFile(context);
                    return true;
            }

            if (!string.IsNullOrEmpty(context.Request["code"]))
            {
                RequestCode(context);
                return true;
            }

            return false;
        }

        public string GetRefreshUrl()
        {
            return AccessTokenUrl;
        }

        public File GetFile(string fileId, out bool editable)
        {
            Global.Logger.Debug("GoogleDriveApp: get file " + fileId);
            fileId = ThirdPartySelector.GetFileId(fileId);

            var token = Token.GetToken(AppAttr);
            var driveFile = GetDriveFile(fileId, token);
            editable = false;

            if (driveFile == null) return null;

            var jsonFile = JObject.Parse(driveFile);

            var file = new File
                {
                    ID = ThirdPartySelector.BuildAppFileId(AppAttr, jsonFile.Value<string>("id")),
                    Title = Global.ReplaceInvalidCharsAndTruncate(GetCorrectTitle(jsonFile)),
                    CreateOn = TenantUtil.DateTimeFromUtc(jsonFile.Value<DateTime>("createdTime")),
                    ModifiedOn = TenantUtil.DateTimeFromUtc(jsonFile.Value<DateTime>("modifiedTime")),
                    ContentLength = Convert.ToInt64(jsonFile.Value<string>("size")),
                    ModifiedByString = jsonFile["lastModifyingUser"]["displayName"].Value<string>(),
                    ProviderKey = "Google"
                };

            var owners = jsonFile["owners"];
            if (owners != null)
            {
                file.CreateByString = owners[0]["displayName"].Value<string>();
            }

            editable = jsonFile["capabilities"]["canEdit"].Value<bool>();
            return file;
        }

        public string GetFileStreamUrl(File file)
        {
            if (file == null) return string.Empty;

            var fileId = ThirdPartySelector.GetFileId(file.ID.ToString());
            return GetFileStreamUrl(fileId);
        }

        private static string GetFileStreamUrl(string fileId)
        {
            Global.Logger.Debug("GoogleDriveApp: get file stream url " + fileId);

            var uriBuilder = new UriBuilder(CommonLinkUtility.GetFullAbsolutePath(ThirdPartyAppHandler.HandlerPath));
            if (uriBuilder.Uri.IsLoopback)
            {
                uriBuilder.Host = Dns.GetHostName();
            }
            var query = uriBuilder.Query;
            query += FilesLinkUtility.Action + "=stream&";
            query += FilesLinkUtility.FileId + "=" + HttpUtility.UrlEncode(fileId) + "&";
            query += CommonLinkUtility.ParamName_UserUserID + "=" + HttpUtility.UrlEncode(SecurityContext.CurrentAccount.ID.ToString()) + "&";
            query += FilesLinkUtility.AuthKey + "=" + EmailValidationKeyProvider.GetEmailKey(fileId + SecurityContext.CurrentAccount.ID) + "&";
            query += ThirdPartySelector.AppAttr + "=" + AppAttr;

            return uriBuilder.Uri + "?" + query;
        }

        public void SaveFile(string fileId, string fileType, string downloadUrl, Stream stream)
        {
            Global.Logger.Debug("GoogleDriveApp: save file stream " + fileId +
                                (stream == null
                                     ? " from - " + downloadUrl
                                     : " from stream"));
            fileId = ThirdPartySelector.GetFileId(fileId);

            var token = Token.GetToken(AppAttr);

            var driveFile = GetDriveFile(fileId, token);
            if (driveFile == null)
            {
                Global.Logger.Error("GoogleDriveApp: file is null");
                throw new Exception("File not found");
            }

            var jsonFile = JObject.Parse(driveFile);
            var currentType = GetCorrectExt(jsonFile);
            if (!fileType.Equals(currentType))
            {
                try
                {
                    if (stream != null)
                    {
                        downloadUrl = PathProvider.GetTempUrl(stream, fileType);
                        downloadUrl = DocumentServiceConnector.ReplaceCommunityAdress(downloadUrl);
                    }

                    Global.Logger.Debug("GoogleDriveApp: GetConvertedUri from " + fileType + " to " + currentType + " - " + downloadUrl);

                    var key = DocumentServiceConnector.GenerateRevisionId(downloadUrl);
                    DocumentServiceConnector.GetConvertedUri(downloadUrl, fileType, currentType, key, null, false, out downloadUrl);
                    stream = null;
                }
                catch (Exception e)
                {
                    Global.Logger.Error("GoogleDriveApp: Error convert", e);
                }
            }

            var request = (HttpWebRequest) WebRequest.Create(GoogleLoginProvider.GoogleUrlFileUpload + "/{fileId}?uploadType=media".Replace("{fileId}", fileId));
            request.Method = "PATCH";
            request.Headers.Add("Authorization", "Bearer " + token);
            request.ContentType = MimeMapping.GetMimeMapping(currentType);

            if (stream != null)
            {
                request.ContentLength = stream.Length;

                const int bufferSize = 2048;
                var buffer = new byte[bufferSize];
                int readed;
                while ((readed = stream.Read(buffer, 0, bufferSize)) > 0)
                {
                    request.GetRequestStream().Write(buffer, 0, readed);
                }
            }
            else
            {
                var downloadRequest = (HttpWebRequest) WebRequest.Create(downloadUrl);
                using (var downloadStream = new ResponseStream(downloadRequest.GetResponse()))
                {
                    request.ContentLength = downloadStream.Length;

                    const int bufferSize = 2048;
                    var buffer = new byte[bufferSize];
                    int readed;
                    while ((readed = downloadStream.Read(buffer, 0, bufferSize)) > 0)
                    {
                        request.GetRequestStream().Write(buffer, 0, readed);
                    }
                }
            }

            try
            {
                using (var response = request.GetResponse())
                using (var responseStream = response.GetResponseStream())
                {
                    string result = null;
                    if (responseStream != null)
                    {
                        using (var readStream = new StreamReader(responseStream))
                        {
                            result = readStream.ReadToEnd();
                        }
                    }

                    Global.Logger.Debug("GoogleDriveApp: save file stream response - " + result);
                }
            }
            catch (WebException e)
            {
                Global.Logger.Error("GoogleDriveApp: Error save file stream", e);
                request.Abort();
                var httpResponse = (HttpWebResponse) e.Response;
                if (httpResponse.StatusCode == HttpStatusCode.Forbidden || httpResponse.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException, e);
                }
                throw;
            }
        }


        private static void RequestCode(HttpContext context)
        {
            var state = context.Request["state"];
            Global.Logger.Debug("GoogleDriveApp: state - " + state);
            if (string.IsNullOrEmpty(state))
            {
                Global.Logger.Error("GoogleDriveApp: empty state");
                throw new Exception("Empty state");
            }

            var token = GetToken(context.Request["code"]);
            if (token == null)
            {
                Global.Logger.Error("GoogleDriveApp: token is null");
                throw new SecurityException("Access token is null");
            }

            var stateJson = JObject.Parse(state);

            var googleUserId = stateJson.Value<string>("userId");

            if (SecurityContext.IsAuthenticated)
            {
                if (!CurrentUser(googleUserId))
                {
                    Global.Logger.Debug("GoogleDriveApp: logout for " + googleUserId);
                    CookiesManager.ClearCookies(CookiesType.AuthKey);
                    SecurityContext.Logout();
                }
            }

            if (!SecurityContext.IsAuthenticated)
            {
                bool isNew;
                var userInfo = GetUserInfo(token, out isNew);

                if (userInfo == null)
                {
                    Global.Logger.Error("GoogleDriveApp: UserInfo is null");
                    throw new Exception("Profile is null");
                }

                var cookiesKey = SecurityContext.AuthenticateMe(userInfo.ID);
                CookiesManager.SetCookies(CookiesType.AuthKey, cookiesKey);
                MessageService.Send(HttpContext.Current.Request, MessageAction.LoginSuccessViaSocialApp);

                if (isNew)
                {
                    UserHelpTourHelper.IsNewUser = true;
                    PersonalSettings.IsNewUser = true;
                    PersonalSettings.IsNotActivated = true;
                }

                if (!string.IsNullOrEmpty(googleUserId) && !CurrentUser(googleUserId))
                {
                    AddLinker(googleUserId);
                }
            }

            Token.SaveToken(token);

            var action = stateJson.Value<string>("action");
            switch (action)
            {
                case "create":
                    var folderId = stateJson.Value<string>("folderId");

                    context.Response.Redirect(App.Location + "?" + FilesLinkUtility.FolderId + "=" + HttpUtility.UrlEncode(folderId), true);
                    return;
                case "open":
                    var idsArray = stateJson.Value<JArray>("ids") ?? stateJson.Value<JArray>("exportIds");
                    if (idsArray == null)
                    {
                        Global.Logger.Error("GoogleDriveApp: ids is empty");
                        throw new Exception("File id is null");
                    }
                    var fileId = idsArray.ToObject<List<string>>().FirstOrDefault();

                    var driveFile = GetDriveFile(fileId, token);
                    if (driveFile == null)
                    {
                        Global.Logger.Error("GoogleDriveApp: file is null");
                        throw new Exception("File not found");
                    }

                    var jsonFile = JObject.Parse(driveFile);
                    var ext = GetCorrectExt(jsonFile);
                    if (FileUtility.ExtsMustConvert.Contains(ext)
                        || GoogleLoginProvider.GoogleDriveExt.Contains(ext))
                    {
                        Global.Logger.Debug("GoogleDriveApp: file must be converted");
                        if (FilesSettings.ConvertNotify)
                        {
                            context.Response.Redirect(App.Location + "?" + FilesLinkUtility.FileId + "=" + HttpUtility.UrlEncode(fileId), true);
                            return;
                        }

                        fileId = CreateConvertedFile(driveFile, token);
                    }

                    context.Response.Redirect(FilesLinkUtility.GetFileWebEditorUrl(ThirdPartySelector.BuildAppFileId(AppAttr, fileId)), true);
                    return;
            }
            Global.Logger.Error("GoogleDriveApp: Action not identified");
            throw new Exception("Action not identified");
        }

        private static void StreamFile(HttpContext context)
        {
            try
            {
                var fileId = context.Request[FilesLinkUtility.FileId];
                var auth = context.Request[FilesLinkUtility.AuthKey];
                var userId = context.Request[CommonLinkUtility.ParamName_UserUserID];

                Global.Logger.Debug("GoogleDriveApp: get file stream " + fileId);

                var validateResult = EmailValidationKeyProvider.ValidateEmailKey(fileId + userId, auth, Global.StreamUrlExpire);
                if (validateResult != EmailValidationKeyProvider.ValidationResult.Ok)
                {
                    var exc = new HttpException((int)HttpStatusCode.Forbidden, FilesCommonResource.ErrorMassage_SecurityException);

                    Global.Logger.Error(string.Format("GoogleDriveApp: validate error {0} {1}: {2}", FilesLinkUtility.AuthKey, validateResult, context.Request.Url), exc);

                    throw exc;
                }

                var token = Token.GetToken(AppAttr, userId);
                var driveFile = GetDriveFile(fileId, token);

                var jsonFile = JObject.Parse(driveFile);

                var downloadUrl = GoogleLoginProvider.GoogleUrlFile + fileId + "?alt=media";

                if (string.IsNullOrEmpty(downloadUrl))
                {
                    Global.Logger.Error("GoogleDriveApp: downloadUrl is null");
                    throw new Exception("downloadUrl is null");
                }

                Global.Logger.Debug("GoogleDriveApp: get file stream downloadUrl - " + downloadUrl);

                var request = (HttpWebRequest)WebRequest.Create(downloadUrl);
                request.Method = "GET";
                request.Headers.Add("Authorization", "Bearer " + token);

                using (var response = request.GetResponse())
                using (var stream = new ResponseStream(response))
                {
                    stream.StreamCopyTo(context.Response.OutputStream);

                    var contentLength = jsonFile.Value<string>("size");
                    Global.Logger.Debug("GoogleDriveApp: get file stream contentLength - " + contentLength);
                    context.Response.AddHeader("Content-Length", contentLength);
                }
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.Write(ex.Message);
                Global.Logger.Error("GoogleDriveApp: Error request " + context.Request.Url, ex);
            }
            try
            {
                context.Response.Flush();
                context.Response.SuppressContent = true;
                context.ApplicationInstance.CompleteRequest();
            }
            catch (HttpException ex)
            {
                Global.Logger.Error("GoogleDriveApp StreamFile", ex);
            }
        }

        private static void ConfirmConvertFile(HttpContext context)
        {
            var fileId = context.Request[FilesLinkUtility.FileId];
            Global.Logger.Debug("GoogleDriveApp: ConfirmConvertFile - " + fileId);

            var token = Token.GetToken(AppAttr);

            var driveFile = GetDriveFile(fileId, token);
            if (driveFile == null)
            {
                Global.Logger.Error("GoogleDriveApp: file is null");
                throw new Exception("File not found");
            }

            fileId = CreateConvertedFile(driveFile, token);

            context.Response.Redirect(FilesLinkUtility.GetFileWebEditorUrl(ThirdPartySelector.BuildAppFileId(AppAttr, fileId)), true);
        }

        private static void CreateFile(HttpContext context)
        {
            var folderId = context.Request[FilesLinkUtility.FolderId];
            var fileName = context.Request[FilesLinkUtility.FileTitle];
            Global.Logger.Debug("GoogleDriveApp: CreateFile folderId - " + folderId + " fileName - " + fileName);

            var token = Token.GetToken(AppAttr);

            var culture = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).GetCulture();
            var storeTemplate = Global.GetStoreTemplate();

            var path = FileConstant.NewDocPath + culture + "/";
            if (!storeTemplate.IsDirectory(path))
            {
                path = FileConstant.NewDocPath + "default/";
            }
            var ext = FileUtility.InternalExtension[FileUtility.GetFileTypeByFileName(fileName)];
            path += "new" + ext;
            fileName = FileUtility.ReplaceFileExtension(fileName, ext);

            string driveFile;
            using (var content = storeTemplate.GetReadStream("", path))
            {
                driveFile = CreateFile(content, fileName, folderId, token);
            }
            if (driveFile == null)
            {
                Global.Logger.Error("GoogleDriveApp: file is null");
                throw new Exception("File not created");
            }

            var jsonFile = JObject.Parse(driveFile);
            var fileId = jsonFile.Value<string>("id");

            context.Response.Redirect(FilesLinkUtility.GetFileWebEditorUrl(ThirdPartySelector.BuildAppFileId(AppAttr, fileId)), true);
        }

        private static Token GetToken(string code)
        {
            try
            {
                Global.Logger.Debug("GoogleDriveApp: GetAccessToken by code " + code);
                var token = OAuth20TokenHelper.GetAccessToken<GoogleDriveApp>(code);
                return new Token(token, AppAttr);
            }
            catch (Exception ex)
            {
                Global.Logger.Error(ex);
            }
            return null;
        }

        private static bool CurrentUser(string googleId)
        {
            var linker = new AccountLinker("webstudio");
            var linkedProfiles = linker.GetLinkedObjectsByHashId(HashHelper.MD5(string.Format("{0}/{1}", ProviderConstants.Google, googleId)));
            linkedProfiles = linkedProfiles.Concat(linker.GetLinkedObjectsByHashId(HashHelper.MD5(string.Format("{0}/{1}", ProviderConstants.OpenId, googleId))));
            Guid tmp;
            return
                linkedProfiles.Any(profileId => Guid.TryParse(profileId, out tmp) && tmp == SecurityContext.CurrentAccount.ID);
        }

        private static void AddLinker(string googleUserId)
        {
            Global.Logger.Debug("GoogleDriveApp: AddLinker " + googleUserId);
            var linker = new AccountLinker("webstudio");
            linker.AddLink(SecurityContext.CurrentAccount.ID.ToString(), googleUserId, ProviderConstants.Google);
        }

        private static UserInfo GetUserInfo(Token token, out bool isNew)
        {
            isNew = false;
            if (token == null)
            {
                Global.Logger.Error("GoogleDriveApp: token is null");
                throw new SecurityException("Access token is null");
            }

            LoginProfile loginProfile = null;
            try
            {
                loginProfile = GoogleLoginProvider.Instance.GetLoginProfile(token.ToString());
            }
            catch (Exception ex)
            {
                Global.Logger.Error("GoogleDriveApp: userinfo request", ex);
            }

            if (loginProfile == null)
            {
                Global.Logger.Error("Error in userinfo request");
                return null;
            }

            var userInfo = CoreContext.UserManager.GetUserByEmail(loginProfile.EMail);
            if (Equals(userInfo, Constants.LostUser))
            {
                userInfo = LoginWithThirdParty.ProfileToUserInfo(loginProfile);

                var cultureName = loginProfile.Locale;
                if (string.IsNullOrEmpty(cultureName))
                    cultureName = Thread.CurrentThread.CurrentUICulture.Name;

                var cultureInfo = SetupInfo.EnabledCultures.Find(c => String.Equals(c.Name, cultureName, StringComparison.InvariantCultureIgnoreCase));
                if (cultureInfo != null)
                {
                    userInfo.CultureName = cultureInfo.Name;
                }
                else
                {
                    Global.Logger.DebugFormat("From google app new personal user '{0}' without culture {1}", userInfo.Email, cultureName);
                }

                try
                {
                    SecurityContext.AuthenticateMe(ASC.Core.Configuration.Constants.CoreSystem);
                    userInfo = UserManagerWrapper.AddUser(userInfo, UserManagerWrapper.GeneratePassword());
                }
                finally
                {
                    SecurityContext.Logout();
                }

                isNew = true;

                Global.Logger.Debug("GoogleDriveApp: new user " + userInfo.ID);
            }

            return userInfo;
        }

        private static string GetDriveFile(string googleFileId, Token token)
        {
            if (token == null)
            {
                Global.Logger.Error("GoogleDriveApp: token is null");
                throw new SecurityException("Access token is null");
            }
            try
            {
                var requestUrl = GoogleLoginProvider.GoogleUrlFile + googleFileId + "?fields=" + HttpUtility.UrlEncode(GoogleLoginProvider.FilesFields);
                var resultResponse = RequestHelper.PerformRequest(requestUrl,
                                                                  headers: new Dictionary<string, string> {{"Authorization", "Bearer " + token}});
                Global.Logger.Debug("GoogleDriveApp: file response - " + resultResponse);
                return resultResponse;
            }
            catch (Exception ex)
            {
                Global.Logger.Error("GoogleDriveApp: file request", ex);
            }
            return null;
        }

        private static string CreateFile(string contentUrl, string fileName, string folderId, Token token)
        {
            if (string.IsNullOrEmpty(contentUrl))
            {
                Global.Logger.Error("GoogleDriveApp: downloadUrl is null");
                throw new Exception("downloadUrl is null");
            }
            Global.Logger.Debug("GoogleDriveApp: create from - " + contentUrl);

            var request = (HttpWebRequest)WebRequest.Create(contentUrl);

            using (var content = new ResponseStream(request.GetResponse()))
            {
                return CreateFile(content, fileName, folderId, token);
            }
        }

        private static string CreateFile(Stream content, string fileName, string folderId, Token token)
        {
            Global.Logger.Debug("GoogleDriveApp: create file");

            var request = (HttpWebRequest)WebRequest.Create(GoogleLoginProvider.GoogleUrlFileUpload + "?uploadType=multipart");

            using (var tmpStream = new MemoryStream())
            {
                var boundary = DateTime.UtcNow.Ticks.ToString("x");

                var folderdata = string.IsNullOrEmpty(folderId) ? "" : string.Format(",\"parents\":[\"{0}\"]", folderId);
                var metadata = string.Format("{{\"name\":\"{0}\"{1}}}", fileName, folderdata);
                var metadataPart = string.Format("\r\n--{0}\r\nContent-Type: application/json; charset=UTF-8\r\n\r\n{1}", boundary, metadata);
                var bytes = Encoding.UTF8.GetBytes(metadataPart);
                tmpStream.Write(bytes, 0, bytes.Length);

                var mediaPartStart = string.Format("\r\n--{0}\r\nContent-Type: {1}\r\n\r\n", boundary, MimeMapping.GetMimeMapping(fileName));
                bytes = Encoding.UTF8.GetBytes(mediaPartStart);
                tmpStream.Write(bytes, 0, bytes.Length);

                content.CopyTo(tmpStream);

                var mediaPartEnd = string.Format("\r\n--{0}--\r\n", boundary);
                bytes = Encoding.UTF8.GetBytes(mediaPartEnd);
                tmpStream.Write(bytes, 0, bytes.Length);

                request.Method = "POST";
                request.Headers.Add("Authorization", "Bearer " + token);
                request.ContentType = "multipart/related; boundary=" + boundary;
                request.ContentLength = tmpStream.Length;
                Global.Logger.Debug("GoogleDriveApp: create file totalSize - " + tmpStream.Length);

                const int bufferSize = 2048;
                var buffer = new byte[bufferSize];
                int readed;
                tmpStream.Seek(0, SeekOrigin.Begin);
                while ((readed = tmpStream.Read(buffer, 0, bufferSize)) > 0)
                {
                    request.GetRequestStream().Write(buffer, 0, readed);
                }
            }

            try
            {
                using (var response = request.GetResponse())
                using (var responseStream = response.GetResponseStream())
                {
                    string result = null;
                    if (responseStream != null)
                    {
                        using (var readStream = new StreamReader(responseStream))
                        {
                            result = readStream.ReadToEnd();
                        }
                    }

                    Global.Logger.Debug("GoogleDriveApp: create file response - " + result);
                    return result;
                }
            }
            catch (WebException e)
            {
                Global.Logger.Error("GoogleDriveApp: Error create file", e);
                request.Abort();

                var httpResponse = (HttpWebResponse)e.Response;
                if (httpResponse.StatusCode == HttpStatusCode.Forbidden || httpResponse.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException, e);
                }
            }
            return null;
        }

        private static string ConvertFile(string fileId, string fromExt)
        {
            Global.Logger.Debug("GoogleDriveApp: convert file");

            var downloadUrl = GetFileStreamUrl(fileId);

            var toExt = FileUtility.GetInternalExtension(fromExt);
            try
            {
                Global.Logger.Debug("GoogleDriveApp: GetConvertedUri- " + downloadUrl);

                var key = DocumentServiceConnector.GenerateRevisionId(downloadUrl);
                DocumentServiceConnector.GetConvertedUri(downloadUrl, fromExt, toExt, key, null, false, out downloadUrl);
            }
            catch (Exception e)
            {
                Global.Logger.Error("GoogleDriveApp: Error GetConvertedUri", e);
            }

            return downloadUrl;
        }

        private static string CreateConvertedFile(string driveFile, Token token)
        {
            var jsonFile = JObject.Parse(driveFile);
            var fileName = GetCorrectTitle(jsonFile);

            var folderId = (string)jsonFile.SelectToken("parents[0]");

            Global.Logger.Info("GoogleDriveApp: create copy - " + fileName);

            var ext = GetCorrectExt(jsonFile);
            var fileId = jsonFile.Value<string>("id");

            if (GoogleLoginProvider.GoogleDriveExt.Contains(ext))
            {
                var internalExt = FileUtility.GetGoogleDownloadableExtension(ext);
                fileName = FileUtility.ReplaceFileExtension(fileName, internalExt);
                var requiredMimeType = MimeMapping.GetMimeMapping(internalExt);

                var downloadUrl = GoogleLoginProvider.GoogleUrlFile
                                  + string.Format("{0}/export?mimeType={1}",
                                                  fileId,
                                                  HttpUtility.UrlEncode(requiredMimeType));

                var request = (HttpWebRequest)WebRequest.Create(downloadUrl);
                request.Method = "GET";
                request.Headers.Add("Authorization", "Bearer " + token);

                Global.Logger.Debug("GoogleDriveApp: download exportLink - " + downloadUrl);
                try
                {
                    using (var fileStream = new ResponseStream(request.GetResponse()))
                    {
                        driveFile = CreateFile(fileStream, fileName, folderId, token);
                    }
                }
                catch (WebException e)
                {
                    Global.Logger.Error("GoogleDriveApp: Error download exportLink", e);
                    request.Abort();

                    var httpResponse = (HttpWebResponse)e.Response;
                    if (httpResponse.StatusCode == HttpStatusCode.Forbidden || httpResponse.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException, e);
                    }
                }
            }
            else
            {
                var convertedUrl = ConvertFile(fileId, ext);

                if (string.IsNullOrEmpty(convertedUrl))
                {
                    Global.Logger.ErrorFormat("GoogleDriveApp: Error convertUrl. size {0}", FileSizeComment.FilesSizeToString(jsonFile.Value<int>("size")));
                    throw new Exception(FilesCommonResource.ErrorMassage_DocServiceException + " (convert)");
                }

                var toExt = FileUtility.GetInternalExtension(fileName);
                fileName = FileUtility.ReplaceFileExtension(fileName, toExt);
                driveFile = CreateFile(convertedUrl, fileName, folderId, token);
            }

            jsonFile = JObject.Parse(driveFile);
            return jsonFile.Value<string>("id");
        }


        private static string GetCorrectTitle(JToken jsonFile)
        {
            var title = jsonFile.Value<string>("name") ?? "";
            var extTitle = FileUtility.GetFileExtension(title);
            var correctExt = GetCorrectExt(jsonFile);

            if (extTitle != correctExt)
            {
                title = title + correctExt;
            }
            return title;
        }

        private static string GetCorrectExt(JToken jsonFile)
        {
            var mimeType = (jsonFile.Value<string>("mimeType") ?? "").ToLower();

            var ext = MimeMapping.GetExtention(mimeType);
            if (!GoogleLoginProvider.GoogleDriveExt.Contains(ext))
            {
                var title = (jsonFile.Value<string>("name") ?? "").ToLower();
                ext = FileUtility.GetFileExtension(title);

                if (MimeMapping.GetMimeMapping(ext) != mimeType)
                {
                    var originalFilename = (jsonFile.Value<string>("originalFilename") ?? "").ToLower();
                    ext = FileUtility.GetFileExtension(originalFilename);

                    if (MimeMapping.GetMimeMapping(ext) != mimeType)
                    {
                        ext = MimeMapping.GetExtention(mimeType);

                        Global.Logger.Debug("GoogleDriveApp: Try GetCorrectExt - " + ext + " for - " + mimeType);
                    }
                }
            }
            return ext;
        }
    }
}