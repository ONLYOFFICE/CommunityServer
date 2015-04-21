using System;
using System.IO;
using System.Net;
using AppLimit.CloudComputing.SharpBox.StorageProvider.API;
using AppLimit.CloudComputing.SharpBox.StorageProvider.BaseObjects;
using AppLimit.CloudComputing.SharpBox.Exceptions;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.CIFS.Logic
{
    internal class CIFSStorageProviderService : GenericStorageProviderService
    {
        public override bool VerifyAccessTokenType(ICloudStorageAccessToken token)
        {
            return (token is ICredentials);
        }

        public override IStorageProviderSession CreateSession(ICloudStorageAccessToken token, ICloudStorageConfiguration configuration)
        {
            // check if url available                                    
            if (!Directory.Exists(configuration.ServiceLocator.LocalPath))
            {
                try
                {
                    Directory.CreateDirectory(configuration.ServiceLocator.LocalPath);
                }
                catch (Exception)
                {
                    return null;
                }
            }

            return new CIFSStorageProviderSession(token, configuration as CIFSConfiguration, this);
        }

        public override ICloudFileSystemEntry RequestResource(IStorageProviderSession session, string Name, ICloudDirectoryEntry parent)
        {
            // build path
            String path;

            if (Name.Equals("/"))
                path = session.ServiceConfiguration.ServiceLocator.LocalPath;
            else if (parent == null)
                path = Path.Combine(path = session.ServiceConfiguration.ServiceLocator.LocalPath, Name);
            else
                path = new Uri(GetResourceUrl(session, parent, Name)).LocalPath;

            // check if file exists
            if (File.Exists(path))
            {
                // create the fileinfo
                FileInfo fInfo = new FileInfo(path);

                BaseFileEntry bf = new BaseFileEntry(fInfo.Name, fInfo.Length, fInfo.LastWriteTimeUtc, this, session) {Parent = parent};

                // add to parent
                if (parent != null)
                    (parent as BaseDirectoryEntry).AddChild(bf);

                // go ahead
                return bf;
            }
                // check if directory exists
            else if (Directory.Exists(path))
            {
                // build directory info
                DirectoryInfo dInfo = new DirectoryInfo(path);

                // build bas dir
                BaseDirectoryEntry dir = CreateEntryByFileSystemInfo(dInfo, session, parent) as BaseDirectoryEntry;
                if (Name.Equals("/"))
                    dir.Name = "/";

                // add to parent
                if (parent != null)
                    (parent as BaseDirectoryEntry).AddChild(dir);

                // refresh the childs
                RefreshChildsOfDirectory(session, dir);

                // go ahead
                return dir;
            }
            else
                return null;
        }

        public override void RefreshResource(IStorageProviderSession session, ICloudFileSystemEntry resource)
        {
            // nothing to do for files
            if (!(resource is ICloudDirectoryEntry))
                return;

            // Refresh schild
            RefreshChildsOfDirectory(session, resource as BaseDirectoryEntry);
        }

        public override bool DeleteResource(IStorageProviderSession session, ICloudFileSystemEntry entry)
        {
            // generate the loca path
            String uriPath = GetResourceUrl(session, entry, null);
            Uri uri = new Uri(uriPath);

            // removed the file
            if (File.Exists(uri.LocalPath))
                File.Delete(uri.LocalPath);
            else if (Directory.Exists(uri.LocalPath))
                Directory.Delete(uri.LocalPath, true);

            // go ahead
            return true;

        }

        public override bool MoveResource(IStorageProviderSession session, ICloudFileSystemEntry fsentry, ICloudDirectoryEntry newParent)
        {
            // build the new uri
            String newPlace = GetResourceUrl(session, newParent, fsentry.Name);

            if (RenameResourceEx(session, fsentry, newPlace))
            {
                // remove from parent
                (fsentry.Parent as BaseDirectoryEntry).RemoveChild(fsentry as BaseFileEntry);

                // add to new parent
                (newParent as BaseDirectoryEntry).AddChild(fsentry as BaseFileEntry);

                return true;
            }
            else
                return false;
        }

        public override Stream CreateDownloadStream(IStorageProviderSession session, ICloudFileSystemEntry fileSystemEntry)
        {
            // get the full path
            String uriPath = GetResourceUrl(session, fileSystemEntry, null);
            Uri uri = new Uri(uriPath);

            // open src file
            return new FileStream(uri.LocalPath, FileMode.Open);
        }

        public override Stream CreateUploadStream(IStorageProviderSession session, ICloudFileSystemEntry fileSystemEntry, long uploadSize)
        {
            // get the full path
            String uriPath = GetResourceUrl(session, fileSystemEntry, null);
            Uri uri = new Uri(uriPath);

            // set the new size
            BaseFileEntry f = fileSystemEntry as BaseFileEntry;
            f.Length = uploadSize;

            // create the file if not exists
            FileStream fs = null;
            if (!File.Exists(uri.LocalPath))
                fs = File.Create(uri.LocalPath);
            else
                fs = File.OpenWrite(uri.LocalPath);

            // go ahead
            return fs;
        }

        public override bool SupportsDirectRetrieve
        {
            get { return false; }
        }

        public override void CommitStreamOperation(IStorageProviderSession session, ICloudFileSystemEntry fileSystemEntry, nTransferDirection Direction, Stream NotDisposedStream)
        {

        }

        public override ICloudFileSystemEntry CreateResource(IStorageProviderSession session, string Name, ICloudDirectoryEntry parent)
        {
            // build the full url
            String resFull = GetResourceUrl(session, parent, Name);
            Uri uri = new Uri(resFull);

            // create the director
            DirectoryInfo dinfo = Directory.CreateDirectory(uri.LocalPath);

            // create the filesystem object
            ICloudFileSystemEntry fsEntry = CreateEntryByFileSystemInfo(dinfo, session, parent);

            // add parent child
            if (parent != null)
                (parent as BaseDirectoryEntry).AddChild(fsEntry as BaseFileEntry);

            // go ahead
            return fsEntry;
        }

        public override bool RenameResource(IStorageProviderSession session, ICloudFileSystemEntry fsentry, string newName)
        {
            // get new name uri
            String uriPath = GetResourceUrl(session, fsentry.Parent, newName);

            // do it 
            return RenameResourceEx(session, fsentry, uriPath.ToString());
        }


        #region Helper

        private BaseFileEntry CreateEntryByFileSystemInfo(FileSystemInfo info, IStorageProviderSession session, ICloudDirectoryEntry parent)
        {
            if (info is DirectoryInfo)
                return new BaseDirectoryEntry(info.Name, 0, info.LastWriteTimeUtc, this, session);
            else if (info is FileInfo)
                return new BaseFileEntry(info.Name, (info as FileInfo).Length, info.LastWriteTimeUtc, this, session);
            else
                throw new Exception("Invalid filesysteminfo type");
        }

        private void RefreshChildsOfDirectory(IStorageProviderSession session, BaseDirectoryEntry dir)
        {
            // get the location
            String reslocation = GetResourceUrl(session, dir as ICloudFileSystemEntry, null);

            // ensure that we have a trailing slash                        
            reslocation = reslocation.TrimEnd('/');
            reslocation = reslocation + "/";

            // build the uri
            Uri resUri = new Uri(reslocation);

            // convert BaseDir to DirInfo
            DirectoryInfo dInfo = new DirectoryInfo(resUri.LocalPath);

            // clear childs
            dir.ClearChilds();

            // get all childs
            foreach (FileSystemInfo fInfo in dInfo.GetFileSystemInfos())
            {
                BaseFileEntry f = CreateEntryByFileSystemInfo(fInfo, session, dir);
                dir.AddChild(f);
            }
        }

        public bool RenameResourceEx(IStorageProviderSession session, ICloudFileSystemEntry fsentry, String newFullPath)
        {
            // get the uri
            String uriPath = GetResourceUrl(session, fsentry, null);
            Uri srUri = new Uri(uriPath);

            // get new name uri            
            Uri tgUri = new Uri(newFullPath);

            // rename
            FileSystemInfo f = null;
            if (File.Exists(srUri.LocalPath))
            {
                f = new FileInfo(srUri.LocalPath);
                ((FileInfo) f).MoveTo(tgUri.LocalPath);
            }
            else if (Directory.Exists(srUri.LocalPath))
            {
                f = new DirectoryInfo(srUri.LocalPath);
                ((DirectoryInfo) f).MoveTo(tgUri.LocalPath);
            }
            else
            {
                throw new SharpBoxException(SharpBoxErrorCodes.ErrorFileNotFound);
            }

            // reload file info 
            if (File.Exists(tgUri.LocalPath))
            {
                f = new FileInfo(tgUri.LocalPath);
            }
            else if (Directory.Exists(tgUri.LocalPath))
            {
                f = new DirectoryInfo(tgUri.LocalPath);
            }
            else
            {
                throw new SharpBoxException(SharpBoxErrorCodes.ErrorFileNotFound);
            }

            // update fsEntry
            BaseFileEntry fs = fsentry as BaseFileEntry;
            fs.Name = Path.GetFileName(tgUri.LocalPath);
            fs.Length = (f is FileInfo ? (f as FileInfo).Length : 0);
            fs.Modified = f.LastWriteTimeUtc;

            // go ahead
            return true;
        }

        #endregion
    }
}