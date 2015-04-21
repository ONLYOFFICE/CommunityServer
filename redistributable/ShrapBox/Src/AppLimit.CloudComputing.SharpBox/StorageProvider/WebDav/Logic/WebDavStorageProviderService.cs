using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using AppLimit.CloudComputing.SharpBox.Common.IO;
using AppLimit.CloudComputing.SharpBox.Common.Net;
using AppLimit.CloudComputing.SharpBox.Common.Net.Web;
using AppLimit.CloudComputing.SharpBox.Common.Net.Web.Dav;
using AppLimit.CloudComputing.SharpBox.StorageProvider.API;
using AppLimit.CloudComputing.SharpBox.StorageProvider.BaseObjects;
using AppLimit.CloudComputing.SharpBox.Exceptions;

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
            WebDavConfiguration config = configuration as WebDavConfiguration;

            // build service
            DavService svc = new DavService();

            // check if url available                        
            int status = (int) HttpStatusCode.OK;
            WebException e = null;
            svc.PerformSimpleWebCall(config.ServiceLocator.ToString(), WebRequestMethodsEx.WebDAV.Options, (token as ICredentials).GetCredential(null, null), null, out status, out e);
            if (status == (int) HttpStatusCode.Unauthorized)
                throw new UnauthorizedAccessException();
            else if (HttpUtilityEx.IsSuccessCode(status))
                return new WebDavStorageProviderSession(token, config, this);
            else
                return null;
        }

        /// <summary>
        /// This method request information about a resource
        /// </summary>
        /// <param name="session"></param>
        /// <param name="Name"></param>        
        /// <param name="parent"></param>
        /// <returns></returns>
        public override ICloudFileSystemEntry RequestResource(IStorageProviderSession session, string Name, ICloudDirectoryEntry parent)
        {
            // build url
            String uriString = GetResourceUrl(session, parent, null);
            uriString = PathHelper.Combine(uriString, Name);

            // get the data
            List<BaseFileEntry> childs = null;
            BaseFileEntry requestResource = RequestResourceFromWebDavShare(session, uriString, out childs);

            // check errors
            if (requestResource == null)
                return null;

            // rename the root
            if (Name.Equals("/"))
                requestResource.Name = "/";

            // init parent child relation
            if (parent != null)
            {
                BaseDirectoryEntry parentDir = parent as BaseDirectoryEntry;
                parentDir.AddChild(requestResource);
            }

            // check if we have to add childs
            if (!(requestResource is BaseDirectoryEntry))
                return requestResource;
            else
            {
                BaseDirectoryEntry requestedDir = requestResource as BaseDirectoryEntry;

                // add the childs
                foreach (BaseFileEntry child in childs)
                {
                    requestedDir.AddChild(child);
                }
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
            String uriString = GetResourceUrl(session, resource, null);

            // get the data
            List<BaseFileEntry> childs = null;
            RequestResourceFromWebDavShare(session, uriString, out childs);

            // set the new childs collection
            BaseDirectoryEntry dirEntry = resource as BaseDirectoryEntry;
            dirEntry.ClearChilds();

            // add the new childs
            if (childs != null)
                dirEntry.AddChilds(childs);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="session"></param>
        /// <param name="Name"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public override ICloudFileSystemEntry CreateResource(IStorageProviderSession session, string Name, ICloudDirectoryEntry parent)
        {
            // get the credentials
            ICredentials creds = session.SessionToken as ICredentials;

            // build url
            String uriString = GetResourceUrl(session, parent, null);
            uriString = PathHelper.Combine(uriString, Name);

            Uri uri = new Uri(uriString);

            // build the DavService
            DavService svc = new DavService();

            // create the webrequest            
            int errorCode;
            WebException e;
            svc.PerformSimpleWebCall(uri.ToString(), WebRequestMethodsEx.Http.MkCol, creds.GetCredential(null, null), null, out errorCode, out e);
            if (errorCode != (int) HttpStatusCode.Created)
                return null;
            else
            {
                BaseDirectoryEntry newDir = new BaseDirectoryEntry(Name, 0, DateTime.Now, this, session);

                // init parent child relation
                if (parent != null)
                {
                    BaseDirectoryEntry parentDir = parent as BaseDirectoryEntry;
                    parentDir.AddChild(newDir);
                }

                return newDir;
            }
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
            String uriString = this.GetResourceUrl(session, entry, null);
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
            String uriStringTarget = GetResourceUrl(session, newParent, null);
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
            String uriStringTarget = this.GetResourceUrl(session, newParent, null);
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
            String uriStringTarget = this.GetResourceUrl(session, fsentry.Parent, null);
            uriStringTarget = PathHelper.Combine(uriStringTarget, newName);

            if (MoveResource(session, fsentry, uriStringTarget))
            {
                // rename the fsentry
                BaseFileEntry fentry = fsentry as BaseFileEntry;
                fentry.Name = newName;

                // go ahead
                return true;
            }
            else
                // go ahead
                return false;
        }

        #endregion

        public override Stream CreateDownloadStream(IStorageProviderSession session, ICloudFileSystemEntry fileSystemEntry)
        {
            // build the url 
            string url = GetResourceUrl(session, fileSystemEntry, null);

            // get the session creds
            ICredentials creds = session.SessionToken as ICredentials;

            // Build the service
            DavService svc = new DavService();

            // create the webrequest
            WebRequest request = svc.CreateWebRequest(url, WebRequestMethodsEx.Http.Get, creds.GetCredential(null, null), false, null);

            // create the response
            WebResponse response = svc.GetWebResponse(request);

            // get the data 
            Stream orgStream = svc.GetResponseStream(response);

            BaseFileEntryDownloadStream dStream = new BaseFileEntryDownloadStream(orgStream, fileSystemEntry);

            // put the disposable on the stack
            dStream._DisposableObjects.Push(response);

            // go ahead
            return dStream;
        }

        public override Stream CreateUploadStream(IStorageProviderSession session, ICloudFileSystemEntry fileSystemEntry, long uploadSize)
        {
            // build the url 
            string url = GetResourceUrl(session, fileSystemEntry, null);

            // get the session creds
            ICredentials creds = session.SessionToken as ICredentials;

            // Build the service
            DavService svc = new DavService();

            // get the service config
            WebDavConfiguration conf = (WebDavConfiguration) session.ServiceConfiguration;

            // build the webrequest                        
            WebRequest networkRequest = svc.CreateWebRequestPUT(url, creds.GetCredential(null, null), conf.UploadDataStreambuffered);

            // get the request stream
            WebRequestStream requestStream = svc.GetRequestStream(networkRequest, uploadSize);

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
            DavService svc = arg[0] as DavService;
            HttpWebRequest uploadRequest = arg[1] as HttpWebRequest;
            BaseFileEntry fileSystemEntry = arg[2] as BaseFileEntry;

#if !WINDOWS_PHONE && !MONODROID
            WebRequestStream requestStream = arg[3] as WebRequestStream;

            // check if all data was written into stream
            if (requestStream.WrittenBytes != uploadRequest.ContentLength)
                // nothing todo request was aborted
                return;
#endif

            // perform the request
            int code;
            WebException e;
            svc.PerformWebRequest(uploadRequest, null, out code, out e);

            // check the ret value
            if (!HttpUtilityEx.IsSuccessCode(code))
                SharpBoxException.ThrowSharpBoxExceptionBasedOnHttpErrorCode(uploadRequest, (HttpStatusCode) code, e);

            // adjust the lengt
#if !WINDOWS_PHONE && !MONODROID
            fileSystemEntry.Length = uploadRequest.ContentLength;
#endif
        }

        public override void CommitStreamOperation(IStorageProviderSession session, ICloudFileSystemEntry fileSystemEntry, nTransferDirection Direction, Stream NotDisposedStream)
        {

        }

        #region Helper

        private bool MoveResource(IStorageProviderSession session, ICloudFileSystemEntry fsentry, String newTargetUrl)
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

            return errorCode == HttpStatusCode.Created || errorCode == HttpStatusCode.NoContent;
        }

        private BaseFileEntry RequestResourceFromWebDavShare(IStorageProviderSession session, String resourceUrl, out List<BaseFileEntry> childs)
        {
            // get the credebtials
            ICredentials creds = session.SessionToken as ICredentials;

            // build the dav service
            DavService svc = new DavService();

            // the result
            WebDavRequestResult RequestResult;

            try
            {
                // create the web request
                WebRequest request = svc.CreateWebRequestPROPFIND(resourceUrl, creds.GetCredential(null, null));

                // get the response
                using (HttpWebResponse response = svc.GetWebResponse(request) as HttpWebResponse)
                {
                    if (response.StatusCode == HttpStatusCode.Moved)
                    {
                        // get the new uri
                        String newUri = response.Headers["Location"];

                        // close the response
                        response.Close();

                        // redo it
                        return RequestResourceFromWebDavShare(session, newUri, out childs);
                    }
                    else
                    {
                        // get the response stream
                        using (Stream data = svc.GetResponseStream(response))
                        {
                            if (data == null)
                            {
                                childs = null;
                                return null;
                            }

                            // build the file entries
                            RequestResult = WebDavRequestParser.CreateObjectsFromNetworkStream(data, resourceUrl, this, session, WebDavNameBaseFilterCallback);

                            // close the stream
                            data.Close();
                        }

                        // close the response
                        response.Close();
                    }
                }

                // get the request fileentry and fill the childs
                if (RequestResult.Self == null)
                {
                    childs = null;
                    return null;
                }

                // set the childs
                childs = RequestResult.Childs;

                // go ahead
                return RequestResult.Self;
            }
            catch (WebException)
            {
                childs = null;
                return null;
            }
        }

        private String WebDavNameBaseFilterCallback(String targetUrl, IStorageProviderService service, IStorageProviderSession session, String NameBase)
        {
            // call our virtual method
            return OnNameBase(targetUrl, service, session, NameBase);
        }

        protected virtual String OnNameBase(String targetUrl, IStorageProviderService service, IStorageProviderSession session, String NameBase)
        {
            return NameBase;
        }

        #endregion
    }
}