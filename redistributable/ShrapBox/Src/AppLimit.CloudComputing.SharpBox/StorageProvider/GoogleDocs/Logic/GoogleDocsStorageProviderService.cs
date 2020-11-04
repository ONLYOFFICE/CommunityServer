using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using AppLimit.CloudComputing.SharpBox.Common.Extensions;
using AppLimit.CloudComputing.SharpBox.Common.Net.Web;
using AppLimit.CloudComputing.SharpBox.Common.Net.oAuth;
using AppLimit.CloudComputing.SharpBox.Common.Net.oAuth.Context;
using AppLimit.CloudComputing.SharpBox.Common.Net.oAuth.Token;
using AppLimit.CloudComputing.SharpBox.Exceptions;
using AppLimit.CloudComputing.SharpBox.StorageProvider.API;
using AppLimit.CloudComputing.SharpBox.StorageProvider.BaseObjects;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.GoogleDocs.Logic
{
    internal class GoogleDocsStorageProviderService : GenericStorageProviderService
    {
        public enum RemoveMode
        {
            Trash,
            Delete,
            FromParentCollection
        };

        #region GoogleDocs Specific

        public void RefreshDirectoryContent(IStorageProviderSession session, BaseDirectoryEntry entry)
        {
            if (entry == null)
                return;

            var url = String.Format(GoogleDocsConstants.GoogleDocsContentsUrlFormat, entry.Id.ReplaceFirst("_", "%3a"));
            var parameters = new Dictionary<string, string> { { "max-results", "1000" } };
            try
            {
                while (!String.IsNullOrEmpty(url))
                {
                    var request = CreateWebRequest(session, url, "GET", parameters);
                    using (var response = (HttpWebResponse)request.GetResponse())
                    using (var rs = response.GetResponseStream())
                    using (var streamReader = new StreamReader(rs))
                    {
                        var feedXml = streamReader.ReadToEnd();
                        var childs = GoogleDocsXmlParser.ParseEntriesXml(session, feedXml);
                        entry.AddChilds(childs);

                        url = GoogleDocsXmlParser.ParseNext(feedXml);
                    }
                }
            }
            catch (WebException)
            {
            }
        }

        public bool RemoveResource(IStorageProviderSession session, ICloudFileSystemEntry resource, RemoveMode mode)
        {
            String url;
            Dictionary<String, String> parameters = null;

            if (mode == RemoveMode.FromParentCollection)
            {
                var pId = (resource.Parent != null ? resource.Parent.Id : GoogleDocsConstants.RootFolderId).ReplaceFirst("_", "%3a");
                url = String.Format("{0}/{1}/contents/{2}", GoogleDocsConstants.GoogleDocsFeedUrl, pId, resource.Id.ReplaceFirst("_", "%3a"));
            }
            else
            {
                url = String.Format(GoogleDocsConstants.GoogleDocsResourceUrlFormat, resource.Id.ReplaceFirst("_", "%3a"));
                parameters = new Dictionary<string, string> { { "delete", "true" } };
            }

            var request = CreateWebRequest(session, url, "DELETE", parameters);
            request.Headers.Add("If-Match", "*");

            try
            {
                using (var response = (HttpWebResponse)request.GetResponse())
                    if (response.StatusCode == HttpStatusCode.OK)
                        return true;
            }
            catch (WebException)
            {
            }

            return false;
        }

        public bool AddToCollection(IStorageProviderSession session, ICloudFileSystemEntry resource, ICloudDirectoryEntry collection)
        {
            var url = String.Format(GoogleDocsConstants.GoogleDocsContentsUrlFormat, collection.Id.ReplaceFirst("_", "%3a"));
            var request = CreateWebRequest(session, url, "POST", null);
            GoogleDocsXmlParser.WriteAtom(request, GoogleDocsXmlParser.EntryElement(null, GoogleDocsXmlParser.IdElement(resource.Id.ReplaceFirst("_", "%3a"))));

            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.Created)
                    return true;
            }
            catch (WebException)
            {
            }

            return false;
        }

        #endregion

        #region GenericStorageProviderService members

        public override bool VerifyAccessTokenType(ICloudStorageAccessToken token)
        {
            return token is GoogleDocsToken;
        }

        public override IStorageProviderSession CreateSession(ICloudStorageAccessToken token, ICloudStorageConfiguration configuration)
        {
            var gdToken = token as GoogleDocsToken;
            return new GoogleDocsStorageProviderSession(this,
                                                        configuration,
                                                        new OAuthConsumerContext(gdToken.ConsumerKey, gdToken.ConsumerSecret),
                                                        gdToken);
        }

        public override ICloudFileSystemEntry RequestResource(IStorageProviderSession session, string name, ICloudDirectoryEntry parent)
        {
            if (GoogleDocsResourceHelper.IsNoolOrRoot(parent) && GoogleDocsResourceHelper.IsRootName(name))
            {
                var root = new BaseDirectoryEntry("/", 0, DateTime.Now, session.Service, session) { Id = GoogleDocsConstants.RootFolderId };
                root.SetPropertyValue(GoogleDocsConstants.ResCreateMediaProperty, GoogleDocsConstants.RootResCreateMediaUrl);
                RefreshDirectoryContent(session, root);
                return root;
            }

            if (name.Equals("/"))
            {
                RefreshDirectoryContent(session, parent as BaseDirectoryEntry);
            }

            if (GoogleDocsResourceHelper.IsResorceId(name))
            {
                var url = String.Format(GoogleDocsConstants.GoogleDocsResourceUrlFormat, name.ReplaceFirst("_", "%3a"));
                var request = CreateWebRequest(session, url, "GET", null);
                try
                {
                    string xml;
                    using (var response = (HttpWebResponse)request.GetResponse())
                    using (var rs = response.GetResponseStream())
                    using (var streamReader = new StreamReader(rs))
                    {
                        xml = streamReader.ReadToEnd();
                    }
                    var entry = GoogleDocsXmlParser.ParseEntriesXml(session, xml).FirstOrDefault();

                    if (entry == null)
                        throw new SharpBoxException(SharpBoxErrorCodes.ErrorFileNotFound);

                    if (parent != null)
                        (parent as BaseDirectoryEntry).AddChild(entry);

                    var dirEntry = entry as BaseDirectoryEntry;

                    if (dirEntry == null)
                        return entry;

                    RefreshDirectoryContent(session, dirEntry);
                    return dirEntry;
                }
                catch (WebException)
                {
                    throw new SharpBoxException(SharpBoxErrorCodes.ErrorFileNotFound);
                }
            }

            throw new SharpBoxException(SharpBoxErrorCodes.ErrorInvalidFileOrDirectoryName);
        }

        public override void RefreshResource(IStorageProviderSession session, ICloudFileSystemEntry resource)
        {
            if (resource == null)
                return;

            if (resource.Id.Equals(GoogleDocsConstants.RootFolderId))
            {
                RefreshDirectoryContent(session, resource as BaseDirectoryEntry);
                return;
            }

            var resourceUrl = String.Format(GoogleDocsConstants.GoogleDocsResourceUrlFormat, resource.Id.ReplaceFirst("_", "%3a"));
            var request = CreateWebRequest(session, resourceUrl, "GET", null);
            request.Headers.Add("If-None-Match", resource.GetPropertyValue(GoogleDocsConstants.EtagProperty));

            try
            {
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode != HttpStatusCode.NotModified)
                    {
                        using (var s = response.GetResponseStream())
                        using (var streamReader = new StreamReader(s))
                        {
                            var xml = streamReader.ReadToEnd();

                            GoogleDocsResourceHelper.UpdateResourceByXml(session, out resource, xml);
                        }
                    }
                }

                var dirEntry = resource as BaseDirectoryEntry;

                if (dirEntry == null || dirEntry.HasChildrens == nChildState.HasChilds)
                    return;

                RefreshDirectoryContent(session, dirEntry);
            }
            catch (WebException)
            {

            }
        }

        public override ICloudFileSystemEntry CreateResource(IStorageProviderSession session, string name, ICloudDirectoryEntry parent)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Name cannot be empty");
            }

            var url = GoogleDocsResourceHelper.IsNoolOrRoot(parent)
                          ? GoogleDocsConstants.GoogleDocsFeedUrl
                          : String.Format(GoogleDocsConstants.GoogleDocsContentsUrlFormat, parent.Id.ReplaceFirst("_", "%3a"));

            var request = CreateWebRequest(session, url, "POST", null);
            GoogleDocsXmlParser.WriteAtom(request, GoogleDocsXmlParser.EntryElement(GoogleDocsXmlParser.CategoryElement(), GoogleDocsXmlParser.TitleElement(name)));

            try
            {
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.Created)
                    {
                        using (var rs = response.GetResponseStream())
                        using (var streamReader = new StreamReader(rs))
                        {
                            var xml = streamReader.ReadToEnd();
                            var entry = GoogleDocsXmlParser.ParseEntriesXml(session, xml).First();

                            if (parent != null)
                                (parent as BaseDirectoryEntry).AddChild(entry);

                            return entry;
                        }
                    }
                }
            }
            catch (WebException)
            {
                throw new SharpBoxException(SharpBoxErrorCodes.ErrorCreateOperationFailed);
            }

            return null;
        }

        public override bool RenameResource(IStorageProviderSession session, ICloudFileSystemEntry fsentry, string newName)
        {
            if (String.IsNullOrEmpty(newName) || GoogleDocsResourceHelper.IsNoolOrRoot(fsentry))
                return false;

            var url = String.Format(GoogleDocsConstants.GoogleDocsResourceUrlFormat, fsentry.Id.ReplaceFirst("_", "%3a"));

            var request = CreateWebRequest(session, url, "PUT", null);
            request.Headers.Add("If-Match", fsentry.GetPropertyValue(GoogleDocsConstants.EtagProperty));
            GoogleDocsXmlParser.WriteAtom(request, GoogleDocsXmlParser.EntryElement(GoogleDocsXmlParser.TitleElement(newName)));

            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    //check if extension added
                    if (!(fsentry is ICloudDirectoryEntry) && String.IsNullOrEmpty(Path.GetExtension(newName)))
                        newName = newName + Path.GetExtension(fsentry.Name);
                    (fsentry as BaseFileEntry).Name = newName;
                    return true;
                }
            }
            catch (WebException)
            {
            }

            return false;
        }

        public override bool DeleteResource(IStorageProviderSession session, ICloudFileSystemEntry entry)
        {
            if (GoogleDocsResourceHelper.IsNoolOrRoot(entry))
                return false;

            if (RemoveResource(session, entry, RemoveMode.Delete))
            {
                var parent = entry.Parent as BaseDirectoryEntry;
                if (parent != null)
                    parent.RemoveChildById(entry.Id);

                return true;
            }

            return false;
        }

        public override bool MoveResource(IStorageProviderSession session, ICloudFileSystemEntry fsentry, ICloudDirectoryEntry newParent)
        {
            if (fsentry == null || newParent == null || GoogleDocsResourceHelper.IsNoolOrRoot(fsentry))
                return false;

            if (RemoveResource(session, fsentry, RemoveMode.FromParentCollection) && AddToCollection(session, fsentry, newParent))
            {
                if (fsentry.Parent != null)
                    (fsentry.Parent as BaseDirectoryEntry).RemoveChild(fsentry as BaseFileEntry);
                (newParent as BaseDirectoryEntry).AddChild(fsentry as BaseFileEntry);

                return true;
            }

            return false;
        }

        public override bool CopyResource(IStorageProviderSession session, ICloudFileSystemEntry fsentry, ICloudDirectoryEntry newParent)
        {
            if (AddToCollection(session, fsentry, newParent))
            {
                (newParent as BaseDirectoryEntry).AddChild(fsentry as BaseFileEntry);
                return true;
            }
            return false;
        }

        public override string GetResourceUrl(IStorageProviderSession session, ICloudFileSystemEntry entry, string path)
        {
            if (!String.IsNullOrEmpty(path))
            {
                var id = path;
                var index = id.LastIndexOf("/");
                if (index != -1)
                {
                    id = id.Substring(index + 1);
                }
                if (GoogleDocsResourceHelper.IsResorceId(id))
                {
                    return id;
                }
            }
            else if (entry != null)
            {
                return entry.Id;
            }
            return base.GetResourceUrl(session, null, path);
        }

        public override Stream CreateDownloadStream(IStorageProviderSession session, ICloudFileSystemEntry fileSystemEntry)
        {
            var url = fileSystemEntry.GetPropertyValue(GoogleDocsConstants.DownloadUrlProperty);

            var format = GoogleDocsResourceHelper.GetStreamExtensionByKind(fileSystemEntry.GetPropertyValue(GoogleDocsConstants.KindProperty));
            if (!String.IsNullOrEmpty(format))
            {
                url = String.Format("{0}&exportFormat={1}", url, format);
                if (format.Equals("docx"))
                    url += "&format=" + format;
            }

            var request = CreateWebRequest(session, url, "GET", null, true);
            var response = (HttpWebResponse)request.GetResponse();

            if (fileSystemEntry.Length > 0)
            {
                return new BaseFileEntryDownloadStream(response.GetResponseStream(), fileSystemEntry);
            }

            var isChukedEncoding = string.Equals(response.Headers.Get("Transfer-Encoding"), "Chunked", StringComparison.OrdinalIgnoreCase);
            if (!isChukedEncoding)
            {
                ((BaseFileEntry)fileSystemEntry).Length = response.ContentLength;
                return new BaseFileEntryDownloadStream(response.GetResponseStream(), fileSystemEntry);
            }

            var tempBuffer = new FileStream(Path.GetTempFileName(), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read, 8096, FileOptions.DeleteOnClose);
            using (var stream = response.GetResponseStream())
            {
                stream.CopyTo(tempBuffer);
            }
            tempBuffer.Flush();
            tempBuffer.Seek(0, SeekOrigin.Begin);
            return tempBuffer;
        }

        public override void CommitStreamOperation(IStorageProviderSession session, ICloudFileSystemEntry fileSystemEntry, nTransferDirection direction, Stream notDisposedStream)
        {
        }

        public override Stream CreateUploadStream(IStorageProviderSession session, ICloudFileSystemEntry fileSystemEntry, long uploadSize)
        {
            var tempStream = new FileStream(Path.GetTempFileName(), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose);
            var stream = new WebRequestStream(tempStream, null, null);
            stream.PushPreDisposeOperation(CommitUploadOperation, session, fileSystemEntry, tempStream, uploadSize);
            return stream;
        }

        public override bool SupportsDirectRetrieve
        {
            get { return false; }
        }

        private void CommitUploadOperation(object[] args)
        {
            var session = (IStorageProviderSession)args[0];
            var file = (BaseFileEntry)args[1];
            var stream = (Stream)args[2];
            var contentLength = (long)args[3];

            stream.Flush();
            stream.Seek(0, SeekOrigin.Begin);

            var uploadSession = CreateUploadSession(session, file, contentLength);
            UploadChunk(session, uploadSession, stream, contentLength);
        }

        #region resumable uploads

        public override bool SupportsChunking
        {
            get { return true; }
        }

        public override IResumableUploadSession CreateUploadSession(IStorageProviderSession session, ICloudFileSystemEntry fileSystemEntry, long bytesToTransfer)
        {
            WebRequest request;
            if (GoogleDocsResourceHelper.IsResorceId(fileSystemEntry.Id))
            {
                //request for update
                request = CreateWebRequest(session, fileSystemEntry.GetPropertyValue(GoogleDocsConstants.ResEditMediaProperty), "PUT", null, true);
                request.Headers.Add("If-Match", "*");
            }
            else
            {
                //request for create
                request = CreateWebRequest(session, fileSystemEntry.Parent.GetPropertyValue(GoogleDocsConstants.ResCreateMediaProperty) + "?convert=false", "POST", null, true);
            }

            if (GoogleDocsResourceHelper.OfGoogleDocsKind(fileSystemEntry))
            {
                ((BaseFileEntry)fileSystemEntry).Name = Path.GetFileNameWithoutExtension(fileSystemEntry.Name);
            }

            GoogleDocsXmlParser.WriteAtom(request, GoogleDocsXmlParser.EntryElement(GoogleDocsXmlParser.TitleElement(fileSystemEntry.Name)));
            request.Headers.Add("X-Upload-Content-Type", Common.Net.MimeMapping.GetMimeMapping(fileSystemEntry.Name));
            request.Headers.Add("X-Upload-Content-Length", bytesToTransfer.ToString(CultureInfo.InvariantCulture));


            var response = request.GetResponse();

            var uploadSession = new ResumableUploadSession(fileSystemEntry, bytesToTransfer);
            uploadSession["Location"] = response.Headers["Location"];
            uploadSession.Status = ResumableUploadSessionStatus.Started;

            return uploadSession;
        }

        public override void AbortUploadSession(IStorageProviderSession session, IResumableUploadSession uploadSession)
        {
            if (uploadSession.Status != ResumableUploadSessionStatus.Completed)
            {
                ((ResumableUploadSession)uploadSession).Status = ResumableUploadSessionStatus.Aborted;
            }
        }

        public override void UploadChunk(IStorageProviderSession session, IResumableUploadSession uploadSession, Stream stream, long chunkLength)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            if (uploadSession.Status != ResumableUploadSessionStatus.Started)
                throw new InvalidOperationException("Can't upload chunk for given upload session.");

            var request = WebRequest.Create(uploadSession.GetItem<string>("Location"));
            request.Method = "PUT";
            request.ContentLength = chunkLength;
            request.Headers.Add("Content-Range", string.Format("bytes {0}-{1}/{2}",
                                                               uploadSession.BytesTransfered,
                                                               uploadSession.BytesTransfered + chunkLength - 1,
                                                               uploadSession.BytesToTransfer));

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

            if (response.StatusCode != HttpStatusCode.Created)
            {
                var uplSession = (ResumableUploadSession)uploadSession;
                uplSession.BytesTransfered += chunkLength;

                var locationHeader = response.Headers["Location"];
                if (!string.IsNullOrEmpty(locationHeader))
                {
                    uplSession["Location"] = locationHeader;
                }
            }
            else
            {
                ((ResumableUploadSession)uploadSession).Status = ResumableUploadSessionStatus.Completed;

                using (var responseStream = response.GetResponseStream())
                {
                    if (responseStream == null) return;
                    string xml;
                    using (var streamReader = new StreamReader(responseStream))
                    {
                        xml = streamReader.ReadToEnd();
                    }
                    var respFile = GoogleDocsXmlParser.ParseEntriesXml(session, xml).First();
                    var initFile = (BaseFileEntry)uploadSession.File;

                    //replace old file with the file from response
                    initFile.Name = respFile.Name;
                    initFile.Id = respFile.Id;
                    initFile.Length = respFile.Length;
                    initFile.Modified = respFile.Modified;
                    initFile[GoogleDocsConstants.EtagProperty] = respFile[GoogleDocsConstants.EtagProperty];
                    initFile[GoogleDocsConstants.KindProperty] = respFile[GoogleDocsConstants.KindProperty];
                    initFile[GoogleDocsConstants.DownloadUrlProperty] = respFile[GoogleDocsConstants.DownloadUrlProperty];
                    initFile[GoogleDocsConstants.ResEditMediaProperty] = respFile[GoogleDocsConstants.ResEditMediaProperty];

                    var parent = initFile.Parent as BaseDirectoryEntry;
                    if (parent != null)
                    {
                        parent.RemoveChildById(initFile.Name);
                        parent.AddChild(initFile);
                    }
                }
            }

            response.Close();
        }

        #endregion

        #endregion

        #region oAuth

        public override void StoreToken(IStorageProviderSession session, Dictionary<string, string> tokendata, ICloudStorageAccessToken token)
        {
            if (token is GoogleDocsRequestToken)
            {
                var requestToken = token as GoogleDocsRequestToken;
                tokendata.Add(GoogleDocsConstants.TokenGoogleDocsAppKey, requestToken.RealToken.TokenKey);
                tokendata.Add(GoogleDocsConstants.TokenGoogleDocsAppSecret, requestToken.RealToken.TokenSecret);
            }
            else if (token is GoogleDocsToken)
            {
                var gdtoken = token as GoogleDocsToken;
                tokendata.Add(GoogleDocsConstants.TokenGoogleDocsAppKey, gdtoken.ConsumerKey);
                tokendata.Add(GoogleDocsConstants.TokenGoogleDocsAppSecret, gdtoken.ConsumerSecret);
                tokendata.Add(GoogleDocsConstants.TokenGoogleDocsUsername, gdtoken.TokenKey);
                tokendata.Add(GoogleDocsConstants.TokenGoogleDocsPassword, gdtoken.TokenSecret);
            }
        }

        public override ICloudStorageAccessToken LoadToken(Dictionary<string, string> tokendata)
        {
            var type = tokendata[CloudStorage.TokenCredentialType];

            if (type.Equals(typeof (GoogleDocsToken).ToString()))
            {
                var tokenKey = tokendata[GoogleDocsConstants.TokenGoogleDocsUsername];
                var tokenSecret = tokendata[GoogleDocsConstants.TokenGoogleDocsPassword];
                var consumerKey = tokendata[GoogleDocsConstants.TokenGoogleDocsAppKey];
                var consumerSecret = tokendata[GoogleDocsConstants.TokenGoogleDocsAppSecret];

                return new GoogleDocsToken(tokenKey, tokenSecret, consumerKey, consumerSecret);
            }

            if (type.Equals(typeof (GoogleDocsRequestToken).ToString()))
            {
                var tokenKey = tokendata[GoogleDocsConstants.TokenGoogleDocsAppKey];
                var tokenSecret = tokendata[GoogleDocsConstants.TokenGoogleDocsAppSecret];

                return new GoogleDocsRequestToken(new OAuthToken(tokenKey, tokenSecret));
            }

            throw new InvalidCastException("Token type not supported through this provider");
        }

        #endregion

        #region Helpers

        private static WebRequest CreateWebRequest(IStorageProviderSession storageProviderSession, String url, String method, Dictionary<String, String> parameters, bool oAuthParamsAsHeader = false)
        {
            var session = storageProviderSession as GoogleDocsStorageProviderSession;
            var configuration = session.ServiceConfiguration as GoogleDocsConfiguration;
            WebRequest request;
            if (!oAuthParamsAsHeader)
            {
                request = OAuthService.CreateWebRequest(url, method, null, null, session.Context, (GoogleDocsToken)session.SessionToken, parameters);
            }
            else
            {
                request = WebRequest.Create(url);
                request.Method = method;

                String oAuthHeader = OAuthService.GetOAuthAuthorizationHeader(url, session.Context, (GoogleDocsToken)session.SessionToken, null, method);
                request.Headers.Add("Authorization", oAuthHeader);
            }

            //using API's 3.0 version
            request.Headers.Add("GData-Version", configuration.GDataVersion);

            return request;
        }

        private static OAuthService OAuthService
        {
            get { return new OAuthService(); }
        }

        #endregion
    }
}