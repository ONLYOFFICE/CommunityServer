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

        ICloudFileSystemEntry GetFileSystemObject(String path, ICloudDirectoryEntry parent);

        ICloudDirectoryEntry CreateFolder(String name, ICloudDirectoryEntry parent);

        Boolean DeleteFileSystemEntry(ICloudFileSystemEntry fsentry);

        Boolean MoveFileSystemEntry(ICloudFileSystemEntry fsentry, ICloudDirectoryEntry newParent);

        Boolean CopyFileSystemEntry(ICloudFileSystemEntry fsentry, ICloudDirectoryEntry newParent);

        Boolean RenameFileSystemEntry(ICloudFileSystemEntry fsentry, String newName);

        ICloudFileSystemEntry CreateFile(ICloudDirectoryEntry parent, String name);

        Uri GetFileSystemObjectUrl(String path, ICloudDirectoryEntry parent);

        String GetFileSystemObjectPath(ICloudFileSystemEntry fsObject);                
    }

    internal interface ICloudStorageProviderInternal : ICloudStorageProvider
    {
        ICloudStorageAccessToken LoadToken(Dictionary<String, String> tokendata);

        void StoreToken(Dictionary<String, String> tokendata, ICloudStorageAccessToken token);
    }
}
