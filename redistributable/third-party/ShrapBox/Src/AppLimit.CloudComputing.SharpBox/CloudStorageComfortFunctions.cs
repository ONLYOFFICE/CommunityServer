using System;
using System.IO;

using AppLimit.CloudComputing.SharpBox.Common.IO;
using AppLimit.CloudComputing.SharpBox.Exceptions;
using System.Net;

namespace AppLimit.CloudComputing.SharpBox
{    
    public partial class CloudStorage
    {
        #region Comfort Functions

        /// <summary>
        /// This method returns access ro a specific subfolder addressed via 
        /// unix like file system path, e.g. /Public/SubFolder/SubSub
        /// 
        /// Valid Exceptions are:
        ///  SharpBoxException(nSharpBoxErrorCodes.ErrorInvalidFileOrDirectoryName);
        ///  SharpBoxException(nSharpBoxErrorCodes.ErrorFileNotFound);
        /// </summary>
        /// <param name="path">The path to the target subfolder</param>
        /// <returns>Reference to the searched folder</returns>
        public ICloudDirectoryEntry GetFolder(String path)
        {
            var ph = new PathHelper(path);
            if (!ph.IsPathRooted())
                throw new SharpBoxException(SharpBoxErrorCodes.ErrorInvalidFileOrDirectoryName);

            return GetFolder(path, null);
        }

        /// <summary>
        /// This method returns access ro a specific subfolder addressed via 
        /// unix like file system path, e.g. SubFolder/SubSub
        /// 
        /// Valid Exceptions are:
        ///  SharpBoxException(nSharpBoxErrorCodes.ErrorInvalidFileOrDirectoryName);
        ///  SharpBoxException(nSharpBoxErrorCodes.ErrorFileNotFound);
        /// </summary>
        /// <param name="path">The path to the target subfolder</param>
        /// <param name="parent">A reference to the parent when a relative path was given</param>
        /// <returns>Reference to the searched folder</returns>
        public ICloudDirectoryEntry GetFolder(String path, ICloudDirectoryEntry parent)
        {
            var dir = GetFileSystemObject(path, parent) as ICloudDirectoryEntry;
            if (dir == null)
                throw new SharpBoxException(SharpBoxErrorCodes.ErrorFileNotFound);
        	
			return dir;
        }

        /// <summary>
        /// This method returns a folder without thrown an exception in the case of an error
        /// </summary>
        /// <param name="path"></param>
        /// <param name="throwException"></param>
        /// <returns></returns>
        public ICloudDirectoryEntry GetFolder(String path, Boolean throwException)
        {
            try
            {
                return GetFolder(path);
            }
            catch (SharpBoxException)
            {
                if (throwException)
                    throw;

                return null;
            }
        }

        /// <summary>
        /// This method ...
        /// </summary>
        /// <param name="path"></param>
        /// <param name="startFolder"></param>
        /// <param name="throwException"></param>
        /// <returns></returns>
        public ICloudDirectoryEntry GetFolder(String path, ICloudDirectoryEntry startFolder, Boolean throwException)
        {
            try
            {
                return GetFolder(path, startFolder);
            }
            catch (SharpBoxException)
            {
                if (throwException)
                    throw;

                return null;
            }
        }

        /// <summary>
        /// This method returns access to a specific file addressed via 
        /// unix like file system path, e.g. /Public/SubFolder/SubSub
        /// 
        /// Valid Exceptions are:
        ///  SharpBoxException(nSharpBoxErrorCodes.ErrorInvalidFileOrDirectoryName);
        ///  SharpBoxException(nSharpBoxErrorCodes.ErrorFileNotFound);
        /// </summary>
        /// <param name="path">The path to the target subfolder</param>
        /// <param name="startFolder">The startfolder for the searchpath</param>
        /// <returns></returns>
        public ICloudFileSystemEntry GetFile(String path, ICloudDirectoryEntry startFolder)
        {
            ICloudFileSystemEntry fsEntry = GetFileSystemObject(path, startFolder);
            if (fsEntry is ICloudDirectoryEntry)
                throw new SharpBoxException(SharpBoxErrorCodes.ErrorInvalidFileOrDirectoryName);
        	
			return fsEntry;
        }      

        /// <summary>
        /// This functions allows to download a specific file
        ///         
        /// Valid Exceptions are:        
        ///  SharpBoxException(nSharpBoxErrorCodes.ErrorFileNotFound);
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <param name="targetPath"></param>        
        /// <returns></returns>
        public void DownloadFile(ICloudDirectoryEntry parent, String name, String targetPath)
        {
            DownloadFile(parent, name, targetPath, null);
        }

        /// <summary>
        /// This functions allows to download a specific file
        ///         
        /// Valid Exceptions are:        
        ///  SharpBoxException(nSharpBoxErrorCodes.ErrorFileNotFound);
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <param name="targetPath"></param>
        /// <param name="delProgress"></param>
        /// <returns></returns>
        public void DownloadFile(ICloudDirectoryEntry parent, String name, String targetPath, FileOperationProgressChanged delProgress)
        {
            // check parameters
            if (parent == null || name == null || targetPath == null)
                throw new SharpBoxException(SharpBoxErrorCodes.ErrorInvalidParameters);


#if !WINDOWS_PHONE && !ANDROID
            // expand environment in target path
            targetPath = Environment.ExpandEnvironmentVariables(targetPath);
#endif
            // get the file entry
			ICloudFileSystemEntry file = parent.GetChild(name, true);
            using (var targetData = new FileStream(Path.Combine(targetPath, file.Name), FileMode.Create, FileAccess.Write, FileShare.None))
            {
                file.GetDataTransferAccessor().Transfer(targetData, nTransferDirection.nDownload, delProgress, null);	
            }
        }

        /// <summary>
        /// This function allows to download a specific file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="targetPath"></param>        
        /// <returns></returns>        
        public void DownloadFile(String filePath, String targetPath)
        {
            DownloadFile(filePath, targetPath, null);
        }

        /// <summary>
        /// This function allows to download a specific file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="targetPath"></param>
        /// <param name="delProgress"></param>
        /// <returns></returns>        
        public void DownloadFile(String filePath, String targetPath, FileOperationProgressChanged delProgress)
        {
            // check parameter
            if (filePath == null || targetPath == null)
                throw new SharpBoxException(SharpBoxErrorCodes.ErrorInvalidParameters);

            // get path and filename
            var ph = new PathHelper(filePath);
            String dir = ph.GetDirectoryName();
            String file = ph.GetFileName();

            // check if we are in root
            if (dir.Length == 0)
                dir = "/";

            // get parent container
            ICloudDirectoryEntry container = GetFolder(dir);

            // download file
            DownloadFile(container, file, targetPath, delProgress);
        }

        /// <summary>
        /// This method returns a data stream which allows to downloads the 
        /// file data
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <param name="targetStream"></param>
        /// <returns></returns>
        public void DownloadFile(String name, ICloudDirectoryEntry parent, Stream targetStream)
        {        
            // check parameters
            if (parent == null || name == null)
                throw new SharpBoxException(SharpBoxErrorCodes.ErrorInvalidParameters);

            // get the file entry
			ICloudFileSystemEntry file = parent.GetChild(name, true);

            // download the data
            file.GetDataTransferAccessor().Transfer(targetStream, nTransferDirection.nDownload);            
        }

        /// <summary>
		/// This function allowes to upload a local file
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="targetContainer"></param>
		/// <returns></returns>
        public ICloudFileSystemEntry UploadFile(String filePath, ICloudDirectoryEntry targetContainer)
        {
            return UploadFile(filePath, targetContainer, (FileOperationProgressChanged)null);
        }

		/// <summary>
		/// This function allowes to upload a local file
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="targetContainer"></param>
        /// <param name="delProgress"></param>
		/// <returns></returns>        
        public ICloudFileSystemEntry UploadFile(String filePath, ICloudDirectoryEntry targetContainer, FileOperationProgressChanged delProgress)
		{
			// check parameters
			if (String.IsNullOrEmpty(filePath))
				throw new SharpBoxException(SharpBoxErrorCodes.ErrorInvalidParameters);

            return UploadFile(filePath, targetContainer, Path.GetFileName(filePath), delProgress);
		}

        /// <summary>
		/// This function allowes to upload a local file. Remote file will be created with the name specifed by
		/// the targetFileName argument
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="targetContainer"></param>
		/// <param name="targetFileName"></param>        
		/// <returns></returns>
        public ICloudFileSystemEntry UploadFile(String filePath, ICloudDirectoryEntry targetContainer, string targetFileName)
        {
            return UploadFile(filePath, targetContainer, targetFileName, null);
        }

		/// <summary>
		/// This function allowes to upload a local file. Remote file will be created with the name specifed by
		/// the targetFileName argument
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="targetContainer"></param>
		/// <param name="targetFileName"></param>
        /// <param name="delProgress"></param>
		/// <returns></returns>
        public ICloudFileSystemEntry UploadFile(String filePath, ICloudDirectoryEntry targetContainer, string targetFileName, FileOperationProgressChanged delProgress)
		{
			// check parameter
			if (String.IsNullOrEmpty(filePath) || String.IsNullOrEmpty(targetFileName) || targetContainer == null)
				throw new SharpBoxException(SharpBoxErrorCodes.ErrorInvalidParameters);

			// check if the target is a real file
			if (!File.Exists(filePath))
				throw new SharpBoxException(SharpBoxErrorCodes.ErrorFileNotFound);

			// build the source stream
			using (var srcStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				// create the upload file
				ICloudFileSystemEntry newFile = CreateFile(targetContainer, targetFileName);
				if (newFile == null)
					throw new SharpBoxException(SharpBoxErrorCodes.ErrorFileNotFound);

                // upload the data
                newFile.GetDataTransferAccessor().Transfer(srcStream, nTransferDirection.nUpload, delProgress, null);

				// go ahead
				return newFile;
			}
		}

        /// <summary>
		/// This function allows to upload a local file
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="targetDirectory"></param>
		/// <returns></returns>
        public ICloudFileSystemEntry UploadFile(string filePath, string targetDirectory)
        {
            return UploadFile(filePath, targetDirectory, (FileOperationProgressChanged)null);
        }

		/// <summary>
		/// This function allows to upload a local file
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="targetDirectory"></param>
        /// <param name="delProgress"></param>
		/// <returns></returns>
        public ICloudFileSystemEntry UploadFile(string filePath, string targetDirectory, FileOperationProgressChanged delProgress)
		{
			// check parameters
			if (String.IsNullOrEmpty(filePath))
				throw new SharpBoxException(SharpBoxErrorCodes.ErrorInvalidParameters);

            return UploadFile(filePath, targetDirectory, Path.GetFileName(filePath), delProgress);
		}

        		/// <summary>
		/// This function allows to upload a local file. Remote file will be created with the name specifed by
		/// the targetFileName argument
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="targetDirectory"></param>
		/// <param name="targetFileName"></param>
		/// <returns></returns>
        public ICloudFileSystemEntry UploadFile(string filePath, string targetDirectory, string targetFileName)
        {
            return UploadFile(filePath, targetDirectory, targetFileName, null);
        }

		/// <summary>
		/// This function allows to upload a local file. Remote file will be created with the name specifed by
		/// the targetFileName argument
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="targetDirectory"></param>
		/// <param name="targetFileName"></param>
        /// <param name="delProgress"></param>
		/// <returns></returns>
        public ICloudFileSystemEntry UploadFile(string filePath, string targetDirectory, string targetFileName, FileOperationProgressChanged delProgress)
		{
			// check parameters
			if (String.IsNullOrEmpty(filePath) || String.IsNullOrEmpty(targetFileName) || targetDirectory == null)
				throw new SharpBoxException(SharpBoxErrorCodes.ErrorInvalidParameters);

			// get target container
			ICloudDirectoryEntry target = GetFolder(targetDirectory);
			if (target == null)
				throw new SharpBoxException(SharpBoxErrorCodes.ErrorFileNotFound);

            // upload file
            using (Stream s = File.OpenRead(filePath))
            {
                return UploadFile(s, targetFileName, target, delProgress);
            }
		}

        /// <summary>
        /// This method allows to upload the data from a given filestream into a target file
        /// </summary>
        /// <param name="uploadDataStream"></param>
        /// <param name="targetFileName"></param>
        /// <param name="targetContainer"></param>
        /// <param name="delProgress"></param>
        /// <returns></returns>
        public ICloudFileSystemEntry UploadFile(Stream uploadDataStream, String targetFileName, ICloudDirectoryEntry targetContainer, FileOperationProgressChanged delProgress)
        {
            // check parameters
            if (String.IsNullOrEmpty(targetFileName) || uploadDataStream == null || targetContainer == null)
                throw new SharpBoxException(SharpBoxErrorCodes.ErrorInvalidParameters);

            // create the upload file
            ICloudFileSystemEntry newFile = CreateFile(targetContainer, targetFileName);
            if (newFile == null)
                throw new SharpBoxException(SharpBoxErrorCodes.ErrorFileNotFound);

            // upload data
            newFile.GetDataTransferAccessor().Transfer(uploadDataStream, nTransferDirection.nUpload, delProgress, null);
            
            // go ahead
            return newFile;            
        }

        /// <summary>
        /// This method allows to upload the data from a given filestream into a target file
        /// </summary>
        /// <param name="uploadDataStream"></param>
        /// <param name="targetFileName"></param>
        /// <param name="targetContainer"></param>
        /// <returns></returns>
        public ICloudFileSystemEntry UploadFile(Stream uploadDataStream, String targetFileName, ICloudDirectoryEntry targetContainer)
        {
            return UploadFile(uploadDataStream, targetFileName, targetContainer, null);
        }

        /// <summary>
        /// This function allows to create full folder pathes in the cloud storage
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public ICloudDirectoryEntry CreateFolder(String path)
        {
            // get the path elemtens
            var ph = new PathHelper(path);

            // check parameter
            if (!ph.IsPathRooted())
                return null;

            // start creation
            return CreateFolderEx(path, GetRoot());
        }

        /// <summary>
        /// This function allows to create full folder pathes in the cloud storage
        /// </summary>
        /// <param name="path"></param>
        /// <param name="entry"></param>
        /// <returns></returns>
        public ICloudDirectoryEntry CreateFolderEx(String path, ICloudDirectoryEntry entry)
        {
            // get the path elemtens
            var ph = new PathHelper(path);

            String[] pes = ph.GetPathElements();

            // check which elements are existing            
            foreach (String el in pes)
            {
                // check if subfolder exists, if it doesn't, create it
                ICloudDirectoryEntry cur = GetFolder(el, entry, false);

                // create if needed
                if (cur == null)
                {
                    ICloudDirectoryEntry newFolder = CreateFolder(el, entry);
                    if (newFolder == null)
                        throw new SharpBoxException(SharpBoxErrorCodes.ErrorCreateOperationFailed);
                    else
                        cur = newFolder;
                }

                // go ahead
                entry = cur;
            }

            // return 
            return entry;
        }

        /// <summary>
        /// This function allows to delete a specific file or directory
        /// </summary>
        /// <param name="filePath">File or directory path which has to be deleted</param>
        /// <returns></returns>        
        public bool DeleteFileSystemEntry(String filePath)
        {
            // check parameter
            if (filePath == null)
                throw new SharpBoxException(SharpBoxErrorCodes.ErrorInvalidParameters);

            // get path and filename
            var ph = new PathHelper(filePath);
            String dir = ph.GetDirectoryName();
            String file = ph.GetFileName();

            // check if we are in root
            if (dir.Length == 0)
                dir = "/";

            // get parent container
            ICloudDirectoryEntry container = GetFolder(dir);

            // get filesystem entry
            ICloudFileSystemEntry fsEntry = GetFileSystemObject(file, container);
            if (fsEntry == null) return false;

            // delete file
            return DeleteFileSystemEntry(fsEntry);
        }

        /// <summary>
        /// This method moves a file or a directory from its current
        /// location to a new onw
        /// </summary>
        /// <param name="filePath">Filesystem object which has to be moved</param>
        /// /// <param name="newParentPath">The new location of the targeted filesystem object</param>
        /// <returns></returns>        
        public bool MoveFileSystemEntry(String filePath, String newParentPath)
        {
            // check parameter
            if ((filePath == null) || (newParentPath == null))
                throw new SharpBoxException(SharpBoxErrorCodes.ErrorInvalidParameters);

            // get path and filename
            var ph = new PathHelper(filePath);
            String dir = ph.GetDirectoryName();
            String file = ph.GetFileName();

            // check if we are in root
            if (dir.Length == 0)
                dir = "/";

            // get parent container
            ICloudDirectoryEntry container = GetFolder(dir);

            // get filesystem entry
            ICloudFileSystemEntry fsEntry = GetFileSystemObject(file, container);

            // get new parent path
            ICloudDirectoryEntry newParent = GetFolder(newParentPath);

            // move file
            return MoveFileSystemEntry(fsEntry, newParent);
        }

        /// <summary>
        /// This method moves a file or a directory from its current
        /// location to a new onw
        /// </summary>
        /// <param name="filePath">Filesystem object which has to be moved</param>
        /// /// <param name="newParentPath">The new location of the targeted filesystem object</param>
        /// <returns></returns>        
        public bool CopyFileSystemEntry(String filePath, String newParentPath)
        {
            // check parameter
            if ((filePath == null) || (newParentPath == null))
                throw new SharpBoxException(SharpBoxErrorCodes.ErrorInvalidParameters);

            // get path and filename
            var ph = new PathHelper(filePath);
            String dir = ph.GetDirectoryName();
            String file = ph.GetFileName();

            // check if we are in root
            if (dir.Length == 0)
                dir = "/";

            // get parent container
            ICloudDirectoryEntry container = GetFolder(dir);

            // get filesystem entry
            ICloudFileSystemEntry fsEntry = GetFileSystemObject(file, container);

            // get new parent path
            ICloudDirectoryEntry newParent = GetFolder(newParentPath);

            // move file
            return CopyFileSystemEntry(fsEntry, newParent);
        }

        /// <summary>
        /// This mehtod allows to perform a server side renam operation which is basicly the same
        /// then a move operation in the same directory
        /// </summary>
        /// <param name="filePath">File or directory which has to be renamed</param>
        /// <param name="newName">The new name of the targeted filesystem object</param>
        /// <returns></returns>        
        public bool RenameFileSystemEntry(String filePath, String newName)
        {
            // check parameter
            if ((filePath == null) || (newName == null))
                throw new SharpBoxException(SharpBoxErrorCodes.ErrorInvalidParameters);

            // get path and filename
            var ph = new PathHelper(filePath);
            String dir = ph.GetDirectoryName();
            String file = ph.GetFileName();

            // check if we are in root
            if (dir.Length == 0)
                dir = "/";

            // get parent container
            ICloudDirectoryEntry container = GetFolder(dir);

            // get filesystem entry
            ICloudFileSystemEntry fsEntry = GetFileSystemObject(file, container);

            // rename file
            return RenameFileSystemEntry(fsEntry, newName);
        }

        /// <summary>
        /// This method creates a new file object in the cloud storage. Use the GetContentStream method to 
        /// get a .net stream which usable in the same way then local stream are usable
        /// </summary>
		/// <param name="filePath">The name of the targeted file</param>
        /// <returns></returns>        
        public ICloudFileSystemEntry CreateFile(String filePath)
        {
            // check parameter
            if (filePath == null)
                throw new SharpBoxException(SharpBoxErrorCodes.ErrorInvalidParameters);

            // get path and filename
            var ph = new PathHelper(filePath);
            String dir = ph.GetDirectoryName();
            String file = ph.GetFileName();

            // check if we are in root
            if (dir.Length == 0)
                dir = "/";

            // get parent container
            ICloudDirectoryEntry container = GetFolder(dir);

            // rename file
            return CreateFile(container, file);
        }

        /// <summary>
        /// This method serializes a token into an opened stream object
        /// </summary>
        /// <param name="token"></param>
        /// <param name="targetStream"></param>
        public void SerializeSecurityTokenToStream(ICloudStorageAccessToken token, Stream targetStream)
        {
            // open the token stream
            using (Stream tokenStream = SerializeSecurityToken(token))
            {
                StreamHelper.CopyStreamData(this, tokenStream, targetStream, null, null);
            }
        }

        #endregion

        #region Configuration Mapping

        /// <summary>
        /// This method maps a given type of supporte cloud storage provider into a working standard configuration. The parameters
        /// field has to be filled out as follows:
        /// DropBox - nothing
        /// BoxNet - nothing
        /// StoreGate - Use ICredentials for authentication (service will be calculated from this)
        /// SmartDriv - nothing
        /// WebDav - The URL of the webdav service
        /// </summary>
        /// <param name="configtype"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static ICloudStorageConfiguration GetCloudConfigurationEasy(nSupportedCloudConfigurations configtype, params object[] param)
        {
            CloudStorage cl = new CloudStorage();
            return cl.GetCloudConfiguration(configtype, param);            
        }

        /// <summary>
        /// This method maps a given type of supporte cloud storage provider into a working standard configuration. The parameters
        /// field has to be filled out as follows:
        /// DropBox - nothing
        /// BoxNet - nothing
        /// StoreGate - Use ICredentials for authentication (service will be calculated from this)
        /// SmartDriv - nothing
        /// WebDav - The URL of the webdav service
        /// </summary>
        /// <param name="configtype"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public ICloudStorageConfiguration GetCloudConfiguration(nSupportedCloudConfigurations configtype, params object[] param)
        {
            switch (configtype)
            {
                case nSupportedCloudConfigurations.DropBox:
                    return StorageProvider.DropBox.DropBoxConfiguration.GetStandardConfiguration();
                case nSupportedCloudConfigurations.BoxNet:
                    return StorageProvider.BoxNet.BoxNetConfiguration.GetBoxNetConfiguration();
                case nSupportedCloudConfigurations.StoreGate:
                    {
                        // check parameters
                        if (param.Length < 1 || (param[0] as ICredentials) == null)
                        {
                            Exception e = new Exception("Missing valid credentials for StoreGate in the first parameter");
                            throw new SharpBoxException(SharpBoxErrorCodes.ErrorInvalidParameters, e);
                        }

                        // cast creds
                        ICredentials creds = (ICredentials)param[0];

                        // build config
                        return StorageProvider.WebDav.WebDavConfiguration.GetStoreGateConfiguration(creds.GetCredential(null, ""));
                    }
                case nSupportedCloudConfigurations.SmartDrive:
                    return StorageProvider.WebDav.WebDavConfiguration.Get1and1Configuration();
                case nSupportedCloudConfigurations.WebDav:
                    {
                        // check parameters
                        if (param.Length < 1 || (param[0] as Uri) == null)
                        {
                            Exception e = new Exception("Missing URL for webdav server in the first parameter");
                            throw new SharpBoxException(SharpBoxErrorCodes.ErrorInvalidParameters, e);
                        }

                        // convert to uri
                        Uri uri = (Uri)param[0];

                        // create the config
                        StorageProvider.WebDav.WebDavConfiguration cfg =new AppLimit.CloudComputing.SharpBox.StorageProvider.WebDav.WebDavConfiguration(uri);
                        cfg.TrustUnsecureSSLConnections = true;

                        // go ahead
                        return cfg;
                    }
                case nSupportedCloudConfigurations.CloudMe:
                    {
                        // check parameters
                        if (param.Length < 1 || (param[0] as ICredentials) == null)
                        {
                            Exception e = new Exception("Missing valid credentials for CloudMe in the first parameter");
                            throw new SharpBoxException(SharpBoxErrorCodes.ErrorInvalidParameters, e);
                        }

                        // cast creds
                        ICredentials creds = (ICredentials)param[0];

                        // build config
                        return StorageProvider.WebDav.WebDavConfiguration.GetCloudMeConfiguration(creds.GetCredential(null, ""));                        
                    }
                case nSupportedCloudConfigurations.HiDrive:                    
                    return StorageProvider.WebDav.WebDavConfiguration.GetHiDriveConfiguration();
                case nSupportedCloudConfigurations.Google:
                    return StorageProvider.GoogleDocs.GoogleDocsConfiguration.GetStandartConfiguration();
                case nSupportedCloudConfigurations.Yandex:
                    return StorageProvider.WebDav.WebDavConfiguration.GetYandexConfiguration();
                case nSupportedCloudConfigurations.SkyDrive:
                    return new StorageProvider.SkyDrive.SkyDriveConfiguration();
                default:
                    {
                        Exception e = new Exception("Unknow service type");
                        throw new SharpBoxException(SharpBoxErrorCodes.ErrorInvalidParameters, e);
                    }
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Just a helper for the stream copy process
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="pe"></param>        
        /// <param name="data"></param>
        /// <returns></returns>
        internal static StreamHelperResultCodes FileStreamCopyCallback(object sender, StreamHelperProgressEvent pe, params Object[] data)
        {
            // check array
            if (data.Length != 3)
                return StreamHelperResultCodes.OK;

            // get the progess delegate
            FileOperationProgressChanged pc = data[0] as FileOperationProgressChanged;
            if (pc == null)
                return StreamHelperResultCodes.OK;

            // get the file
            ICloudFileSystemEntry e = data[1] as ICloudFileSystemEntry;
            if (e == null)
                return StreamHelperResultCodes.OK;

            // get the progress context
            Object progressContext = data[2];
            
            // create the eventargs element
            FileDataTransferEventArgs arg = new FileDataTransferEventArgs();

            arg.FileSystemEntry = e;
            arg.CurrentBytes = pe.ReadBytesTotal;
            arg.CustomnContext = progressContext;

            if (pe.TotalLength == -1)
                arg.TotalBytes = e.Length;
            else
                arg.TotalBytes = pe.TotalLength;

            arg.PercentageProgress = pe.PercentageProgress;
            arg.TransferRateTotal = pe.TransferRateTotal;
            arg.TransferRateCurrent = pe.TransferRateCurrent;
            
            // calc transfertime            
            if (pe.TransferRateTotal != -1 && pe.TransferRateTotal > 0 )
            {
                long bytesPerSecond = (arg.TransferRateTotal / 8) * 1000;

                if (bytesPerSecond > 0)
                {
                    long neededSeconds = (arg.TotalBytes - arg.CurrentBytes) / bytesPerSecond;
                    arg.OpenTransferTime = new TimeSpan(neededSeconds * TimeSpan.TicksPerSecond);
                }
                else
                    arg.OpenTransferTime = new TimeSpan(long.MaxValue);                       
            }
            else
                arg.OpenTransferTime = new TimeSpan(long.MaxValue);                       

            // call it
            pc(sender, arg);
            
            // create the ret value
            if (arg.Cancel)
                return StreamHelperResultCodes.Aborted;
            else
                return StreamHelperResultCodes.OK;
        }

        #endregion
    }
}
