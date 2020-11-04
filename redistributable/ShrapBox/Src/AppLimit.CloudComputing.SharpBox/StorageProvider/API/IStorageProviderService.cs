using System;
using System.Collections.Generic;
using System.IO;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.API
{
    /// <summary>
    /// This interface is part of the sharpbox extensability api
    /// and has to be implemented as main entry point for an 
    /// existing service
    /// </summary>
    public interface IStorageProviderService
    {
        /// <summary>
        /// Checks if the given token are valid for using with this
        /// service
        ///         
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Boolean VerifyAccessTokenType(ICloudStorageAccessToken token);

        /// <summary>
        /// establishing a new session via access token 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        IStorageProviderSession CreateSession(ICloudStorageAccessToken token, ICloudStorageConfiguration configuration);

        /// <summary>
        /// This method closes a created session
        /// </summary>
        /// <param name="session"></param>
        void CloseSession(IStorageProviderSession session);

        CloudStorageLimits GetLimits(IStorageProviderSession session);

        /// <summary>
        /// Requests all resource information via webcall
        /// </summary>
        /// <param name="session"></param>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        ICloudFileSystemEntry RequestResource(IStorageProviderSession session, string name, ICloudDirectoryEntry parent);

        /// <summary>
        /// This method refreshes the metadata of an resource
        /// </summary>
        /// <param name="session"></param>
        /// <param name="resource"></param>
        void RefreshResource(IStorageProviderSession session, ICloudFileSystemEntry resource);

        /// <summary>
        /// Creates or updates a resource via webcall 
        /// </summary>
        /// <param name="session"></param>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        ICloudFileSystemEntry CreateResource(IStorageProviderSession session, string name, ICloudDirectoryEntry parent);

        /// <summary>
        /// Removes a specific resource from the web location
        /// </summary>
        /// <param name="session"></param>
        /// <param name="entry"></param>
        Boolean DeleteResource(IStorageProviderSession session, ICloudFileSystemEntry entry);

        /// <summary>
        /// Moves a give resource from one location to an other
        /// </summary>
        /// <param name="session"></param>
        /// <param name="fsentry"></param>
        /// <param name="newParent"></param>
        /// <returns></returns>
        Boolean MoveResource(IStorageProviderSession session, ICloudFileSystemEntry fsentry, ICloudDirectoryEntry newParent);


        /// <summary>
        /// Copy a give resource from one location to an other
        /// </summary>
        /// <param name="session"></param>
        /// <param name="fsentry"></param>
        /// <param name="newParent"></param>
        /// <returns></returns>
        Boolean CopyResource(IStorageProviderSession session, ICloudFileSystemEntry fsentry, ICloudDirectoryEntry newParent);

        /// <summary>
        /// Renames a give resource
        /// </summary>
        /// <param name="session"></param>
        /// <param name="fsentry"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        Boolean RenameResource(IStorageProviderSession session, ICloudFileSystemEntry fsentry, String newName);

        /// <summary>
        /// Receives the information which are stored in a token
        /// </summary>
        /// <param name="session"></param>
        /// <param name="tokendata"></param>
        /// <param name="token"></param>
        void StoreToken(IStorageProviderSession session, Dictionary<String, String> tokendata, ICloudStorageAccessToken token);

        /// <summary>
        /// Loads the information from a token stream 
        /// </summary>
        ICloudStorageAccessToken LoadToken(Dictionary<String, String> tokendata);

        /// <summary>
        /// The method returns the url to a specific resource
        /// </summary>
        /// <param name="session"></param>
        /// <param name="fileSystemEntry"></param>
        /// <param name="additionalPath"></param>
        /// <returns></returns>
        String GetResourceUrl(IStorageProviderSession session, ICloudFileSystemEntry fileSystemEntry, string additionalPath);

        /// <summary>
        /// This interface implements the download code
        /// </summary>
        /// <param name="session"></param>
        /// <param name="fileSystemEntry"></param>
        /// <param name="targetDataStream"></param>
        /// <param name="progressCallback"></param>
        /// <param name="progressContext"></param>
        void DownloadResourceContent(IStorageProviderSession session, ICloudFileSystemEntry fileSystemEntry, Stream targetDataStream, FileOperationProgressChanged progressCallback, Object progressContext);

        /// <summary>
        /// Allows the access to the download stream directly
        /// </summary>
        /// <param name="session"></param>
        /// <param name="fileSystemEntry"></param>
        /// <returns></returns>
        Stream CreateDownloadStream(IStorageProviderSession session, ICloudFileSystemEntry fileSystemEntry);

        /// <summary>
        /// This method is called when the stream operation is finished but before the 
        /// dispose method of the stream will be called
        /// </summary>
        /// <param name="session"></param>
        /// <param name="fileSystemEntry"></param>
        /// <param name="direction"></param>
        /// <param name="notDisposedStream"></param>
        void CommitStreamOperation(IStorageProviderSession session, ICloudFileSystemEntry fileSystemEntry, nTransferDirection direction, Stream notDisposedStream);

        /// <summary>
        /// This interface implements the upload code
        /// </summary>
        /// <param name="session"></param>
        /// <param name="fileSystemEntry"></param>
        /// <param name="targetDataStream"></param>
        /// <param name="progressCallback"></param>
        /// <param name="progressContext"></param>
        void UploadResourceContent(IStorageProviderSession session, ICloudFileSystemEntry fileSystemEntry, Stream targetDataStream, FileOperationProgressChanged progressCallback, Object progressContext);

        /// <summary>
        /// Allows the access to the upload stream directly
        /// </summary>
        /// <param name="session"></param>
        /// <param name="fileSystemEntry"></param>
        /// <param name="uploadSize"></param>
        /// <returns></returns>
        Stream CreateUploadStream(IStorageProviderSession session, ICloudFileSystemEntry fileSystemEntry, long uploadSize);

        bool SupportsDirectRetrieve { get; }

        bool SupportsChunking { get; }

        IResumableUploadSession CreateUploadSession(IStorageProviderSession session, ICloudFileSystemEntry fileSystemEntry, long bytesToTransfer);

        void AbortUploadSession(IStorageProviderSession session, IResumableUploadSession uploadSession);

        void UploadChunk(IStorageProviderSession session, IResumableUploadSession uploadSession, Stream stream, long chunkLength);
    }
}