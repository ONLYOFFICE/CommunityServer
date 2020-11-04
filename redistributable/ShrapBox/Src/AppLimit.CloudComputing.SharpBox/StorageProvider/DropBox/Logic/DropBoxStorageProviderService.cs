using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web;
using AppLimit.CloudComputing.SharpBox.Common.Extensions;
using AppLimit.CloudComputing.SharpBox.Common.IO;
using AppLimit.CloudComputing.SharpBox.Common.Net;
using AppLimit.CloudComputing.SharpBox.Common.Net.Json;
using AppLimit.CloudComputing.SharpBox.Common.Net.oAuth;
using AppLimit.CloudComputing.SharpBox.Common.Net.oAuth.Context;
using AppLimit.CloudComputing.SharpBox.Common.Net.oAuth.Token;
using AppLimit.CloudComputing.SharpBox.Common.Net.Web;
using AppLimit.CloudComputing.SharpBox.Exceptions;
using AppLimit.CloudComputing.SharpBox.StorageProvider.API;
using AppLimit.CloudComputing.SharpBox.StorageProvider.BaseObjects;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.DropBox.Logic
{
    internal class DropBoxStorageProviderService : GenericStorageProviderService
    {
        // token storage definitions        
        public const String TokenDropBoxCredUsername = "TokenDropBoxUsername";
        public const String TokenDropBoxCredPassword = "TokenDropBoxPassword";
        public const String TokenDropBoxAppKey = "TokenDropBoxAppKey";
        public const String TokenDropBoxAppSecret = "TokenDropBoxAppSecret";

        // server and base url 
        public const string DropBoxPrototcolPrefix = "https://";
        public const String DropBoxServer = "api.dropbox.com";
        public const String DropBoxEndUserServer = "www.dropbox.com";
        public const String DropBoxContentServer = "api-content.dropbox.com";
        public const String DropBoxBaseUrl = DropBoxPrototcolPrefix + DropBoxServer;
        public const String DropBoxBaseUrlEndUser = DropBoxPrototcolPrefix + DropBoxEndUserServer;

        // api version
        public const String DropBoxApiVersion = "{0}"; // it's a formt placeholder

        // action urls
        public const String DropBoxGetAccountInfo = DropBoxBaseUrl + "/" + DropBoxApiVersion + "/account/info";
        public const String DropBoxSandboxRoot = DropBoxBaseUrl + "/" + DropBoxApiVersion + "/metadata/sandbox/";
        public const String DropBoxDropBoxRoot = DropBoxBaseUrl + "/" + DropBoxApiVersion + "/metadata/dropbox/";
        public const String DropBoxCreateFolder = DropBoxBaseUrl + "/" + DropBoxApiVersion + "/fileops/create_folder";
        public const String DropBoxDeleteItem = DropBoxBaseUrl + "/" + DropBoxApiVersion + "/fileops/delete";
        public const String DropBoxMoveItem = DropBoxBaseUrl + "/" + DropBoxApiVersion + "/fileops/move";
        public const String DropBoxCopyItem = DropBoxBaseUrl + "/" + DropBoxApiVersion + "/fileops/copy";

        public const String DropBoxChunkedUpload = DropBoxPrototcolPrefix + DropBoxContentServer + "/" + DropBoxApiVersion + "/chunked_upload";
        public const String DropBoxCommitChunkedUpload = DropBoxPrototcolPrefix + DropBoxContentServer + "/" + DropBoxApiVersion + "/commit_chunked_upload";

        public const String DropBoxUploadDownloadFile = DropBoxPrototcolPrefix + DropBoxContentServer + "/" + DropBoxApiVersion + "/files";

        // authorization urls
        public const string DropBoxOAuthRequestToken = DropBoxBaseUrl + "/" + DropBoxApiVersion + "/oauth/request_token";
        public const string DropBoxOAuthAccessToken = DropBoxBaseUrl + "/" + DropBoxApiVersion + "/oauth/access_token";
        public const string DropBoxOAuthAuthToken = DropBoxBaseUrlEndUser + "/" + DropBoxApiVersion + "/oauth/authorize";

        #region DropBox Specific members

        public DropBoxAccountInfo GetAccountInfo(IStorageProviderSession session)
        {
            // request the json object via oauth            
            int code;
            var res = DropBoxRequestParser.RequestResourceByUrl(GetUrlString(DropBoxGetAccountInfo, session.ServiceConfiguration), this, session, out code);

            // parse the jason stuff            
            return new DropBoxAccountInfo(res);
        }

        #endregion

        #region IStorageProviderService Members

        public override bool VerifyAccessTokenType(ICloudStorageAccessToken token)
        {
            return (token is DropBoxToken);
        }

        public override IStorageProviderSession CreateSession(ICloudStorageAccessToken token, ICloudStorageConfiguration configuration)
        {
            // get the user credentials
            var userToken = token as DropBoxToken;
            var svcConfig = configuration as DropBoxConfiguration;

            // get the session
            return Authorize(userToken, svcConfig);
        }

        public override CloudStorageLimits GetLimits(IStorageProviderSession session)
        {
            var accountInfo = GetAccountInfo(session);
            var quotaRemained = accountInfo.QuotaInfo.QuotaBytes - accountInfo.QuotaInfo.NormalBytes - accountInfo.QuotaInfo.SharedBytes;
            return new CloudStorageLimits
                {
                    MaxUploadFileSize = Math.Min((long)quotaRemained, session.ServiceConfiguration.Limits.MaxUploadFileSize),
                    MaxChunkedUploadFileSize = Math.Min((long)quotaRemained, session.ServiceConfiguration.Limits.MaxChunkedUploadFileSize),
                    MaxDownloadFileSize = -1,
                };
        }

        public override ICloudFileSystemEntry RequestResource(IStorageProviderSession session, string name, ICloudDirectoryEntry parent)
        {
            var path = DropBoxResourceIDHelpers.GetResourcePath(parent, name);
            var uriString = GetResourceUrlInternal(session, path);

            int code;
            var res = DropBoxRequestParser.RequestResourceByUrl(uriString, this, session, out code);

            if (res.Length == 0)
            {
                if (code != (int)HttpStatusCode.OK)
                    throw new SharpBoxException(
                        code == (int)HttpStatusCode.NotFound
                            ? SharpBoxErrorCodes.ErrorFileNotFound
                            : SharpBoxErrorCodes.ErrorCouldNotRetrieveDirectoryList,
                        new HttpException(Convert.ToInt32(code), "HTTP Error"));
                throw new SharpBoxException(SharpBoxErrorCodes.ErrorCouldNotRetrieveDirectoryList);
            }

            // build the entry and subchilds
            var entry = DropBoxRequestParser.CreateObjectsFromJsonString(res, this, session);

            var hash = entry.GetPropertyValue("hash");
            if (!string.IsNullOrEmpty(hash))
            {
                DropBoxRequestParser.Addhash(uriString, hash, res, session);
            }
            // check if it was a deleted file
            if (entry.IsDeleted)
                return null;

            // set the parent
            if (parent is BaseDirectoryEntry && parent.Id.Equals(entry.ParentID))
                (parent as BaseDirectoryEntry).AddChild(entry);

            return entry;
        }

        public override void RefreshResource(IStorageProviderSession session, ICloudFileSystemEntry resource)
        {
            var path = GetResourceUrlInternal(session, DropBoxResourceIDHelpers.GetResourcePath(resource));

            int code;
            var res = DropBoxRequestParser.RequestResourceByUrl(path, this, session, out code);

            if (res.Length == 0)
                throw new SharpBoxException(SharpBoxErrorCodes.ErrorCouldNotRetrieveDirectoryList);

            // build the entry and subchilds
            DropBoxRequestParser.UpdateObjectFromJsonString(res, resource as BaseFileEntry, this, session);

            var hash = resource.GetPropertyValue("hash");
            if (!string.IsNullOrEmpty(hash))
            {
                DropBoxRequestParser.Addhash(path, hash, res, session);
            }
        }

        public override ICloudFileSystemEntry CreateResource(IStorageProviderSession session, string name, ICloudDirectoryEntry parent)
        {
            var path = DropBoxResourceIDHelpers.GetResourcePath(parent, name);

            var parameters = new Dictionary<string, string>
                {
                    { "path", path },
                    { "root", GetRootToken(session as DropBoxStorageProviderSession) }
                };

            int code;
            var res = DropBoxRequestParser.RequestResourceByUrl(GetUrlString(DropBoxCreateFolder, session.ServiceConfiguration), parameters, this, session, out code);
            if (res.Length != 0)
            {
                var entry = DropBoxRequestParser.CreateObjectsFromJsonString(res, this, session);
                if (parent is BaseDirectoryEntry && parent.Id.Equals(entry.ParentID))
                    (parent as BaseDirectoryEntry).AddChild(entry);
                return entry;
            }

            return null;
        }

        public override bool DeleteResource(IStorageProviderSession session, ICloudFileSystemEntry entry)
        {
            var resourcePath = DropBoxResourceIDHelpers.GetResourcePath(entry);

            var parameters = new Dictionary<string, string>
                {
                    { "path", resourcePath },
                    { "root", GetRootToken(session as DropBoxStorageProviderSession) }
                };

            try
            {
                int code;
                DropBoxRequestParser.RequestResourceByUrl(GetUrlString(DropBoxDeleteItem, session.ServiceConfiguration), parameters, this, session, out code);

                var parent = entry.Parent as BaseDirectoryEntry;
                if (parent != null)
                {
                    parent.RemoveChildById(entry.Id);
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public override bool MoveResource(IStorageProviderSession session, ICloudFileSystemEntry fsentry, ICloudDirectoryEntry newParent)
        {
            var oldPath = DropBoxResourceIDHelpers.GetResourcePath(fsentry);
            var path = DropBoxResourceIDHelpers.GetResourcePath(newParent, fsentry.Name);

            if (MoveOrRenameItem(session as DropBoxStorageProviderSession, fsentry as BaseFileEntry, path))
            {
                // set the new parent
                if (fsentry.Parent is BaseDirectoryEntry)
                    (fsentry.Parent as BaseDirectoryEntry).RemoveChildById(oldPath);
                if (newParent is BaseDirectoryEntry)
                    (newParent as BaseDirectoryEntry).AddChild(fsentry as BaseFileEntry);

                return true;
            }

            return false;
        }

        public override bool CopyResource(IStorageProviderSession session, ICloudFileSystemEntry fsentry, ICloudDirectoryEntry newParent)
        {
            var path = DropBoxResourceIDHelpers.GetResourcePath(newParent, fsentry.Name);

            // Move
            if (CopyItem(session as DropBoxStorageProviderSession, fsentry as BaseFileEntry, path))
            {
                // set the new parent
                if (newParent != null && newParent is BaseDirectoryEntry)
                    (newParent as BaseDirectoryEntry).AddChild(fsentry as BaseFileEntry);

                return true;
            }

            return false;
        }

        public override bool RenameResource(IStorageProviderSession session, ICloudFileSystemEntry fsentry, string newName)
        {
            var path = DropBoxResourceIDHelpers.GetResourcePath(fsentry).ReplaceLast(fsentry.Name, newName);
            return MoveOrRenameItem(session as DropBoxStorageProviderSession, fsentry as BaseFileEntry, path);
        }

        public override void StoreToken(IStorageProviderSession session, Dictionary<String, string> tokendata, ICloudStorageAccessToken token)
        {
            if (token is DropBoxRequestToken)
            {
                var requestToken = token as DropBoxRequestToken;
                tokendata.Add(TokenDropBoxAppKey, requestToken.RealToken.TokenKey);
                tokendata.Add(TokenDropBoxAppSecret, requestToken.RealToken.TokenSecret);
            }
            else
            {
                var dropboxToken = token as DropBoxToken;
                tokendata.Add(TokenDropBoxCredPassword, dropboxToken.TokenSecret);
                tokendata.Add(TokenDropBoxCredUsername, dropboxToken.TokenKey);
                tokendata.Add(TokenDropBoxAppKey, dropboxToken.BaseTokenInformation.ConsumerKey);
                tokendata.Add(TokenDropBoxAppSecret, dropboxToken.BaseTokenInformation.ConsumerSecret);
            }
        }

        public override ICloudStorageAccessToken LoadToken(Dictionary<String, string> tokendata)
        {
            // get the credential type
            var type = tokendata[CloudStorage.TokenCredentialType];

            if (type.Equals(typeof (DropBoxToken).ToString()))
            {
                var tokenSecret = tokendata[TokenDropBoxCredPassword];
                var tokenKey = tokendata[TokenDropBoxCredUsername];

                var bc = new DropBoxBaseTokenInformation
                    {
                        ConsumerKey = tokendata[TokenDropBoxAppKey],
                        ConsumerSecret = tokendata[TokenDropBoxAppSecret]
                    };

                return new DropBoxToken(tokenKey, tokenSecret, bc);
            }
            if (type.Equals(typeof (DropBoxRequestToken).ToString()))
            {
                var tokenSecret = tokendata[TokenDropBoxAppSecret];
                var tokenKey = tokendata[TokenDropBoxAppKey];

                return new DropBoxRequestToken(new OAuthToken(tokenKey, tokenSecret));
            }

            throw new InvalidCastException("Token type not supported through this provider");
        }

        public override string GetResourceUrl(IStorageProviderSession session, ICloudFileSystemEntry fileSystemEntry, String additionalPath)
        {
            var path = DropBoxResourceIDHelpers.GetResourcePath(fileSystemEntry, additionalPath);
            var basicUrl = session.ServiceConfiguration.ServiceLocator.ToString().TrimEnd('/');
            return !String.IsNullOrEmpty(path) ? basicUrl + "/" + path : basicUrl;
        }

        public override Stream CreateDownloadStream(IStorageProviderSession session, ICloudFileSystemEntry fileSystemEntry)
        {
            // build the url 
            var url = GetDownloadFileUrlInternal(session, fileSystemEntry);

            // build the service
            var svc = new OAuthService();

            // get the dropbox session
            var dropBoxSession = session as DropBoxStorageProviderSession;

            // create webrequst 
            var requestProtected = svc.CreateWebRequest(url, WebRequestMethodsEx.Http.Get, null, null, dropBoxSession.Context, (DropBoxToken)dropBoxSession.SessionToken, null);

            // get the response
            var response = svc.GetWebResponse(requestProtected);

            // get the data stream
            var orgStream = svc.GetResponseStream(response);

            // build the download stream
            var dStream = new BaseFileEntryDownloadStream(orgStream, fileSystemEntry);

            // put the disposable on the stack
            dStream._DisposableObjects.Push(response);

            // go ahead
            return dStream;
        }

        public override Stream CreateUploadStream(IStorageProviderSession session, ICloudFileSystemEntry fileSystemEntry, long uploadSize)
        {
            // build the url 
            var url = GetDownloadFileUrlInternal(session, fileSystemEntry.Parent);

            // get the session
            var dbSession = session as DropBoxStorageProviderSession;

            // build service
            var svc = new OAuthService();

            // encode the filename
            var fileName = fileSystemEntry.Name;

            // build oAuth parameter
            var param = new Dictionary<string, string>();
            param.Add("file", fileName);

            // sign the url 
            var signedUrl = svc.GetSignedUrl(url, dbSession.Context, dbSession.SessionToken as DropBoxToken, param);

            // build upload web request
            var uploadRequest = svc.CreateWebRequestMultiPartUpload(signedUrl, null);

            // get the network stream
            var ws = svc.GetRequestStreamMultiPartUpload(uploadRequest, fileName, uploadSize);

            // register the post dispose opp
            ws.PushPostDisposeOperation(CommitUploadStream, svc, uploadRequest, uploadSize, fileSystemEntry, ws);

            // go ahead
            return ws;
        }

        public override bool SupportsDirectRetrieve
        {
            get { return true; }
        }

        public void CommitUploadStream(params object[] arg)
        {
            // convert the args
            var svc = arg[0] as OAuthService;
            var uploadRequest = arg[1] as HttpWebRequest;
            var uploadSize = (long)arg[2];
            var fileSystemEntry = arg[3] as BaseFileEntry;

            var requestStream = arg[4] as WebRequestStream;

            // check if all data was written into stream
            if (requestStream.WrittenBytes != uploadRequest.ContentLength)
                // nothing todo request was aborted
                return;

            // perform the request
            int code;
            WebException e;
            svc.PerformWebRequest(uploadRequest, null, out code, out e);

            // check the ret value
            if (code != (int)HttpStatusCode.OK)
                SharpBoxException.ThrowSharpBoxExceptionBasedOnHttpErrorCode(uploadRequest, (HttpStatusCode)code, e);

            if (fileSystemEntry != null)
            {
                fileSystemEntry.Length = uploadSize;
                fileSystemEntry.Id = fileSystemEntry.ParentID != "/" ? fileSystemEntry.ParentID + "/" + fileSystemEntry.Name : fileSystemEntry.Name;

                var parent = fileSystemEntry.Parent as BaseDirectoryEntry;
                if (parent != null)
                {
                    parent.RemoveChildById(fileSystemEntry.Name);
                    parent.AddChild(fileSystemEntry);
                }
            }
        }

        public override void CommitStreamOperation(IStorageProviderSession session, ICloudFileSystemEntry fileSystemEntry, nTransferDirection direction, Stream notDisposedStream)
        {
        }

        #region resumable uploads

        public override bool SupportsChunking
        {
            get { return true; }
        }

        public override IResumableUploadSession CreateUploadSession(IStorageProviderSession session, ICloudFileSystemEntry fileSystemEntry, long bytesToTransfer)
        {
            //return new ResumableUploadSession(fileSystemEntry, bytesToTransfer);
            if (fileSystemEntry == null) throw new ArgumentNullException("fileSystemEntry");

            return new ResumableUploadSession(fileSystemEntry.Id, fileSystemEntry.Name, fileSystemEntry.ParentID, bytesToTransfer);
        }

        public override void UploadChunk(IStorageProviderSession session, IResumableUploadSession uploadSession, Stream stream, long chunkLength)
        {
            if (uploadSession.Status == ResumableUploadSessionStatus.Completed || uploadSession.Status == ResumableUploadSessionStatus.Aborted)
                throw new InvalidOperationException("Upload session was either completed or aborted.");

            if (stream == null)
                throw new ArgumentNullException("stream");

            var requestParams = new Dictionary<string, string>();
            if (uploadSession.Status == ResumableUploadSessionStatus.Started)
            {
                requestParams.Add("upload_id", uploadSession.GetItem<string>("UploadId"));
                requestParams.Add("offset", uploadSession.BytesTransfered.ToString());
            }

            var request = new OAuthService().CreateWebRequest(GetUrlString(DropBoxChunkedUpload, session.ServiceConfiguration),
                                                              "PUT",
                                                              null,
                                                              null,
                                                              ((DropBoxStorageProviderSession)session).Context,
                                                              (DropBoxToken)session.SessionToken,
                                                              requestParams);

            request.ContentLength = chunkLength;
            using (var requestStream = request.GetRequestStream())
            {
                stream.CopyTo(requestStream);
            }

            using (var response = request.GetResponse())
            using (var responseStream = response.GetResponseStream())
            {
                if (responseStream == null) return;

                var json = new JsonHelper();
                using (var streamReader = new StreamReader(responseStream))
                {
                    json.ParseJsonMessage(streamReader.ReadToEnd());
                }

                var uplSession = (ResumableUploadSession)uploadSession;
                uplSession["UploadId"] = json.GetProperty("upload_id");
                uplSession["Expired"] = json.GetDateTimeProperty("expired");
                uplSession.BytesTransfered += chunkLength;
                uplSession.Status = ResumableUploadSessionStatus.Started;

                if (uplSession.BytesToTransfer == uploadSession.BytesTransfered)
                {
                    CommitUploadSession(session, uploadSession);
                }
            }
        }

        public void CommitUploadSession(IStorageProviderSession session, IResumableUploadSession uploadSession)
        {
            if (uploadSession.Status == ResumableUploadSessionStatus.Started)
            {
                var requestService = new OAuthService();
                var request = requestService.CreateWebRequest(GetCommitUploadSessionUrl(session, uploadSession),
                                                              "POST",
                                                              null,
                                                              null,
                                                              ((DropBoxStorageProviderSession)session).Context,
                                                              (DropBoxToken)session.SessionToken,
                                                              new Dictionary<string, string> { { "upload_id", uploadSession.GetItem<string>("UploadId") } });

                int httpStatusCode;
                WebException httpException;
                requestService.PerformWebRequest(request, null, out httpStatusCode, out httpException);

                if (httpStatusCode != (int)HttpStatusCode.OK)
                    SharpBoxException.ThrowSharpBoxExceptionBasedOnHttpErrorCode((HttpWebRequest)request, (HttpStatusCode)httpStatusCode, httpException);

                var rUploadSession = uploadSession as ResumableUploadSession;
                rUploadSession.FileId = rUploadSession.ParentId != "/" ? rUploadSession.ParentId + "/" + rUploadSession.FileName : rUploadSession.FileName;


                //var file = (BaseFileEntry)uploadSession.File;
                //file.Length = uploadSession.BytesToTransfer;
                //file.Id = file.ParentID != "/" ? file.ParentID + "/" + file.Name : file.Name;

                //var parent = file.Parent as BaseDirectoryEntry;
                //if (parent != null)
                //{
                //    parent.RemoveChildById(file.Name);
                //    parent.AddChild(file);
                //}

                ((ResumableUploadSession)uploadSession).Status = ResumableUploadSessionStatus.Completed;
            }
        }

        public override void AbortUploadSession(IStorageProviderSession session, IResumableUploadSession uploadSession)
        {
            if (uploadSession.Status != ResumableUploadSessionStatus.Completed)
            {
                ((ResumableUploadSession)uploadSession).Status = ResumableUploadSessionStatus.Aborted;
            }
        }

        #endregion

        #endregion

        #region Authorization Helper

        private DropBoxStorageProviderSession Authorize(DropBoxToken token, DropBoxConfiguration configuration)
        {
            // Get a valid dropbox session through oAuth authorization
            var session = BuildSessionFromAccessToken(token, configuration);

            //( Get a valid root object
            return GetRootBySessionExceptionHandled(session);
        }

        private DropBoxStorageProviderSession GetRootBySessionExceptionHandled(DropBoxStorageProviderSession session)
        {
            try
            {
                var root = GetRootBySession(session);

                // return the infos
                return root == null ? null : session;
            }
            catch (SharpBoxException ex)
            {
                // check if the exception an http error 403
                if (ex.InnerException is HttpException)
                {
                    if ((((HttpException)ex.InnerException).GetHttpCode() == 403) || (((HttpException)ex.InnerException).GetHttpCode() == 401))
                        throw new UnauthorizedAccessException();
                }

                // otherwise rethrow the old exception
                throw ex;
            }
        }

        private ICloudDirectoryEntry GetRootBySession(DropBoxStorageProviderSession session)
        {
            // now check if we have application keys and secrets from a sandbox or a fullbox
            // try to load the root of the full box if this fails we have only sandbox access
            ICloudDirectoryEntry root;

            try
            {
                root = RequestResource(session, "/", null) as ICloudDirectoryEntry;
            }
            catch (SharpBoxException ex)
            {
                if (session.SandBoxMode)
                    throw ex;
                else
                {
                    // enable sandbox mode
                    session.SandBoxMode = true;

                    // retry to get root object
                    root = RequestResource(session, "/", null) as ICloudDirectoryEntry;
                }
            }

            // go ahead
            return root;
        }

        public DropBoxStorageProviderSession BuildSessionFromAccessToken(DropBoxToken token, DropBoxConfiguration configuration)
        {
            // build the consumer context
            var consumerContext = new OAuthConsumerContext(token.BaseTokenInformation.ConsumerKey, token.BaseTokenInformation.ConsumerSecret);

            // build the session
            var session = new DropBoxStorageProviderSession(token, configuration, consumerContext, this);

            // go aahead
            return session;
        }

        private static String GetRootToken(DropBoxStorageProviderSession session)
        {
            return session.SandBoxMode ? "sandbox" : "dropbox";
        }

        private bool MoveOrRenameItem(DropBoxStorageProviderSession session, BaseFileEntry orgEntry, String toPath)
        {
            return MoveOrRenameOrCopyItem(session, orgEntry, toPath, false);
        }

        private bool CopyItem(DropBoxStorageProviderSession session, BaseFileEntry orgEntry, String toPath)
        {
            return MoveOrRenameOrCopyItem(session, orgEntry, toPath, true);
        }

        private bool MoveOrRenameOrCopyItem(DropBoxStorageProviderSession session, BaseFileEntry orgEntry, String toPath, bool copy)
        {
            // build the path for resource
            var resourcePath = DropBoxResourceIDHelpers.GetResourcePath(orgEntry);

            // request the json object via oauth
            var parameters = new Dictionary<string, string>
                {
                    { "from_path", resourcePath },
                    { "root", GetRootToken(session) },
                    { "to_path", toPath }
                };

            try
            {
                // move or rename the entry
                int code;
                var res = DropBoxRequestParser.RequestResourceByUrl(GetUrlString(copy ? DropBoxCopyItem : DropBoxMoveItem, session.ServiceConfiguration), parameters, this, session, out code);

                // update the entry
                DropBoxRequestParser.UpdateObjectFromJsonString(res, orgEntry, this, session);
            }
            catch (Exception)
            {
                return false;
            }

            orgEntry.Id = toPath;
            return true;
        }

        private static string GetResourceUrlInternal(IStorageProviderSession session, String path)
        {
            if (session is DropBoxStorageProviderSession)
            {
                var dbSession = session as DropBoxStorageProviderSession;
                String uri = PathHelper.Combine(dbSession.SandBoxMode
                                                    ? GetUrlString(DropBoxSandboxRoot, session.ServiceConfiguration)
                                                    : GetUrlString(DropBoxDropBoxRoot, session.ServiceConfiguration),
                                                HttpUtilityEx.UrlEncodeUTF8(path));
                return uri;
            }
            return null;
        }

        public static String GetDownloadFileUrlInternal(IStorageProviderSession session, ICloudFileSystemEntry entry)
        {
            // cast varibales
            var dropBoxSession = session as DropBoxStorageProviderSession;

            // gather information
            var rootToken = GetRootToken(dropBoxSession);
            var dropboxPath = DropBoxResourceIDHelpers.GetResourcePath(entry);

            // add all information to url;
            var url = GetUrlString(DropBoxUploadDownloadFile, session.ServiceConfiguration) + "/" + rootToken;

            if (dropboxPath.Length > 0 && dropboxPath[0] != '/')
                url += "/";

            url += HttpUtilityEx.UrlEncodeUTF8(dropboxPath);

            return url;
        }

        public static String GetCommitUploadSessionUrl(IStorageProviderSession session, IResumableUploadSession uploadSession)
        {
            var rUploadSession = uploadSession as ResumableUploadSession;
            if (rUploadSession == null) throw new ArgumentNullException("uploadSession");

            return GetUrlString(DropBoxCommitChunkedUpload, session.ServiceConfiguration)
                   + "/" + GetRootToken((DropBoxStorageProviderSession)session)
                   + "/" + HttpUtilityEx.UrlEncodeUTF8(DropBoxResourceIDHelpers.GetResourcePath(null, rUploadSession.FileName, rUploadSession.ParentId));
        }

        #endregion

        #region API URL helper

        public static String GetUrlString(String urltemplate, ICloudStorageConfiguration configuration)
        {
            if (urltemplate.Contains("{0}"))
            {
                if (configuration == null || !(configuration is DropBoxConfiguration))
                    return String.Format(urltemplate, ((int)DropBoxAPIVersion.Stable).ToString());

                var versionValue = (int)((DropBoxConfiguration)configuration).APIVersion;
                return String.Format(urltemplate, versionValue.ToString());
            }

            return urltemplate;
        }

        #endregion
    }
}