// ----------------------------------------------------------------------------------------
// FTP Provider Functions - by Fungusware [www.fungusware.com]
// ----------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Net;
using AppLimit.CloudComputing.SharpBox.Common.IO;
using AppLimit.CloudComputing.SharpBox.StorageProvider.API;
using AppLimit.CloudComputing.SharpBox.StorageProvider.BaseObjects;
using AppLimit.CloudComputing.SharpBox.Common.Net.Web.Ftp;
using AppLimit.CloudComputing.SharpBox.Common.Net.Web;
using System.Text.RegularExpressions;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.FTP.Logic
{
    internal class FtpStorageProviderService : GenericStorageProviderService
    {
        private FtpService _ftpService = new FtpService();

        public override bool VerifyAccessTokenType(ICloudStorageAccessToken token)
        {
            return (token is ICredentials);
        }

        public override IStorageProviderSession CreateSession(ICloudStorageAccessToken token, ICloudStorageConfiguration configuration)
        {
            // cast the creds to the right type
            GenericNetworkCredentials creds = token as GenericNetworkCredentials;

            // check if url available                        
            int status = (int) FtpStatusCode.CommandOK;
            WebException e = null;
            _ftpService.PerformSimpleWebCall(configuration.ServiceLocator.ToString(), WebRequestMethods.Ftp.ListDirectory, creds.GetCredential(null, null), null, out status, out e);
            if (status == (int) FtpStatusCode.NotLoggedIn)
                throw new UnauthorizedAccessException();
            else if (status >= 100 && status < 400)
                return new FtpStorageProviderSession(token, configuration as FtpConfiguration, this);
            else
                return null;
        }

        public override ICloudFileSystemEntry RequestResource(IStorageProviderSession session, string Name, ICloudDirectoryEntry parent)
        {
            //declare the requested entry
            ICloudFileSystemEntry fsEntry = null;

            // lets have a look if we are on the root node
            if (parent == null)
            {
                // just create the root entry
                fsEntry = GenericStorageProviderFactory.CreateDirectoryEntry(session, Name, parent);
            }
            else
            {
                // ok we have a parent, let's retrieve the resource 
                // from his child list
                fsEntry = parent.GetChild(Name, false);
            }

            // now that we create the entry just update the chuld
            if (fsEntry != null && fsEntry is ICloudDirectoryEntry)
                RefreshResource(session, fsEntry as ICloudDirectoryEntry);

            // go ahead
            return fsEntry;
        }

        public override void RefreshResource(IStorageProviderSession session, ICloudFileSystemEntry resource)
        {
            // nothing to do for files
            if (!(resource is ICloudDirectoryEntry))
                return;

            // Refresh schild
            RefreshChildsOfDirectory(session, resource as ICloudDirectoryEntry);
        }

        public override bool DeleteResource(IStorageProviderSession session, ICloudFileSystemEntry entry)
        {
            // get the creds
            ICredentials creds = ((GenericNetworkCredentials) session.SessionToken).GetCredential(null, null);

            // generate the loca path
            String uriPath = GetResourceUrl(session, entry, null);

            // removed the file
            if (entry is ICloudDirectoryEntry)
            {
                // we need an empty directory
                foreach (ICloudFileSystemEntry child in (ICloudDirectoryEntry) entry)
                {
                    DeleteResource(session, child);
                }

                // remove the directory
                return _ftpService.FtpDeleteEmptyDirectory(uriPath, creds);
            }
            else
            {
                // remove the file
                return _ftpService.FtpDeleteFile(uriPath, creds);
            }
        }

        public override bool MoveResource(IStorageProviderSession session, ICloudFileSystemEntry fsentry, ICloudDirectoryEntry newParent)
        {
            // get credentials
            ICredentials creds = ((GenericNetworkCredentials) session.SessionToken).GetCredential(null, null);

            // get old name uri
            String uriPath = GetResourceUrl(session, fsentry.Parent, fsentry.Name);

            // get the new path
            String TargetPath = PathHelper.Combine(GenericHelper.GetResourcePath(newParent), fsentry.Name);

            // do it 
            if (!_ftpService.FtpRename(uriPath, TargetPath, creds))
                return false;

            // remove from parent
            (fsentry.Parent as BaseDirectoryEntry).RemoveChild(fsentry as BaseFileEntry);

            // add to new parent
            (newParent as BaseDirectoryEntry).AddChild(fsentry as BaseFileEntry);

            return true;
        }

        public override Stream CreateDownloadStream(IStorageProviderSession session, ICloudFileSystemEntry fileSystemEntry)
        {
            // get the session creds
            ICredentials creds = session.SessionToken as ICredentials;

            // get the full path
            String uriPath = GetResourceUrl(session, fileSystemEntry, null);

            // get the ftp request
            FtpWebRequest ftp = (FtpWebRequest) _ftpService.CreateWebRequest(uriPath, WebRequestMethodsEx.Ftp.DownloadFile, creds, false, null);

            // set request to download a file in binary mode            
            ftp.UseBinary = true;

            // create the response
            WebResponse response = _ftpService.GetWebResponse(ftp);

            // get the data 
            Stream orgStream = _ftpService.GetResponseStream(response);

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

            // build the webrequest                        
            FtpWebRequest networkRequest = (FtpWebRequest) _ftpService.CreateWebRequest(url, WebRequestMethodsEx.Ftp.UploadFile, creds, false, null);

            // set the binary mode            
            networkRequest.UseBinary = true;

            // Notify FTP of the expected size
            networkRequest.ContentLength = uploadSize;

            // get the request stream
            WebRequestStream requestStream = _ftpService.GetRequestStream(networkRequest, uploadSize);

            // add disposal opp
            requestStream.PushPostDisposeOperation(CommitUploadStream, _ftpService, networkRequest, fileSystemEntry, requestStream);

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
            // FtpService svc = arg[0] as FtpService;
            FtpWebRequest uploadRequest = arg[1] as FtpWebRequest;
            BaseFileEntry fileSystemEntry = arg[2] as BaseFileEntry;

#if !WINDOWS_PHONE && !ANDROID
            WebRequestStream requestStream = arg[3] as WebRequestStream;

            // close the stream
            requestStream.Close();

            // close conncetion
            uploadRequest.Abort();

            // check if all data was written into stream
            if (requestStream.WrittenBytes != uploadRequest.ContentLength)
                // nothing todo request was aborted
                return;
#endif
            // adjust the lengt
#if !WINDOWS_PHONE && !ANDROID
            fileSystemEntry.Length = uploadRequest.ContentLength;
#endif
        }

        public override void CommitStreamOperation(IStorageProviderSession session, ICloudFileSystemEntry fileSystemEntry, nTransferDirection Direction, Stream NotDisposedStream)
        {

        }

        public override ICloudFileSystemEntry CreateResource(IStorageProviderSession session, string Name, ICloudDirectoryEntry parent)
        {
            // get credentials
            ICredentials creds = ((GenericNetworkCredentials) session.SessionToken).GetCredential(null, null);

            // build the full url
            String resFull = GetResourceUrl(session, parent, Name);

            // create the director
            if (_ftpService.FtpCreateDirectory(resFull, creds))
            {
                // create the filesystem object
                ICloudDirectoryEntry fsEntry = GenericStorageProviderFactory.CreateDirectoryEntry(session, Name, parent);

                // go ahead
                return fsEntry;
            }
            else
                return null;
        }

        public override bool RenameResource(IStorageProviderSession session, ICloudFileSystemEntry fsentry, string newName)
        {
            // get credentials
            ICredentials creds = ((GenericNetworkCredentials) session.SessionToken).GetCredential(null, null);

            // get old name uri
            String uriPath = GetResourceUrl(session, fsentry.Parent, fsentry.Name);

            // get the new path
            String TargetPath = PathHelper.Combine(GenericHelper.GetResourcePath(fsentry.Parent), newName);

            // do it 
            if (!_ftpService.FtpRename(uriPath, TargetPath, creds))
                return false;

            // rename the entry            
            BaseFileEntry fentry = fsentry as BaseFileEntry;
            fentry.Name = newName;

            // go ahead
            return true;
        }

        #region Helper

        private void RefreshChildsOfDirectory(IStorageProviderSession session, ICloudDirectoryEntry dir)
        {
            // get the creds
            ICredentials creds = ((GenericNetworkCredentials) session.SessionToken).GetCredential(null, null);

            // get the uri
            String resSource = GetResourceUrl(session, dir.Parent, dir.Name);

            // clear all childs
            GenericStorageProviderFactory.ClearAllChilds(dir);

            // we need to request a directory list here 
            using (MemoryStream data = _ftpService.PerformSimpleWebCall(resSource, WebRequestMethodsEx.Ftp.ListDirectoryDetails, creds, null))
            {
                using (StreamReader r = new StreamReader(data))
                {
                    while (!r.EndOfStream)
                    {
                        String ftpline = r.ReadLine();

                        Match m = GetMatchingRegexFromFTPLine(ftpline);

                        if (m == null)
                        {
                            //failed
                            throw new ApplicationException("Unable to parse line: " + ftpline);
                        }
                        else
                        {
                            // get the filename
                            String filename = m.Groups["name"].Value;

                            // get teh modified date
                            DateTime fileDateTime = DateTime.MinValue;
                            try
                            {
                                fileDateTime = System.DateTime.Parse(m.Groups["timestamp"].Value);
                            }
                            catch (Exception)
                            {
                            }

                            // evaluate if we have a directory
                            string _dir = m.Groups["dir"].Value;
                            if ((!string.IsNullOrEmpty(_dir) & _dir != "-"))
                            {
                                GenericStorageProviderFactory.CreateDirectoryEntry(session, filename, fileDateTime, dir);
                            }
                            else
                            {
                                // get the size 
                                long size = Convert.ToInt64(m.Groups["size"].Value);

                                // create the file object 
                                GenericStorageProviderFactory.CreateFileSystemEntry(session, filename, fileDateTime, size, dir);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// List of REGEX formats for different FTP server listing formats
        /// </summary>
        /// <remarks>
        /// The first three are various UNIX/LINUX formats, fourth is for MS FTP
        /// in detailed mode and the last for MS FTP in 'DOS' mode.
        /// I wish VB.NET had support for Const arrays like C# but there you go
        /// </remarks>
        private static string[] _ParseFormats = {
                                                    "(?<dir>[\\-d])(?<permission>([\\-r][\\-w][\\-xs]){3})\\s+\\d+\\s+\\w+\\s+\\w+\\s+(?<size>\\d+)\\s+(?<timestamp>\\w+\\s+\\d+\\s+\\d{4})\\s+(?<name>.+)",
                                                    "(?<dir>[\\-d])(?<permission>([\\-r][\\-w][\\-xs]){3})\\s+\\d+\\s+\\d+\\s+(?<size>\\d+)\\s+(?<timestamp>\\w+\\s+\\d+\\s+\\d{4})\\s+(?<name>.+)",
                                                    "(?<dir>[\\-d])(?<permission>([\\-r][\\-w][\\-xs]){3})\\s+\\d+\\s+\\d+\\s+(?<size>\\d+)\\s+(?<timestamp>\\w+\\s+\\d+\\s+\\d{1,2}:\\d{2})\\s+(?<name>.+)",
                                                    "(?<dir>[\\-d])(?<permission>([\\-r][\\-w][\\-xs]){3})\\s+\\d+\\s+\\w+\\s+\\w+\\s+(?<size>\\d+)\\s+(?<timestamp>\\w+\\s+\\d+\\s+\\d{1,2}:\\d{2})\\s+(?<name>.+)",
                                                    "(?<dir>[\\-d])(?<permission>([\\-r][\\-w][\\-xs]){3})(\\s+)(?<size>(\\d+))(\\s+)(?<ctbit>(\\w+\\s\\w+))(\\s+)(?<size2>(\\d+))\\s+(?<timestamp>\\w+\\s+\\d+\\s+\\d{2}:\\d{2})\\s+(?<name>.+)",
                                                    "(?<timestamp>\\d{2}\\-\\d{2}\\-\\d{2}\\s+\\d{2}:\\d{2}[Aa|Pp][mM])\\s+(?<dir>\\<\\w+\\>){0,1}(?<size>\\d+){0,1}\\s+(?<name>.+)"
                                                };

        private Match GetMatchingRegexFromFTPLine(string ftpline)
        {
            Regex rx = default(Regex);
            Match m = default(Match);
            for (int i = 0; i <= _ParseFormats.Length - 1; i++)
            {
                rx = new Regex(_ParseFormats[i]);
                m = rx.Match(ftpline);
                if (m.Success)
                    return m;
            }
            return null;
        }

        #endregion
    }
}