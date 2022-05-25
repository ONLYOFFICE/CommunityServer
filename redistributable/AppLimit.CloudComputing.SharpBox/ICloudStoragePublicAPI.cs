using System;
using System.Collections.Generic;
using System.IO;

namespace AppLimit.CloudComputing.SharpBox
{
    /// <summary>
    /// This enum describes all support cloud storage provider
    /// configuration in sharpbox. This enum will also be used
    /// to implement a service chooser in the future
    /// </summary>
    public enum nSupportedCloudConfigurations
    {
        /// <summary>
        /// DropBox 
        /// </summary>
        DropBox,

        /// <summary>
        /// StoreGate
        /// </summary>
        StoreGate,

        /// <summary>
        /// BoxNet
        /// </summary>
        BoxNet,

        /// <summary>
        /// SmartDrive
        /// </summary>
        SmartDrive,

        /// <summary>
        /// WebDav
        /// </summary>
        WebDav,

        /// <summary>
        /// CloudMe
        /// </summary>
        CloudMe,

        /// <summary>
        /// Strato HiDrive
        /// </summary>
        HiDrive,

        /// <summary>
        /// Google Drive
        /// </summary>
        Google,

        /// <summary>
        /// kDrive
        /// </summary>
        kDrive,

        /// <summary>
        /// Yandex.Disk
        /// </summary>
        Yandex,

        /// <summary>
        /// SkyDrive
        /// </summary>
        SkyDrive
    }

    public interface ICloudStoragePublicAPI : ICloudStorageProvider, ICloudStorageAsyncInterface
    {
        bool IsOpened { get; }

        #region Security Token Handling

        Stream SerializeSecurityToken(ICloudStorageAccessToken token);

        Stream SerializeSecurityToken(ICloudStorageAccessToken token, Dictionary<string, string> additionalMetaData);

        Stream SerializeSecurityTokenEx(ICloudStorageAccessToken token, Type configurationType, Dictionary<string, string> additionalMetaData);

        ICloudStorageAccessToken DeserializeSecurityToken(Stream tokenStream);

        ICloudStorageAccessToken DeserializeSecurityToken(Stream tokenStream, out Dictionary<string, string> additionalMetaData);

        #endregion

        #region Comfort Functions

        ICloudDirectoryEntry GetFolder(string path);

        ICloudDirectoryEntry GetFolder(string path, ICloudDirectoryEntry parent);

        ICloudDirectoryEntry GetFolder(string path, bool throwException);

        ICloudDirectoryEntry GetFolder(string path, ICloudDirectoryEntry startFolder, bool throwException);

        ICloudFileSystemEntry GetFile(string path, ICloudDirectoryEntry startFolder);

        void DownloadFile(ICloudDirectoryEntry parent, string name, string targetPath);

        void DownloadFile(ICloudDirectoryEntry parent, string name, string targetPath, FileOperationProgressChanged delProgress);

        void DownloadFile(string filePath, string targetPath);

        void DownloadFile(string filePath, string targetPath, FileOperationProgressChanged delProgress);

        void DownloadFile(string name, ICloudDirectoryEntry parent, Stream targetStream);

        ICloudFileSystemEntry UploadFile(string filePath, ICloudDirectoryEntry targetContainer);

        ICloudFileSystemEntry UploadFile(string filePath, ICloudDirectoryEntry targetContainer, FileOperationProgressChanged delProgress);

        ICloudFileSystemEntry UploadFile(string filePath, ICloudDirectoryEntry targetContainer, string targetFileName);

        ICloudFileSystemEntry UploadFile(string filePath, ICloudDirectoryEntry targetContainer, string targetFileName, FileOperationProgressChanged delProgress);

        ICloudFileSystemEntry UploadFile(string filePath, string targetDirectory);

        ICloudFileSystemEntry UploadFile(string filePath, string targetDirectory, FileOperationProgressChanged delProgress);

        ICloudFileSystemEntry UploadFile(string filePath, string targetDirectory, string targetFileName);

        ICloudFileSystemEntry UploadFile(string filePath, string targetDirectory, string targetFileName, FileOperationProgressChanged delProgress);

        ICloudFileSystemEntry UploadFile(Stream uploadDataStream, string targetFileName, ICloudDirectoryEntry targetContainer, FileOperationProgressChanged delProgress);

        ICloudFileSystemEntry UploadFile(Stream uploadDataStream, string targetFileName, ICloudDirectoryEntry targetContainer);

        ICloudDirectoryEntry CreateFolder(string path);

        ICloudDirectoryEntry CreateFolderEx(string path, ICloudDirectoryEntry entry);

        bool DeleteFileSystemEntry(string filePath);

        bool MoveFileSystemEntry(string filePath, string newParentPath);

        bool RenameFileSystemEntry(string filePath, string newName);
        bool CopyFileSystemEntry(string filePath, string newParentPath);
        ICloudFileSystemEntry CreateFile(string filePath);

        #endregion

        #region Configuration Mapping

        ICloudStorageConfiguration GetCloudConfiguration(nSupportedCloudConfigurations configtype, params object[] param);

        #endregion
    }
}