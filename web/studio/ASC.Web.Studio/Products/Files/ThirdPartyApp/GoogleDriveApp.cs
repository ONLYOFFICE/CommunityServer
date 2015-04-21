/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.FederatedLogin;
using ASC.FederatedLogin.Helpers;
using ASC.Files.Core;
using ASC.MessagingSystem;
using ASC.Security.Cryptography;
using ASC.Thrdparty;
using ASC.Thrdparty.Configuration;
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
using ASC.Web.Studio.Utility;
using Newtonsoft.Json.Linq;
using File = ASC.Files.Core.File;
using MimeMapping = ASC.Common.Web.MimeMapping;
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Web.Files.ThirdPartyApp
{
    public class GoogleDriveApp : IThirdPartyApp
    {
        public const string AppAttr = "gdrive";

        private const string GoogleUrlToken = "https://www.googleapis.com/oauth2/v3/token";
        private const string GoogleUrlUserInfo = "https://www.googleapis.com/oauth2/v1/userinfo?access_token={access_token}";
        private const string GoogleUrlFile = "https://www.googleapis.com/drive/v2/files/{fileId}";
        private const string GoogleUrlUpload = "https://www.googleapis.com/upload/drive/v2/files";

        private static readonly Dictionary<string, FileType> GoogleMimeTypes = new Dictionary<string, FileType>
            {
                { "application/vnd.google-apps.document", FileType.Document },
                { "application/vnd.google-apps.spreadsheet", FileType.Spreadsheet },
                { "application/vnd.google-apps.presentation", FileType.Presentation }
            };

        private static string ClientId
        {
            get { return KeyStorage.Get("googleDriveAppClientId"); }
        }

        private static string SecretKey
        {
            get { return KeyStorage.Get("googleDriveAppSecretKey"); }
        }

        private static string RedirectUrl
        {
            get { return KeyStorage.Get("googleDriveAppRedirectUrl"); }
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

        public Token RefreshToken(string refreshToken)
        {
            Global.Logger.Debug("GoogleDriveApp: refresh token");

            var query = String.Format("client_id={0}&client_secret={1}&refresh_token={2}&grant_type=refresh_token",
                                      HttpUtility.UrlEncode(ClientId),
                                      HttpUtility.UrlEncode(SecretKey),
                                      HttpUtility.UrlEncode(refreshToken));

            var jsonToken = RequestHelper.PerformRequest(GoogleUrlToken, "application/x-www-form-urlencoded", "POST", query);
            Global.Logger.Debug("GoogleDriveApp: refresh token response - " + jsonToken);
            var token = Token.FromJson(AppAttr, jsonToken);
            if (token != null)
            {
                Token.SaveToken(token);
            }
            return token;
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
                    CreateOn = TenantUtil.DateTimeFromUtc(jsonFile.Value<DateTime>("createdDate")),
                    ModifiedOn = TenantUtil.DateTimeFromUtc(jsonFile.Value<DateTime>("modifiedDate")),
                    ContentLength = Convert.ToInt64(jsonFile.Value<string>("fileSize")),
                    ModifiedByString = jsonFile.Value<string>("lastModifyingUserName"),
                    ProviderKey = "Google"
                };

            var owners = jsonFile.Value<JArray>("ownerNames");
            if (owners != null)
            {
                file.CreateByString = owners.ToObject<List<string>>().FirstOrDefault();
            }

            editable = jsonFile.Value<bool>("editable");
            return file;
        }

        public string GetFileStreamUrl(File file)
        {
            if (file == null) return string.Empty;

            var fileId = ThirdPartySelector.GetFileId(file.ID.ToString());

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

        public void SaveFile(string fileId, string downloadUrl)
        {
            Global.Logger.Debug("GoogleDriveApp: save file stream " + fileId + " from - " + downloadUrl);
            fileId = ThirdPartySelector.GetFileId(fileId);

            var token = Token.GetToken(AppAttr);

            var driveFile = GetDriveFile(fileId, token);
            if (driveFile == null)
            {
                Global.Logger.Error("GoogleDriveApp: file is null");
                throw new Exception("File not found");
            }

            var jsonFile = JObject.Parse(driveFile);
            var curExt = GetCorrectExt(jsonFile);
            var newExt = FileUtility.GetFileExtension(downloadUrl);
            if (curExt != newExt)
            {
                try
                {
                    Global.Logger.Debug("GoogleDriveApp: GetConvertedUri from " + newExt + " to " + curExt + " - " + downloadUrl);

                    var key = DocumentServiceConnector.GenerateRevisionId(downloadUrl);
                    DocumentServiceConnector.GetConvertedUri(downloadUrl, newExt, curExt, key, false, out downloadUrl);
                }
                catch (Exception e)
                {
                    Global.Logger.Error("GoogleDriveApp: Error convert", e);
                }
            }

            var downloadRequest = WebRequest.Create(downloadUrl);

            using (var downloadResponse = downloadRequest.GetResponse())
            using (var downloadStream = new ResponseStream(downloadResponse))
            {
                var request = (HttpWebRequest)WebRequest.Create(GoogleUrlUpload + "/{fileId}?uploadType=media".Replace("{fileId}", fileId));
                request.Method = "PUT";
                request.Headers.Add("Authorization", "Bearer " + token);
                request.ContentType = downloadResponse.ContentType;
                request.ContentLength = downloadResponse.ContentLength;

                const int bufferSize = 2048;
                var buffer = new byte[bufferSize];
                int readed;
                while ((readed = downloadStream.Read(buffer, 0, bufferSize)) > 0)
                {
                    request.GetRequestStream().Write(buffer, 0, readed);
                }

                try
                {
                    using (var response = request.GetResponse())
                    using (var stream = response.GetResponseStream())
                    {
                        var result = stream != null ? new StreamReader(stream).ReadToEnd() : null;

                        Global.Logger.Debug("GoogleDriveApp: save file stream response - " + result);
                    }
                }
                catch (WebException e)
                {
                    Global.Logger.Error("GoogleDriveApp: Error save file stream", e);
                    request.Abort();
                    var httpResponse = (HttpWebResponse)e.Response;
                    if (httpResponse.StatusCode == HttpStatusCode.Forbidden)
                    {
                        throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException, e);
                    }
                    throw;
                }
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
                MessageService.Send(HttpContext.Current.Request, MessageAction.LoginSuccessViaSocialAccount);

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

                    context.Response.Redirect(App.Location + "?" + FilesLinkUtility.FolderId + "=" + folderId, true);
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
                    var mimeType = (jsonFile.Value<string>("mimeType") ?? "").ToLower();
                    if (FileUtility.ExtsMustConvert.Contains(ext)
                        || GoogleMimeTypes.Keys.Contains(mimeType))
                    {
                        Global.Logger.Debug("GoogleDriveApp: file must be converted");
                        if (FilesSettings.ConvertNotify)
                        {
                            context.Response.Redirect(App.Location + "?" + FilesLinkUtility.FileId + "=" + fileId, true);
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

                int validateTimespan;
                int.TryParse(WebConfigurationManager.AppSettings["files.stream-url-minute"], out validateTimespan);
                if (validateTimespan <= 0) validateTimespan = 5;

                var validateResult = EmailValidationKeyProvider.ValidateEmailKey(fileId + userId, auth, TimeSpan.FromMinutes(validateTimespan));
                if (validateResult != EmailValidationKeyProvider.ValidationResult.Ok)
                {
                    var exc = new HttpException((int)HttpStatusCode.Forbidden, FilesCommonResource.ErrorMassage_SecurityException);

                    Global.Logger.Error(string.Format("GoogleDriveApp: validate error {0} {1}: {2}", FilesLinkUtility.AuthKey, validateResult, context.Request.Url), exc);

                    throw exc;
                }

                var token = Token.GetToken(AppAttr, userId);
                var driveFile = GetDriveFile(fileId, token);

                var jsonFile = JObject.Parse(driveFile);

                var downloadUrl = jsonFile.Value<string>("downloadUrl");
                var contentLength = jsonFile.Value<string>("fileSize");

                if (string.IsNullOrEmpty(downloadUrl))
                {
                    Global.Logger.Error("GoogleDriveApp: downloadUrl is null");
                    throw new Exception("downloadUrl is null");
                }

                Global.Logger.Debug("GoogleDriveApp: get file stream  downloadUrl - " + downloadUrl);

                var request = WebRequest.Create(downloadUrl);
                request.Method = "GET";
                request.Headers.Add("Authorization", "Bearer " + token);

                using (var response = request.GetResponse())
                using (var stream = new ResponseStream(response))
                {
                    stream.StreamCopyTo(context.Response.OutputStream);

                    Global.Logger.Debug("GoogleDriveApp: get file stream  contentLength - " + contentLength);
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
                context.Response.End();
            }
            catch (HttpException)
            {
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
            using (var content = storeTemplate.IronReadStream("", path, 10))
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

            context.Response.Redirect(FilesLinkUtility.GetFileWebEditorUrl(ThirdPartySelector.BuildAppFileId(AppAttr, fileId)) + "&new=true", true);
        }

        private static Token GetToken(string code)
        {
            var data = string.Format("code={0}&client_id={1}&client_secret={2}&redirect_uri={3}&grant_type=authorization_code",
                                     HttpUtility.UrlEncode(code),
                                     HttpUtility.UrlEncode(ClientId),
                                     HttpUtility.UrlEncode(SecretKey),
                                     RedirectUrl);

            try
            {
                var jsonToken = RequestHelper.PerformRequest(GoogleUrlToken, "application/x-www-form-urlencoded", "POST", data);
                Global.Logger.Debug("GoogleDriveApp: token response - " + jsonToken);

                return Token.FromJson(AppAttr, jsonToken);
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

            var resultResponse = string.Empty;
            try
            {
                resultResponse = RequestHelper.PerformRequest(GoogleUrlUserInfo.Replace("{access_token}", HttpUtility.UrlEncode(token.ToString())));
                Global.Logger.Debug("GoogleDriveApp: userinfo response - " + resultResponse);
            }
            catch (Exception ex)
            {
                Global.Logger.Error("GoogleDriveApp: userinfo request", ex);
            }

            var googleUserInfo = JObject.Parse(resultResponse);
            if (googleUserInfo == null)
            {
                Global.Logger.Error("Error in userinfo request");
                return null;
            }

            var email = googleUserInfo.Value<string>("email");
            var userInfo = CoreContext.UserManager.GetUserByEmail(email);
            if (Equals(userInfo, Constants.LostUser))
            {
                userInfo = new UserInfo
                    {
                        FirstName = googleUserInfo.Value<string>("given_name"),
                        LastName = googleUserInfo.Value<string>("family_name"),
                        Email = email,
                    };

                var gender = googleUserInfo.Value<string>("gender");
                if (!string.IsNullOrEmpty(gender))
                {
                    userInfo.Sex = gender == "male";
                }

                var cultureName = googleUserInfo.Value<string>("locale");
                if(string.IsNullOrEmpty(cultureName))
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

                if (string.IsNullOrEmpty(userInfo.FirstName))
                {
                    userInfo.FirstName = FilesCommonResource.UnknownFirstName;
                }
                if (string.IsNullOrEmpty(userInfo.LastName))
                {
                    userInfo.LastName = FilesCommonResource.UnknownLastName;
                }

                var pwd = UserManagerWrapper.GeneratePassword();

                try
                {
                    SecurityContext.AuthenticateMe(ASC.Core.Configuration.Constants.CoreSystem);
                    userInfo = UserManagerWrapper.AddUser(userInfo, pwd);
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
                var resultResponse = RequestHelper.PerformRequest(GoogleUrlFile.Replace("{fileId}", googleFileId),
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

            var request = WebRequest.Create(contentUrl);

            using (var response = request.GetResponse())
            using (var content = new ResponseStream(response))
            {
                return CreateFile(content, fileName, folderId, token);
            }
        }

        private static string CreateFile(Stream content, string fileName, string folderId, Token token)
        {
            Global.Logger.Debug("GoogleDriveApp: create file");

            var request = (HttpWebRequest)WebRequest.Create(GoogleUrlUpload + "?uploadType=multipart");

            using (var tmpStream = new MemoryStream())
            {
                var boundary = DateTime.UtcNow.Ticks.ToString("x");

                var folderdata = string.IsNullOrEmpty(folderId) ? "" : string.Format(",\"parents\":[{{\"id\":\"{0}\"}}]", folderId);
                var metadata = string.Format("{{\"title\":\"{0}\"{1}}}", fileName, folderdata);
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
                using (var stream = response.GetResponseStream())
                {
                    var result = stream != null ? new StreamReader(stream).ReadToEnd() : null;

                    Global.Logger.Debug("GoogleDriveApp: create file response - " + result);
                    return result;
                }
            }
            catch (WebException e)
            {
                Global.Logger.Error("GoogleDriveApp: Error create file", e);
                request.Abort();

                var httpResponse = (HttpWebResponse)e.Response;
                if (httpResponse.StatusCode == HttpStatusCode.Forbidden)
                {
                    throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException, e);
                }
            }
            return null;
        }

        private static string ConvertFile(string downloadUrl, string fromExt, Token token)
        {
            Global.Logger.Debug("GoogleDriveApp: convert file");

            if (string.IsNullOrEmpty(downloadUrl))
            {
                Global.Logger.Error("GoogleDriveApp: downloadUrl is null");
                throw new Exception("downloadUrl is null");
            }

            var request = WebRequest.Create(downloadUrl);
            request.Method = "GET";
            request.Headers.Add("Authorization", "Bearer " + token);

            try
            {
                using (var response = request.GetResponse())
                using (var fileStream = new ResponseStream(response))
                {
                    Global.Logger.Debug("GoogleDriveApp: GetExternalUri - " + downloadUrl);

                    var key = DocumentServiceConnector.GenerateRevisionId(downloadUrl);
                    downloadUrl = DocumentServiceConnector.GetExternalUri(fileStream, response.ContentType, key);
                }
            }
            catch (WebException e)
            {
                Global.Logger.Error("GoogleDriveApp: Error GetExternalUri", e);
                request.Abort();
            }

            var toExt = FileUtility.GetInternalExtension(fromExt);
            try
            {
                Global.Logger.Debug("GoogleDriveApp: GetConvertedUri- " + downloadUrl);

                var key = DocumentServiceConnector.GenerateRevisionId(downloadUrl);
                DocumentServiceConnector.GetConvertedUri(downloadUrl, fromExt, toExt, key, false, out downloadUrl);
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
            fileName = FileUtility.ReplaceFileExtension(fileName, FileUtility.GetInternalExtension(fileName));

            var folderId = (string)jsonFile.SelectToken("parents[0].id");

            Global.Logger.Info("GoogleDriveApp: create copy - " + fileName);

            var mimeType = (jsonFile.Value<string>("mimeType") ?? "").ToLower();
            FileType fileType;
            if (GoogleMimeTypes.TryGetValue(mimeType, out fileType))
            {
                var links = jsonFile["exportLinks"];
                if (links == null)
                {
                    Global.Logger.Error("GoogleDriveApp: exportLinks is null");
                    throw new Exception("exportLinks is null");
                }

                var internalExt = FileUtility.InternalExtension[fileType];
                var requiredMimeType = MimeMapping.GetMimeMapping(internalExt);

                var exportLinks = links.ToObject<Dictionary<string, string>>();
                var downloadUrl = exportLinks[requiredMimeType] ?? "";

                if (string.IsNullOrEmpty(downloadUrl))
                {
                    Global.Logger.Error("GoogleDriveApp: exportLinks without requested mime - " + links);
                    throw new Exception("exportLinks without requested mime");
                }


                var request = WebRequest.Create(downloadUrl);
                request.Method = "GET";
                request.Headers.Add("Authorization", "Bearer " + token);

                Global.Logger.Debug("GoogleDriveApp: download exportLink - " + downloadUrl);
                try
                {
                    using (var response = request.GetResponse())
                    using (var fileStream = new ResponseStream(response))
                    {
                        driveFile = CreateFile(fileStream, fileName, folderId, token);
                    }
                }
                catch (WebException e)
                {
                    Global.Logger.Error("GoogleDriveApp: Error download exportLink", e);
                    request.Abort();

                    var httpResponse = (HttpWebResponse)e.Response;
                    if (httpResponse.StatusCode == HttpStatusCode.Unauthorized && fileType == FileType.Spreadsheet)
                    {
                        throw new SecurityException(FilesCommonResource.AppDriveSpreadsheetException, e);
                    }
                }
            }
            else
            {
                var downloadUrl = jsonFile.Value<string>("downloadUrl");

                var ext = GetCorrectExt(jsonFile);
                var convertedUrl = ConvertFile(downloadUrl, ext, token);

                driveFile = CreateFile(convertedUrl, fileName, folderId, token);
            }

            jsonFile = JObject.Parse(driveFile);
            return jsonFile.Value<string>("id");
        }


        private static string GetCorrectTitle(JObject jsonFile)
        {
            var title = (jsonFile.Value<string>("title") ?? "").ToLower();
            var extTitle = FileUtility.GetFileExtension(title);
            var correctExt = GetCorrectExt(jsonFile);

            if (extTitle != correctExt)
            {
                title = title + correctExt;
            }
            return title;
        }

        private static string GetCorrectExt(JObject jsonFile)
        {
            string ext;
            var mimeType = (jsonFile.Value<string>("mimeType") ?? "").ToLower();

            FileType fileType;
            if (GoogleMimeTypes.TryGetValue(mimeType, out fileType))
            {
                ext = FileUtility.InternalExtension[fileType];
            }
            else
            {
                var title = (jsonFile.Value<string>("title") ?? "").ToLower();
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