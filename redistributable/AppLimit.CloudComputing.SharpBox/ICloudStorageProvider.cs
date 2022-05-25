using System;
using System.Collections.Generic;

namespace AppLimit.CloudComputing.SharpBox
{
    public interface ICloudStorageProvider
    {
        ICloudStorageAccessToken CurrentAccessToken { get; }

        ICloudStorageAccessToken Open(ICloudStorageConfiguration configuration, ICloudStorageAccessToken token);

        void Close();

        ICloudDirectoryEntry GetRoot();

        ICloudFileSystemEntry GetFileSystemObject(string path, ICloudDirectoryEntry parent);

        ICloudDirectoryEntry CreateFolder(string name, ICloudDirectoryEntry parent);

        bool DeleteFileSystemEntry(ICloudFileSystemEntry fsentry);

        bool MoveFileSystemEntry(ICloudFileSystemEntry fsentry, ICloudDirectoryEntry newParent);

        bool CopyFileSystemEntry(ICloudFileSystemEntry fsentry, ICloudDirectoryEntry newParent);

        bool RenameFileSystemEntry(ICloudFileSystemEntry fsentry, string newName);

        ICloudFileSystemEntry CreateFile(ICloudDirectoryEntry parent, string name);

        Uri GetFileSystemObjectUrl(string path, ICloudDirectoryEntry parent);

        string GetFileSystemObjectPath(ICloudFileSystemEntry fsObject);
    }

    internal interface ICloudStorageProviderInternal : ICloudStorageProvider
    {
        ICloudStorageAccessToken LoadToken(Dictionary<string, string> tokendata);

        void StoreToken(Dictionary<string, string> tokendata, ICloudStorageAccessToken token);
    }
}