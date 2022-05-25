using System;
using System.Collections.Generic;
using System.IO;
using AppLimit.CloudComputing.SharpBox.Common.Cache;
using AppLimit.CloudComputing.SharpBox.StorageProvider.API;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider
{
    public class CachedServiceWrapper : IStorageProviderService
    {
        private static readonly CachedDictionary<ICloudFileSystemEntry> FsCache =
            new CachedDictionary<ICloudFileSystemEntry>("sbox-fs", TimeSpan.FromSeconds(120), TimeSpan.Zero, x => x != null);

        private readonly IStorageProviderService _service;

        public CachedServiceWrapper(IStorageProviderService service)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            _service = service;
        }

        public bool VerifyAccessTokenType(ICloudStorageAccessToken token)
        {
            return _service.VerifyAccessTokenType(token);
        }

        public IStorageProviderSession CreateSession(ICloudStorageAccessToken token, ICloudStorageConfiguration configuration)
        {
            return _service.CreateSession(token, configuration);
        }

        public void CloseSession(IStorageProviderSession session)
        {
            FsCache.Reset(GetSessionKey(session), string.Empty);
            _service.CloseSession(session);
        }

        public CloudStorageLimits GetLimits(IStorageProviderSession session)
        {
            return _service.GetLimits(session);
        }

        public ICloudFileSystemEntry RequestResource(IStorageProviderSession session, string name, ICloudDirectoryEntry parent)
        {
            return FsCache.Get(GetSessionKey(session), GetCacheKey(session, name, parent), () => _service.RequestResource(session, name, parent));
        }

        private static string GetSessionKey(IStorageProviderSession session)
        {
            return session.GetType().Name + " " + session.SessionToken;
        }

        private string GetCacheKey(IStorageProviderSession session, string nameOrId, ICloudFileSystemEntry parent)
        {
            return GetResourceUrl(session, parent, nameOrId);
        }


        public void RefreshResource(IStorageProviderSession session, ICloudFileSystemEntry resource)
        {
            var cached = FsCache.Get(GetSessionKey(session), GetCacheKey(session, null, resource), null) as ICloudDirectoryEntry;

            if (cached == null || cached.HasChildrens == nChildState.HasNotEvaluated)
            {
                _service.RefreshResource(session, resource);
                FsCache.Add(GetSessionKey(session), GetCacheKey(session, null, resource), resource);
            }
        }

        public ICloudFileSystemEntry CreateResource(IStorageProviderSession session, string name, ICloudDirectoryEntry parent)
        {
            return _service.CreateResource(session, name, parent);
        }

        public bool DeleteResource(IStorageProviderSession session, ICloudFileSystemEntry entry)
        {
            FsCache.Reset(GetSessionKey(session), string.Empty);
            return _service.DeleteResource(session, entry);
        }

        public bool MoveResource(IStorageProviderSession session, ICloudFileSystemEntry fsentry, ICloudDirectoryEntry newParent)
        {
            FsCache.Reset(GetSessionKey(session), string.Empty);
            return _service.MoveResource(session, fsentry, newParent);
        }

        public bool CopyResource(IStorageProviderSession session, ICloudFileSystemEntry fsentry, ICloudDirectoryEntry newParent)
        {
            FsCache.Reset(GetSessionKey(session), string.Empty);
            return _service.CopyResource(session, fsentry, newParent);
        }

        public bool RenameResource(IStorageProviderSession session, ICloudFileSystemEntry fsentry, string newName)
        {
            FsCache.Reset(GetSessionKey(session), string.Empty);
            return _service.RenameResource(session, fsentry, newName);
        }

        public void StoreToken(IStorageProviderSession session, Dictionary<string, string> tokendata, ICloudStorageAccessToken token)
        {
            _service.StoreToken(session, tokendata, token);
        }

        public ICloudStorageAccessToken LoadToken(Dictionary<string, string> tokendata)
        {
            return _service.LoadToken(tokendata);
        }

        public string GetResourceUrl(IStorageProviderSession session, ICloudFileSystemEntry fileSystemEntry, string additionalPath)
        {
            return _service.GetResourceUrl(session, fileSystemEntry, additionalPath);
        }

        public void DownloadResourceContent(IStorageProviderSession session, ICloudFileSystemEntry fileSystemEntry, Stream targetDataStream, FileOperationProgressChanged progressCallback, object progressContext)
        {
            _service.DownloadResourceContent(session, fileSystemEntry, targetDataStream, progressCallback, progressContext);
        }

        public Stream CreateDownloadStream(IStorageProviderSession session, ICloudFileSystemEntry fileSystemEntry)
        {
            return _service.CreateDownloadStream(session, fileSystemEntry);
        }

        public void CommitStreamOperation(IStorageProviderSession session, ICloudFileSystemEntry fileSystemEntry, nTransferDirection direction, Stream notDisposedStream)
        {
            _service.CommitStreamOperation(session, fileSystemEntry, direction, notDisposedStream);
        }

        public void UploadResourceContent(IStorageProviderSession session, ICloudFileSystemEntry fileSystemEntry, Stream targetDataStream, FileOperationProgressChanged progressCallback, object progressContext)
        {
            _service.UploadResourceContent(session, fileSystemEntry, targetDataStream, progressCallback, progressContext);
        }

        public Stream CreateUploadStream(IStorageProviderSession session, ICloudFileSystemEntry fileSystemEntry, long uploadSize)
        {
            return _service.CreateUploadStream(session, fileSystemEntry, uploadSize);
        }

        public bool SupportsDirectRetrieve
        {
            get { return _service.SupportsDirectRetrieve; }
        }


        public bool SupportsChunking
        {
            get { return _service.SupportsChunking; }
        }

        public IResumableUploadSession CreateUploadSession(IStorageProviderSession session, ICloudFileSystemEntry fileSystemEntry, long bytesToTransfer)
        {
            return _service.CreateUploadSession(session, fileSystemEntry, bytesToTransfer);
        }

        public void AbortUploadSession(IStorageProviderSession session, IResumableUploadSession uploadSession)
        {
            _service.AbortUploadSession(session, uploadSession);
        }

        public void UploadChunk(IStorageProviderSession session, IResumableUploadSession uploadSession, Stream stream, long chunkLength)
        {
            _service.UploadChunk(session, uploadSession, stream, chunkLength);
        }
    }
}