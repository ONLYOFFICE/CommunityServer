using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using AppLimit.CloudComputing.SharpBox.Common.IO;
using AppLimit.CloudComputing.SharpBox.Common.Net;
using AppLimit.CloudComputing.SharpBox.Common.Net.Web;
using AppLimit.CloudComputing.SharpBox.Common.Net.Web.Dav;
using AppLimit.CloudComputing.SharpBox.Exceptions;
using AppLimit.CloudComputing.SharpBox.StorageProvider.API;
using AppLimit.CloudComputing.SharpBox.StorageProvider.BaseObjects;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.WebDav.Logic
{
    internal class WebDavStorageProviderService : GenericStorageProviderService
    {
        #region IStorageProviderService Members

        /// <summary>
        /// Verifies if the given credentials are valid webdav credentials
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public override bool VerifyAccessTokenType(ICloudStorageAccessToken token)
        {
            return (token is ICredentials);
        }

        /// <summary>
        /// This method generates a session to a webdav share via username and password
        /// </summary>
        /// <param name="token"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public override IStorageProviderSession CreateSession(ICloudStorageAccessToken token, ICloudStorageConfiguration configuration)
        {
            // cast the creds to the right type            
            var config = configuration as WebDavConfiguration;

            // build service
            var svc = new DavService();

            // check if url available                        
            int status;
            WebException e;
            svc.PerformSimpleWebCall(config.ServiceLocator.ToString(), WebRequestMethodsEx.WebDAV.Options, (token as ICredentials).GetCredential(null, null), null, out status, out e);
            if (status == (int)HttpStatusCode.Unauthorized)
                throw new UnauthorizedAccessException();
            if (HttpUtilityEx.IsSuccessCode(status))
                return new WebDavStorageProviderSession(token, config, this);
            return null;
        }

        /// <summary>
        /// This method request information about a resource
        /// </summary>
        /// <param name="session"></param>
        /// <param name="name"></param>        
        /// <param name="parent"></param>
        /// <returns></returns>
        public override ICloudFileSystemEntry RequestResource(IStorageProviderSession session, string name, ICloudDirectoryEntry parent)
        {
            // build url
            var uriString = GetResourceUrl(session, parent, null);
            uriString = PathHelper.Combine(uriString, name);

            // get the data
            List<BaseFileEntry> childs;
            var requestResource = RequestResourceFromWebDavShare(session, uriString, out childs);

            // check errors
            if (requestResource == null)
                return null;

            // rename the root
            if (name.Equals("/"))
                requestResource.Name = "/";

            // init parent child relation
            if (parent != null)
            {
                var parentDir = parent as BaseDirectoryEntry;
                parentDir.AddChild(requestResource);
            }

            // check if we have to add childs
            if (!(requestResource is BaseDirectoryEntry))
                return requestResource;

            var requestedDir = requestResource as BaseDirectoryEntry;

            // add the childs
            foreach (var child in childs)
            {
                requestedDir.AddChild(child);
            }

            // go ahead
            return requestResource;
        }

        public override void RefreshResource(IStorageProviderSession session, ICloudFileSystemEntry resource)
        {
            // nothing to do for files
            if (!(resource is ICloudDirectoryEntry))
                return;

            // build url
            var uriString = GetResourceUrl(session, resource, null);

            // get the data
            List<BaseFileEntry> childs;
            RequestResourceFromWebDavShare(session, uriString, out childs);

            // set the new childs collection
            var dirEntry = resource as BaseDirectoryEntry;
            dirEntry.ClearChilds();

            // add the new childs
            if (childs != null)
                dirEntry.AddChilds(childs);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="session"></param>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public override ICloudFileSystemEntry CreateResource(IStorageProviderSession session, string name, ICloudDirectoryEntry parent)
        {
            // get the credentials
            var creds = session.SessionToken as ICredentials;

            // build url
            var uriString = GetResourceUrl(session, parent, null);
            uriString = PathHelper.Combine(uriString, name);

            var uri = new Uri(uriString);

            // build the DavService
            var svc = new DavService();

            // create the webrequest            
            int errorCode;
            WebException e;
            svc.PerformSimpleWebCall(uri.ToString(), WebRequestMethodsEx.Http.MkCol, creds.GetCredential(null, null), null, out errorCode, out e);
            if (errorCode != (int)HttpStatusCode.Created)
                return null;

            var newDir = new BaseDirectoryEntry(name, 0, DateTime.Now, this, session);

            // init parent child relation
            if (parent != null)
            {
                var parentDir = parent as BaseDirectoryEntry;
                parentDir.AddChild(newDir);
            }

            return newDir;
        }

        /// <summary>
        /// This method removes a specific resource from a webdav share
        /// </summary>
        /// <param name="session"></param>
        /// <param name="entry"></param>
        public override Boolean DeleteResource(IStorageProviderSession session, ICloudFileSystemEntry entry)
        {
            // get the credentials
            var creds = session.SessionToken as ICredentials;

            // build url            
            var uriString = GetResourceUrl(session, entry, null);
            var uri = new Uri(uriString);

            // create the service
            var svc = new DavService();

            // create the webrequest            
            int errorCode;
            WebException e;
            svc.PerformSimpleWebCall(uri.ToString(), WebRequestMethodsEx.WebDAV.Delete, creds.GetCredential(null, null), null, out errorCode, out e);
            if (!HttpUtilityEx.IsSuccessCode(errorCode))
                return false;

            // remove from parent 
            var parentDir = entry.Parent as BaseDirectoryEntry;
            if (parentDir != null)
            {
                parentDir.RemoveChildById(entry.Id);
            }

            return true;

        }

        /// <summary>
        /// This method moves a resource from one webdav location to an other
        /// </summary>
        /// <param name="session"></param>
        /// <param name="fsentry"></param>
        /// <param name="newParent"></param>
        /// <returns></returns>
        public override bool MoveResource(IStorageProviderSession session, ICloudFileSystemEntry fsentry, ICloudDirectoryEntry newParent)
        {
            // build the targte url            
            var uriStringTarget = GetResourceUrl(session, newParent, null);
            uriStringTarget = PathHelper.Combine(uriStringTarget, fsentry.Name);

            if (!MoveResource(session, fsentry, uriStringTarget))
                return false;

            // readjust parent
            var oldParent = fsentry.Parent as BaseDirectoryEntry;
            if (oldParent != null)
            {
                oldParent.RemoveChild(fsentry as BaseFileEntry);
            }

            var newParentObject = newParent as BaseDirectoryEntry;
            if (newParentObject != null)
            {
                newParentObject.AddChild(fsentry as BaseFileEntry);
            }

            return true;
        }

        public override bool CopyResource(IStorageProviderSession session, ICloudFileSystemEntry fsentry, ICloudDirectoryEntry newParent)
        {
            // build the targte url            
            var uriStringTarget = GetResourceUrl(session, newParent, null);
            uriStringTarget = PathHelper.Combine(uriStringTarget, fsentry.Name);

            if (!CopyResource(session, fsentry, uriStringTarget))
                return false;

            var newParentObject = newParent as BaseDirectoryEntry;
            if (newParentObject != null)
            {
                newParentObject.AddChild(fsentry as BaseFileEntry);
            }

            return true;
        }

        /// <summary>
        /// Renames a webdave file or folder
        /// </summary>
        /// <param name="session"></param>
        /// <param name="fsentry"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        public override bool RenameResource(IStorageProviderSession session, ICloudFileSystemEntry fsentry, string newName)
        {
            // build the targte url
            var uriStringTarget = GetResourceUrl(session, fsentry.Parent, null);
            uriStringTarget = PathHelper.Combine(uriStringTarget, newName);

            if (MoveResource(session, fsentry, uriStringTarget))
            {
                // rename the fsentry
                var fentry = fsentry as BaseFileEntry;
                fentry.Name = newName;

                // go ahead
                return true;
            }

            // go ahead
            return false;
        }

        #endregion

        public override Stream CreateDownloadStream(IStorageProviderSession session, ICloudFileSystemEntry fileSystemEntry)
        {
            // build the url 
            var url = GetResourceUrl(session, fileSystemEntry, null);

            // get the session creds
            var creds = session.SessionToken as ICredentials;

            // Build the service
            var svc = new DavService();

            // create the response
            var response = CreateDownloadResponse(url, creds.GetCredential(null, null));

            // get the data 
            var orgStream = svc.GetResponseStream(response);

            var dStream = new BaseFileEntryDownloadStream(orgStream, fileSystemEntry);

            // put the disposable on the stack
            dStream._DisposableObjects.Push(response);

            // go ahead
            return dStream;
        }

        private WebResponse CreateDownloadResponse(string url, ICredentials creds)
        {
            // Build the service
            var svc = new DavService();

            // create the webrequest
            var request = svc.CreateWebRequest(url, WebRequestMethodsEx.Http.Get, creds, false, null);

            try
            {
                // create the response
                var response = svc.GetWebResponse(request);
                return response;
            }
            catch (WebException e)
            {
                if (e.Response is HttpWebResponse)
                {
                    var code = (e.Response as HttpWebResponse).StatusCode;
                    if (code == HttpStatusCode.Moved)
                    {
                        // get the new uri
                        var newUri = e.Response.Headers["Location"];

                        // redo it
                        return CreateDownloadResponse(newUri, creds);
                    }

                    NetworkCredential networkCredential;
                    if (code == HttpStatusCode.Unauthorized && (networkCredential = creds as NetworkCredential) != null)
                    {
                        var search = new Regex(@"^\w+", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                        // get authentication method
                        var authMethod = search.Match(e.Response.Headers["WWW-Authenticate"]).Value;
                        var newCredentials = new CredentialCache { { new Uri((new Uri(url)).GetLeftPart(UriPartial.Authority)), authMethod, networkCredential } };

                        // redo it
                        return CreateDownloadResponse(url, newCredentials);
                    }
                }

                throw;
            }
        }

        public override Stream CreateUploadStream(IStorageProviderSession session, ICloudFileSystemEntry fileSystemEntry, long uploadSize)
        {
            // build the url 
            var url = GetResourceUrl(session, fileSystemEntry, null);

            // get the session creds
            var creds = session.SessionToken as ICredentials;

            // Build the service
            var svc = new DavService();

            // get the service config
            var conf = (WebDavConfiguration)session.ServiceConfiguration;

            // build the webrequest                        
            var networkRequest = svc.CreateWebRequestPUT(url, creds.GetCredential(null, null), conf.UploadDataStreambuffered);

            // get the request stream
            var requestStream = svc.GetRequestStream(networkRequest, uploadSize);

            // add disposal opp
            requestStream.PushPostDisposeOperation(CommitUploadStream, svc, networkRequest, fileSystemEntry, requestStream);

            // go ahead
            return requestStream;
        }

        public override bool SupportsDirectRetrieve
        {
            get { return false; }
        }

        public void CommitUploadStream(params object[] arg)
        {
            // convert the args
            var svc = arg[0] as DavService;
            var uploadRequest = arg[1] as HttpWebRequest;
            var fileSystemEntry = arg[2] as BaseFileEntry;

            var requestStream = arg[3] as WebRequestStream;

            // check if all data was written into stream
            if (requestStream.WrittenBytes != uploadRequest.ContentLength)
                // nothing todo request was aborted
                return;

            // perform the request
            int code;
            WebException e;
            svc.PerformWebRequest(uploadRequest, null, out code, out e);

            // check the ret value
            if (!HttpUtilityEx.IsSuccessCode(code))
                SharpBoxException.ThrowSharpBoxExceptionBasedOnHttpErrorCode(uploadRequest, (HttpStatusCode)code, e);

            // adjust the lengt
            fileSystemEntry.Length = uploadRequest.ContentLength;
        }

        public override void CommitStreamOperation(IStorageProviderSession session, ICloudFileSystemEntry fileSystemEntry, nTransferDirection direction, Stream notDisposedStream)
        {
        }

        #region Helper

        private static bool MoveResource(IStorageProviderSession session, ICloudFileSystemEntry fsentry, String newTargetUrl)
        {
            var config = session.ServiceConfiguration as WebDavConfiguration;
            var creds = session.SessionToken as ICredentials;

            var uriString = PathHelper.Combine(config.ServiceLocator.ToString(), GenericHelper.GetResourcePath(fsentry));

            var uri = new Uri(uriString);
            var uriTarget = new Uri(newTargetUrl);

            var errorCode = new DavService().PerformMoveWebRequest(uri.ToString(), uriTarget.ToString(), creds.GetCredential(null, null));

            return errorCode == HttpStatusCode.Created || errorCode == HttpStatusCode.NoContent;
        }

        private bool CopyResource(IStorageProviderSession session, ICloudFileSystemEntry fsentry, String newTargetUrl)
        {
            var config = session.ServiceConfiguration as WebDavConfiguration;
            var creds = session.SessionToken as ICredentials;

            var uriString = PathHelper.Combine(config.ServiceLocator.ToString(), GenericHelper.GetResourcePath(fsentry));

            var uri = new Uri(uriString);
            var uriTarget = new Uri(newTargetUrl);

            var errorCode = new DavService().PerformCopyWebRequest(uri.ToString(), uriTarget.ToString(), creds.GetCredential(null, null));

            return errorCode == HttpStatusCode.Created || errorCode == HttpStatusCode.NoContent
                   || errorCode == HttpStatusCode.Accepted && uriString.StartsWith(WebDavConfiguration.YaUrl);
        }

        private BaseFileEntry RequestResourceFromWebDavShare(IStorageProviderSession session, String resourceUrl, out List<BaseFileEntry> childs)
        {
            // get the credebtials
            var creds = session.SessionToken as ICredentials;
            return RequestResourceFromWebDavShare(session, creds.GetCredential(null, null), resourceUrl, out childs);
        }

        private BaseFileEntry RequestResourceFromWebDavShare(IStorageProviderSession session, ICredentials creds, String resourceUrl, out List<BaseFileEntry> childs)
        {
            // build the dav service
            var svc = new DavService();

            try
            {
                // create the web request
                var request = svc.CreateWebRequestPROPFIND(resourceUrl, creds);

                // the result
                WebDavRequestResult requestResult;

                // get the response
                using (var response = svc.GetWebResponse(request) as HttpWebResponse)
                {
                    if (response.StatusCode == HttpStatusCode.Moved)
                    {
                        // get the new uri
                        var newUri = response.Headers["Location"];
                        (session.ServiceConfiguration as WebDavConfiguration).ServiceLocator = new Uri(newUri);

                        // close the response
                        response.Close();

                        // redo it
                        return RequestResourceFromWebDavShare(session, creds, newUri, out childs);
                    }

                    // get the response stream
                    using (var data = svc.GetResponseStream(response))
                    {
                        if (data == null)
                        {
                            childs = null;
                            return null;
                        }

                        // build the file entries
                        requestResult = WebDavRequestParser.CreateObjectsFromNetworkStream(data, resourceUrl, this, session, WebDavNameBaseFilterCallback);

                        // close the stream
                        data.Close();
                    }

                    // close the response
                    response.Close();
                }

                // get the request fileentry and fill the childs
                if (requestResult.Self == null)
                {
                    childs = null;
                    return null;
                }

                // set the childs
                childs = requestResult.Childs;

                // go ahead
                return requestResult.Self;
            }
            catch (WebException e)
            {
                if (e.Response is HttpWebResponse)
                {
                    var code = (e.Response as HttpWebResponse).StatusCode;
                    if (code == HttpStatusCode.Moved)
                    {
                        // get the new uri
                        var newUri = e.Response.Headers["Location"];
                        (session.ServiceConfiguration as WebDavConfiguration).ServiceLocator = new Uri(newUri);

                        // redo it
                        return RequestResourceFromWebDavShare(session, creds, newUri, out childs);
                    }

                    NetworkCredential networkCredential;
                    if (code == HttpStatusCode.Unauthorized && (networkCredential = creds as NetworkCredential) != null)
                    {
                        // get authentication method
                        var headers = e.Response.Headers["WWW-Authenticate"];
                        if (!string.IsNullOrEmpty(headers))
                        {
                            var search = new Regex(@"^\w+", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                            var authMethod = search.Match(headers).Value;
                            var newCredentials = new CredentialCache { { new Uri((new Uri(resourceUrl)).GetLeftPart(UriPartial.Authority)), authMethod, networkCredential } };

                            // redo it
                            return RequestResourceFromWebDavShare(session, newCredentials, resourceUrl, out childs);
                        }
                    }
                }

                childs = null;
                return null;
            }
        }

        private String WebDavNameBaseFilterCallback(String targetUrl, IStorageProviderService service, IStorageProviderSession session, String NameBase)
        {
            // call our virtual method
            return OnNameBase(targetUrl, service, session, NameBase);
        }

        protected virtual String OnNameBase(String targetUrl, IStorageProviderService service, IStorageProviderSession session, String nameBase)
        {
            return nameBase;
        }

        #endregion
    }
}