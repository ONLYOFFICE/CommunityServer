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
        Boolean IsOpened { get; }

        #region Security Token Handling

        Stream SerializeSecurityToken(ICloudStorageAccessToken token);

        Stream SerializeSecurityToken(ICloudStorageAccessToken token, Dictionary<String, String> additionalMetaData);

        Stream SerializeSecurityTokenEx(ICloudStorageAccessToken token, Type configurationType, Dictionary<String, String> additionalMetaData);

        ICloudStorageAccessToken DeserializeSecurityToken(Stream tokenStream);

        ICloudStorageAccessToken DeserializeSecurityToken(Stream tokenStream, out Dictionary<String, String> additionalMetaData);

        #endregion

        #region Comfort Functions

        ICloudDirectoryEntry GetFolder(String path);

        ICloudDirectoryEntry GetFolder(String path, ICloudDirectoryEntry parent);

        ICloudDirectoryEntry GetFolder(String path, Boolean throwException);

        ICloudDirectoryEntry GetFolder(String path, ICloudDirectoryEntry startFolder, Boolean throwException);

        ICloudFileSystemEntry GetFile(String path, ICloudDirectoryEntry startFolder);

        void DownloadFile(ICloudDirectoryEntry parent, String name, String targetPath);

        void DownloadFile(ICloudDirectoryEntry parent, String name, String targetPath, FileOperationProgressChanged delProgress);

        void DownloadFile(String filePath, String targetPath);

        void DownloadFile(String filePath, String targetPath, FileOperationProgressChanged delProgress);

        void DownloadFile(String name, ICloudDirectoryEntry parent, Stream targetStream);

        ICloudFileSystemEntry UploadFile(String filePath, ICloudDirectoryEntry targetContainer);

        ICloudFileSystemEntry UploadFile(String filePath, ICloudDirectoryEntry targetContainer, FileOperationProgressChanged delProgress);

        ICloudFileSystemEntry UploadFile(String filePath, ICloudDirectoryEntry targetContainer, string targetFileName);

        ICloudFileSystemEntry UploadFile(String filePath, ICloudDirectoryEntry targetContainer, string targetFileName, FileOperationProgressChanged delProgress);

        ICloudFileSystemEntry UploadFile(string filePath, string targetDirectory);

        ICloudFileSystemEntry UploadFile(string filePath, string targetDirectory, FileOperationProgressChanged delProgress);

        ICloudFileSystemEntry UploadFile(string filePath, string targetDirectory, string targetFileName);

        ICloudFileSystemEntry UploadFile(string filePath, string targetDirectory, string targetFileName, FileOperationProgressChanged delProgress);

        ICloudFileSystemEntry UploadFile(Stream uploadDataStream, String targetFileName, ICloudDirectoryEntry targetContainer, FileOperationProgressChanged delProgress);

        ICloudFileSystemEntry UploadFile(Stream uploadDataStream, String targetFileName, ICloudDirectoryEntry targetContainer);

        ICloudDirectoryEntry CreateFolder(String path);

        ICloudDirectoryEntry CreateFolderEx(String path, ICloudDirectoryEntry entry);

        bool DeleteFileSystemEntry(String filePath);

        bool MoveFileSystemEntry(String filePath, String newParentPath);

        bool RenameFileSystemEntry(String filePath, String newName);
        bool CopyFileSystemEntry(String filePath, String newParentPath);
        ICloudFileSystemEntry CreateFile(String filePath);

        #endregion

        #region Configuration Mapping

        ICloudStorageConfiguration GetCloudConfiguration(nSupportedCloudConfigurations configtype, params object[] param);

        #endregion
    }
}