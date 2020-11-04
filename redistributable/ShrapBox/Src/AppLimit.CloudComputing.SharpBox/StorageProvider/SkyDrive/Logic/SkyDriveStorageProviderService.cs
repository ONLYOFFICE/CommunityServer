using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using AppLimit.CloudComputing.SharpBox.Common.Net.oAuth20;
using AppLimit.CloudComputing.SharpBox.Common.Net.Web;
using AppLimit.CloudComputing.SharpBox.Exceptions;
using AppLimit.CloudComputing.SharpBox.StorageProvider.API;
using AppLimit.CloudComputing.SharpBox.StorageProvider.BaseObjects;
using AppLimit.CloudComputing.SharpBox.StorageProvider.SkyDrive.Authorization;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.SkyDrive.Logic
{
    internal class SkyDriveStorageProviderService : GenericStorageProviderService
    {
        public void RefreshDirectoryContent(IStorageProviderSession session, ICloudDirectoryEntry directory)
        {
            if (!(directory is BaseDirectoryEntry)) return;
            var uri = String.Format(SkyDriveConstants.FilesAccessUrlFormat, directory.GetPropertyValue(SkyDriveConstants.InnerIDKey));
            (directory as BaseDirectoryEntry).AddChilds(RequestContentByUrl(session, uri).Cast<BaseFileEntry>());
        }

        private static IEnumerable<ICloudFileSystemEntry> RequestContentByUrl(IStorageProviderSession session, String url)
        {
            var json = SkyDriveRequestHelper.PerformRequest(session, url);
            return SkyDriveJsonParser.ParseListOfEntries(session, json) ?? new List<ICloudFileSystemEntry>();
        }

        private static ICloudFileSystemEntry RequestResourseByUrl(IStorageProviderSession session, String url)
        {
            var json = SkyDriveRequestHelper.PerformRequest(session, url);
            return SkyDriveJsonParser.ParseSingleEntry(session, json);
        }

        public override bool VerifyAccessTokenType(ICloudStorageAccessToken token)
        {
            return token is OAuth20Token;
        }

        public override IStorageProviderSession CreateSession(ICloudStorageAccessToken token, ICloudStorageConfiguration configuration)
        {
            var skydriveToken = token as OAuth20Token;
            if (skydriveToken == null) throw new ArgumentException("Cannot create skydrive session with given token", "token");
            if (skydriveToken.IsExpired) token = SkyDriveAuthorizationHelper.RefreshToken(skydriveToken);
            return new SkyDriveStorageProviderSession(token, this, configuration);
        }

        public override ICloudFileSystemEntry RequestResource(IStorageProviderSession session, String name, ICloudDirectoryEntry parent)
        {
            /* In this method name could be either requested resource name or it's ID.
             * In first case just refresh the parent and then search child with an appropriate.
             * In second case it does not matter if parent is null or not a parent because we use only resource ID. */

            if (SkyDriveHelpers.HasResourceID(name) || name.Equals("/") && parent == null)
                //If request by ID or root folder requested
            {
                var id = SkyDriveHelpers.GetResourceID(name);
                var uri = id != String.Empty
                              ? String.Format("{0}/{1}", SkyDriveConstants.BaseAccessUrl, id)
                              : SkyDriveConstants.RootAccessUrl;

                if (SkyDriveHelpers.IsFolderID(id))
                {
                    var contents = new List<ICloudFileSystemEntry>();

                    /*var completeContent = new AutoResetEvent(false);
                    ThreadPool.QueueUserWorkItem(state =>
                        {
                            contents.AddRange(RequestContentByUrl(session, uri + "/files"));
                            ((AutoResetEvent) state).Set();
                        }, completeContent);

                    var completeEntry = new AutoResetEvent(false);
                    ThreadPool.QueueUserWorkItem(state =>
                        {
                            entry = RequestResourseByUrl(session, uri) as BaseDirectoryEntry;
                            ((AutoResetEvent) state).Set();
                        }, completeEntry);

                    WaitHandle.WaitAll(new WaitHandle[] {completeContent, completeEntry});*/

                    var entry = RequestResourseByUrl(session, uri) as BaseDirectoryEntry;
                    contents.AddRange(RequestContentByUrl(session, uri + "/files"));

                    if (entry != null && contents.Any())
                        entry.AddChilds(contents.Cast<BaseFileEntry>());

                    return entry;
                }

                return RequestResourseByUrl(session, uri);
            }
            else
            {
                String uri;
                if (SkyDriveHelpers.HasParentID(name))
                    uri = String.Format(SkyDriveConstants.FilesAccessUrlFormat, SkyDriveHelpers.GetParentID(name));
                else if (parent != null)
                    uri = String.Format(SkyDriveConstants.FilesAccessUrlFormat, parent.GetPropertyValue(SkyDriveConstants.InnerIDKey));
                else
                    uri = SkyDriveConstants.RootAccessUrl + "/files";

                name = Path.GetFileName(name);
                var entry = RequestContentByUrl(session, uri).FirstOrDefault(x => x.Name.Equals(name));
                return entry;
            }
        }

        public override void RefreshResource(IStorageProviderSession session, ICloudFileSystemEntry resource)
        {
            //not refresh if resource was requested recently
            var timestamp = resource.GetPropertyValue(SkyDriveConstants.TimestampKey);
            var refreshNeeded = DateTime.Parse(timestamp, CultureInfo.InvariantCulture) + TimeSpan.FromSeconds(5) < DateTime.UtcNow;
            if (refreshNeeded)
            {
                //Request resource by ID and then update properties from requested
                var current = RequestResource(session, resource.GetPropertyValue(SkyDriveConstants.InnerIDKey), null);
                SkyDriveHelpers.CopyProperties(current, resource);
            }

            var directory = resource as ICloudDirectoryEntry;
            if (directory != null && !refreshNeeded && directory.HasChildrens == nChildState.HasNotEvaluated)
                RefreshDirectoryContent(session, directory);
        }

        public override bool DeleteResource(IStorageProviderSession session, ICloudFileSystemEntry entry)
        {
            var uri = String.Format("{0}/{1}", SkyDriveConstants.BaseAccessUrl, entry.GetPropertyValue(SkyDriveConstants.InnerIDKey));
            var json = SkyDriveRequestHelper.PerformRequest(session, uri, "DELETE", null, true);

            if (!SkyDriveJsonParser.ContainsError(json, false))
            {
                var parent = entry.Parent as BaseDirectoryEntry;
                if (parent != null)
                    parent.RemoveChildById(entry.GetPropertyValue(SkyDriveConstants.InnerIDKey));
                return true;
            }

            return false;
        }

        public override ICloudFileSystemEntry CreateResource(IStorageProviderSession session, String name, ICloudDirectoryEntry parent)
        {
            if (name.Contains("/"))
                throw new SharpBoxException(SharpBoxErrorCodes.ErrorInvalidFileOrDirectoryName);

            var uri =
                parent != null
                    ? String.Format("{0}/{1}", SkyDriveConstants.BaseAccessUrl, parent.GetPropertyValue(SkyDriveConstants.InnerIDKey))
                    : SkyDriveConstants.RootAccessUrl;

            var data = String.Format("{{name: \"{0}\"}}", name);
            var json = SkyDriveRequestHelper.PerformRequest(session, uri, "POST", data, false);
            var entry = SkyDriveJsonParser.ParseSingleEntry(session, json);

            var parentBase = parent as BaseDirectoryEntry;
            if (parentBase != null && entry != null)
                parentBase.AddChild(entry as BaseFileEntry);

            return entry;
        }

        public override bool RenameResource(IStorageProviderSession session, ICloudFileSystemEntry entry, String newName)
        {
            if (entry.Name.Equals("/") || newName.Contains("/"))
                return false;

            var uri = String.Format("{0}/{1}", SkyDriveConstants.BaseAccessUrl, entry.GetPropertyValue(SkyDriveConstants.InnerIDKey));
            var data = String.Format("{{name: \"{0}\"}}", newName);
            var json = SkyDriveRequestHelper.PerformRequest(session, uri, "PUT", data, false);

            if (!SkyDriveJsonParser.ContainsError(json, false))
            {
                var entryBase = entry as BaseFileEntry;
                if (entryBase != null)
                    entryBase.Name = newName;
                return true;
            }

            return false;
        }

        public override bool MoveResource(IStorageProviderSession session, ICloudFileSystemEntry entry, ICloudDirectoryEntry moveTo)
        {
            if (entry.Name.Equals("/"))
                return false;

            var uri = String.Format("{0}/{1}", SkyDriveConstants.BaseAccessUrl, entry.GetPropertyValue(SkyDriveConstants.InnerIDKey));
            var data = String.Format("{{destination: \"{0}\"}}", moveTo.GetPropertyValue(SkyDriveConstants.InnerIDKey));
            var json = SkyDriveRequestHelper.PerformRequest(session, uri, "MOVE", data, false);

            if (!SkyDriveJsonParser.ContainsError(json, false))
            {
                var parent = entry.Parent as BaseDirectoryEntry;
                if (parent != null)
                    parent.RemoveChildById(entry.GetPropertyValue(SkyDriveConstants.InnerIDKey));
                var moveToBase = moveTo as BaseDirectoryEntry;
                if (moveToBase != null)
                    moveToBase.AddChild(entry as BaseFileEntry);
                return true;
            }

            return false;
        }

        public override bool CopyResource(IStorageProviderSession session, ICloudFileSystemEntry entry, ICloudDirectoryEntry copyTo)
        {
            if (entry.Name.Equals("/"))
                return false;

            if (entry is ICloudDirectoryEntry)
            {
                // skydrive allowes to copy only files so we will recursively create/copy entries
                var newEntry = CreateResource(session, entry.Name, copyTo) as ICloudDirectoryEntry;
                return newEntry != null && (entry as ICloudDirectoryEntry).Aggregate(true, (current, subEntry) => current && CopyResource(session, subEntry, newEntry));
            }

            var uri = String.Format("{0}/{1}", SkyDriveConstants.BaseAccessUrl, entry.GetPropertyValue(SkyDriveConstants.InnerIDKey));
            var data = String.Format("{{destination: \"{0}\"}}", copyTo.GetPropertyValue(SkyDriveConstants.InnerIDKey));
            var json = SkyDriveRequestHelper.PerformRequest(session, uri, "COPY", data, false);

            if (json != null && !SkyDriveJsonParser.ContainsError(json, false))
            {
                var copyToBase = copyTo as BaseDirectoryEntry;
                if (copyToBase != null)
                    copyToBase.AddChild(entry as BaseFileEntry);
                return true;
            }

            return false;
        }

        public override Stream CreateDownloadStream(IStorageProviderSession session, ICloudFileSystemEntry entry)
        {
            if (entry is ICloudDirectoryEntry)
                throw new ArgumentException("Download operation can be perform for files only");

            var uri = String.Format("{0}/{1}/content", SkyDriveConstants.BaseAccessUrl, entry.GetPropertyValue(SkyDriveConstants.InnerIDKey));
            uri = SkyDriveRequestHelper.SignUri(session, uri);
            var request = WebRequest.Create(uri);
            using (var response = request.GetResponse())
            {
                ((BaseFileEntry)entry).Length = response.ContentLength;
                return new BaseFileEntryDownloadStream(response.GetResponseStream(), entry);
            }
        }

        public override Stream CreateUploadStream(IStorageProviderSession session, ICloudFileSystemEntry entry, long uploadSize)
        {
            if (entry is ICloudDirectoryEntry)
                throw new ArgumentException("Upload operation can be perform for files only");

            var tempStream = new FileStream(Path.GetTempFileName(), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose);
            var uploadStream = new WebRequestStream(tempStream, null, null);
            uploadStream.PushPreDisposeOperation(CommitUploadOperation, tempStream, uploadSize, session, entry);
            return uploadStream;
        }

        public override void CommitStreamOperation(IStorageProviderSession session, ICloudFileSystemEntry entry, nTransferDirection direction, Stream notDisposedStream)
        {
        }

        public override bool SupportsDirectRetrieve
        {
            get { return true; }
        }

        public override void StoreToken(IStorageProviderSession session, Dictionary<String, String> tokendata, ICloudStorageAccessToken token)
        {
            if (token is OAuth20Token)
            {
                tokendata.Add(SkyDriveConstants.SerializedDataKey, (token as OAuth20Token).ToJson());
            }
        }

        public override ICloudStorageAccessToken LoadToken(Dictionary<String, String> tokendata)
        {
            var type = tokendata[CloudStorage.TokenCredentialType];
            if (type.Equals(typeof (OAuth20Token).ToString()))
            {
                var json = tokendata[SkyDriveConstants.SerializedDataKey];
                return OAuth20Token.FromJson(json);
            }
            return null;
        }

        private static void CommitUploadOperation(object[] args)
        {
            var tempStream = (Stream)args[0];
            var contentLength = (long)args[1];
            var session = (IStorageProviderSession)args[2];
            var file = (BaseFileEntry)args[3];

            var request = (HttpWebRequest)WebRequest.Create(GetSignedUploadUrl(session, file));
            request.Method = "PUT";
            request.ContentLength = contentLength;
            request.Timeout = Timeout.Infinite;

            tempStream.Flush();
            tempStream.Seek(0, SeekOrigin.Begin);

            using (var requestSteam = request.GetRequestStream())
            {
                tempStream.CopyTo(requestSteam);
            }

            tempStream.Close();

            using (var response = request.GetResponse())
            using (var rs = response.GetResponseStream())
            {
                if (rs == null) return;
                string json;
                using (var streamReader = new StreamReader(rs))
                {
                    json = streamReader.ReadToEnd();
                }
                var id = SkyDriveJsonParser.ParseEntryID(json);
                file.Id = id;
                file[SkyDriveConstants.InnerIDKey] = id;
                file.Modified = DateTime.UtcNow;

                var parent = file.Parent as BaseDirectoryEntry;
                if (parent != null)
                {
                    parent.RemoveChildById(file.Name);
                    parent.AddChild(file);
                }
            }

        }

        public override string GetResourceUrl(IStorageProviderSession session, ICloudFileSystemEntry fileSystemEntry, string additionalPath)
        {
            var id = SkyDriveHelpers.GetResourceID(additionalPath);
            if (!String.IsNullOrEmpty(id) || additionalPath != null && additionalPath.Equals("/"))
                return "/" + id;
            if (String.IsNullOrEmpty(additionalPath) && fileSystemEntry != null)
                return "/" + (!fileSystemEntry.Id.Equals("/") ? fileSystemEntry.Id : "");
            return base.GetResourceUrl(session, fileSystemEntry, additionalPath);
        }

        private static string GetSignedUploadUrl(IStorageProviderSession session, ICloudFileSystemEntry fileSystemEntry)
        {
            var uri = String.Format("{0}/{1}/files/{2}",
                                    SkyDriveConstants.BaseAccessUrl,
                                    fileSystemEntry.Parent.GetPropertyValue(SkyDriveConstants.InnerIDKey),
                                    fileSystemEntry.Name);

            return SkyDriveRequestHelper.SignUri(session, uri);
        }
    }
}