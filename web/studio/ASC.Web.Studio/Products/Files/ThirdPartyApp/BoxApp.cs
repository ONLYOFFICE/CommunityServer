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
using ASC.Web.Studio.Utility;
using Newtonsoft.Json.Linq;
using File = ASC.Files.Core.File;
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Web.Files.ThirdPartyApp
{
    public class BoxApp : Consumer, IThirdPartyApp, IOAuthProvider
    {
        public const string AppAttr = "box";

        private const string BoxUrlUserInfo = "https://api.box.com/2.0/users/me";
        private const string BoxUrlFile = "https://api.box.com/2.0/files/{fileId}";
        private const string BoxUrlUpload = "https://upload.box.com/api/2.0/files/{fileId}/content";

        public string Scopes { get { return ""; } }
        public string CodeUrl { get { return ""; } }
        public string AccessTokenUrl { get { return "https://www.box.com/api/oauth2/token"; } }
        public string RedirectUri { get { return ""; } }
        public string ClientID { get { return this["boxAppClientId"]; } }
        public string ClientSecret { get { return this["boxAppSecretKey"]; } }

        public bool IsEnabled
        {
            get { return !string.IsNullOrEmpty(ClientID) && !string.IsNullOrEmpty(ClientSecret); }
        }

        public BoxApp() { }

        public BoxApp(string name, int order, Dictionary<string, string> additional)
            : base(name, order, additional)
        {
        }

        public bool Request(HttpContext context)
        {
            if ((context.Request[FilesLinkUtility.Action] ?? "").Equals("stream", StringComparison.InvariantCultureIgnoreCase))
            {
                StreamFile(context);
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
            Global.Logger.Debug("BoxApp: get file " + fileId);
            fileId = ThirdPartySelector.GetFileId(fileId);

            var token = Token.GetToken(AppAttr);

            var boxFile = GetBoxFile(fileId, token);
            editable = true;

            if (boxFile == null) return null;

            var jsonFile = JObject.Parse(boxFile);

            var file = new File
                {
                    ID = ThirdPartySelector.BuildAppFileId(AppAttr, jsonFile.Value<string>("id")),
                    Title = Global.ReplaceInvalidCharsAndTruncate(jsonFile.Value<string>("name")),
                    CreateOn = TenantUtil.DateTimeFromUtc(jsonFile.Value<DateTime>("created_at")),
                    ModifiedOn = TenantUtil.DateTimeFromUtc(jsonFile.Value<DateTime>("modified_at")),
                    ContentLength = Convert.ToInt64(jsonFile.Value<string>("size")),
                    ProviderKey = "Box"
                };

            var modifiedBy = jsonFile.Value<JObject>("modified_by");
            if (modifiedBy != null)
            {
                file.ModifiedByString = modifiedBy.Value<string>("name");
            }

            var createdBy = jsonFile.Value<JObject>("created_by");
            if (createdBy != null)
            {
                file.CreateByString = createdBy.Value<string>("name");
            }


            var locked = jsonFile.Value<JObject>("lock");
            if (locked != null)
            {
                var lockedBy = locked.Value<JObject>("created_by");
                if (lockedBy != null)
                {
                    var lockedUserId = lockedBy.Value<string>("id");
                    Global.Logger.Debug("BoxApp: locked by " + lockedUserId);

                    editable = CurrentUser(lockedUserId);
                }
            }

            return file;
        }

        public string GetFileStreamUrl(File file)
        {
            if (file == null) return string.Empty;

            var fileId = ThirdPartySelector.GetFileId(file.ID.ToString());

            Global.Logger.Debug("BoxApp: get file stream url " + fileId);

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
            Global.Logger.Debug("BoxApp: save file stream " + fileId +
                                (stream == null
                                     ? " from - " + downloadUrl
                                     : " from stream"));
            fileId = ThirdPartySelector.GetFileId(fileId);

            var token = Token.GetToken(AppAttr);

            var boxFile = GetBoxFile(fileId, token);
            if (boxFile == null)
            {
                Global.Logger.Error("BoxApp: file is null");
                throw new Exception("File not found");
            }

            var jsonFile = JObject.Parse(boxFile);
            var title = Global.ReplaceInvalidCharsAndTruncate(jsonFile.Value<string>("name"));
            var currentType = FileUtility.GetFileExtension(title);
            if (!fileType.Equals(currentType))
            {
                try
                {
                    if (stream != null)
                    {
                        downloadUrl = PathProvider.GetTempUrl(stream, fileType);
                        downloadUrl = DocumentServiceConnector.ReplaceCommunityAdress(downloadUrl);
                    }

                    Global.Logger.Debug("BoxApp: GetConvertedUri from " + fileType + " to " + currentType + " - " + downloadUrl);

                    var key = DocumentServiceConnector.GenerateRevisionId(downloadUrl);
                    DocumentServiceConnector.GetConvertedUri(downloadUrl, fileType, currentType, key, null, false, out downloadUrl);
                    stream = null;
                }
                catch (Exception e)
                {
                    Global.Logger.Error("BoxApp: Error convert", e);
                }
            }

            var request = (HttpWebRequest)WebRequest.Create(BoxUrlUpload.Replace("{fileId}", fileId));

            using (var tmpStream = new MemoryStream())
            {
                var boundary = DateTime.UtcNow.Ticks.ToString("x");

                var metadata = string.Format("Content-Disposition: form-data; name=\"filename\"; filename=\"{0}\"\r\nContent-Type: application/octet-stream\r\n\r\n", title);
                var metadataPart = string.Format("--{0}\r\n{1}", boundary, metadata);
                var bytes = Encoding.UTF8.GetBytes(metadataPart);
                tmpStream.Write(bytes, 0, bytes.Length);

                if (stream != null)
                {
                    stream.CopyTo(tmpStream);
                }
                else
                {
                    var downloadRequest = (HttpWebRequest) WebRequest.Create(downloadUrl);
                    using (var downloadStream = new ResponseStream(downloadRequest.GetResponse()))
                    {
                        downloadStream.CopyTo(tmpStream);
                    }
                }

                var mediaPartEnd = string.Format("\r\n--{0}--\r\n", boundary);
                bytes = Encoding.UTF8.GetBytes(mediaPartEnd);
                tmpStream.Write(bytes, 0, bytes.Length);

                request.Method = "POST";
                request.Headers.Add("Authorization", "Bearer " + token);
                request.ContentType = "multipart/form-data; boundary=" + boundary;
                request.ContentLength = tmpStream.Length;
                Global.Logger.Debug("BoxApp: save file totalSize - " + tmpStream.Length);

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

                    Global.Logger.Debug("BoxApp: save file response - " + result);
                }
            }
            catch (WebException e)
            {
                Global.Logger.Error("BoxApp: Error save file", e);
                request.Abort();
                var httpResponse = (HttpWebResponse)e.Response;
                if (httpResponse.StatusCode == HttpStatusCode.Forbidden || httpResponse.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException, e);
                }
                throw;
            }
        }


        private void RequestCode(HttpContext context)
        {
            var token = GetToken(context.Request["code"]);
            if (token == null)
            {
                Global.Logger.Error("BoxApp: token is null");
                throw new SecurityException("Access token is null");
            }

            var boxUserId = context.Request["userId"];

            if (SecurityContext.IsAuthenticated)
            {
                if (!CurrentUser(boxUserId))
                {
                    Global.Logger.Debug("BoxApp: logout for " + boxUserId);
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
                    Global.Logger.Error("BoxApp: UserInfo is null");
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

                if (!string.IsNullOrEmpty(boxUserId) && !CurrentUser(boxUserId))
                {
                    AddLinker(boxUserId);
                }
            }

            Token.SaveToken(token);

            var fileId = context.Request["id"];

            context.Response.Redirect(FilesLinkUtility.GetFileWebEditorUrl(ThirdPartySelector.BuildAppFileId(AppAttr, fileId)), true);
        }

        private static void StreamFile(HttpContext context)
        {
            try
            {
                var fileId = context.Request[FilesLinkUtility.FileId];
                var auth = context.Request[FilesLinkUtility.AuthKey];
                var userId = context.Request[CommonLinkUtility.ParamName_UserUserID];

                Global.Logger.Debug("BoxApp: get file stream " + fileId);

                var validateResult = EmailValidationKeyProvider.ValidateEmailKey(fileId + userId, auth, Global.StreamUrlExpire);
                if (validateResult != EmailValidationKeyProvider.ValidationResult.Ok)
                {
                    var exc = new HttpException((int)HttpStatusCode.Forbidden, FilesCommonResource.ErrorMassage_SecurityException);

                    Global.Logger.Error(string.Format("BoxApp: validate error {0} {1}: {2}", FilesLinkUtility.AuthKey, validateResult, context.Request.Url), exc);

                    throw exc;
                }

                var token = Token.GetToken(AppAttr, userId);
                if (token == null)
                {
                    Global.Logger.Error("BoxApp: token is null");
                    throw new SecurityException("Access token is null");
                }

                var request = (HttpWebRequest)WebRequest.Create(BoxUrlFile.Replace("{fileId}", fileId) + "/content");
                request.Method = "GET";
                request.Headers.Add("Authorization", "Bearer " + token);

                using (var stream = new ResponseStream(request.GetResponse()))
                {
                    stream.StreamCopyTo(context.Response.OutputStream);
                }
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.Write(ex.Message);
                Global.Logger.Error("BoxApp: Error request " + context.Request.Url, ex);
            }

            try
            {
                context.Response.Flush();
                context.Response.SuppressContent = true;
                context.ApplicationInstance.CompleteRequest();
            }
            catch (HttpException ex)
            {
                Global.Logger.Error("BoxApp StreamFile", ex);
            }
        }

        private static bool CurrentUser(string boxUserId)
        {
            var linkedProfiles = new AccountLinker("webstudio")
                .GetLinkedObjectsByHashId(HashHelper.MD5(string.Format("{0}/{1}", ProviderConstants.Box, boxUserId)));
            Guid tmp;
            return
                linkedProfiles.Any(profileId => Guid.TryParse(profileId, out tmp) && tmp == SecurityContext.CurrentAccount.ID);
        }

        private static void AddLinker(string boxUserId)
        {
            Global.Logger.Debug("BoxApp: AddLinker " + boxUserId);
            var linker = new AccountLinker("webstudio");
            linker.AddLink(SecurityContext.CurrentAccount.ID.ToString(), boxUserId, ProviderConstants.Box);
        }

        private static UserInfo GetUserInfo(Token token, out bool isNew)
        {
            isNew = false;
            if (token == null)
            {
                Global.Logger.Error("BoxApp: token is null");
                throw new SecurityException("Access token is null");
            }

            var resultResponse = string.Empty;
            try
            {
                resultResponse = RequestHelper.PerformRequest(BoxUrlUserInfo,
                                                              headers: new Dictionary<string, string> {{"Authorization", "Bearer " + token}});
                Global.Logger.Debug("BoxApp: userinfo response - " + resultResponse);
            }
            catch (Exception ex)
            {
                Global.Logger.Error("BoxApp: userinfo request", ex);
            }

            var boxUserInfo = JObject.Parse(resultResponse);
            if (boxUserInfo == null)
            {
                Global.Logger.Error("Error in userinfo request");
                return null;
            }

            var email = boxUserInfo.Value<string>("login");
            var userInfo = CoreContext.UserManager.GetUserByEmail(email);
            if (Equals(userInfo, Constants.LostUser))
            {
                userInfo = new UserInfo
                    {
                        FirstName = boxUserInfo.Value<string>("name"),
                        Email = email,
                        MobilePhone = boxUserInfo.Value<string>("phone"),
                    };

                var cultureName = boxUserInfo.Value<string>("language");
                if(string.IsNullOrEmpty(cultureName))
                    cultureName = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
                var cultureInfo = SetupInfo.EnabledCultures.Find(c => String.Equals(c.TwoLetterISOLanguageName, cultureName, StringComparison.InvariantCultureIgnoreCase));
                if (cultureInfo != null)
                {
                    userInfo.CultureName = cultureInfo.Name;
                }
                else
                {
                    Global.Logger.DebugFormat("From box app new personal user '{0}' without culture {1}", userInfo.Email, cultureName);
                }

                if (string.IsNullOrEmpty(userInfo.FirstName))
                {
                    userInfo.FirstName = FilesCommonResource.UnknownFirstName;
                }
                if (string.IsNullOrEmpty(userInfo.LastName))
                {
                    userInfo.LastName = FilesCommonResource.UnknownLastName;
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

                Global.Logger.Debug("BoxApp: new user " + userInfo.ID);
            }

            return userInfo;
        }

        private static string GetBoxFile(string boxFileId, Token token)
        {
            if (token == null)
            {
                Global.Logger.Error("BoxApp: token is null");
                throw new SecurityException("Access token is null");
            }

            try
            {
                var resultResponse = RequestHelper.PerformRequest(BoxUrlFile.Replace("{fileId}", boxFileId),
                                                                  headers: new Dictionary<string, string> {{"Authorization", "Bearer " + token}});
                Global.Logger.Debug("BoxApp: file response - " + resultResponse);
                return resultResponse;
            }
            catch (Exception ex)
            {
                Global.Logger.Error("BoxApp: file request", ex);
            }
            return null;
        }

        private Token GetToken(string code)
        {
            try
            {
                Global.Logger.Debug("BoxApp: GetAccessToken by code " + code);
                var token = OAuth20TokenHelper.GetAccessToken<BoxApp>(code);
                return new Token(token, AppAttr);
            }
            catch (Exception ex)
            {
                Global.Logger.Error(ex);
            }
            return null;
        }
    }
}