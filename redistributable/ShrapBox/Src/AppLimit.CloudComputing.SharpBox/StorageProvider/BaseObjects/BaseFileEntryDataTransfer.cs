using System;
using System.IO;
using AppLimit.CloudComputing.SharpBox.Exceptions;
using AppLimit.CloudComputing.SharpBox.StorageProvider.API;
using System.Threading;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.BaseObjects
{
    internal class BaseFileEntryDataTransfer : ICloudFileDataTransfer
    {
        private readonly ICloudFileSystemEntry _fsEntry;
        private readonly IStorageProviderService _service;
        private readonly IStorageProviderSession _session;

        private class BaseFileEntryDataTransferAsyncContext
        {
            public Object ProgressContext { get; set; }
            public FileOperationProgressChanged ProgressCallback { get; set; }
        }

        public BaseFileEntryDataTransfer(ICloudFileSystemEntry fileSystemEntry, IStorageProviderService service, IStorageProviderSession session)
        {
            _fsEntry = fileSystemEntry;
            _session = session;
            _service = service;
        }

        #region ICloudFileDataTransfer Members

        public void Transfer(Stream targetDataStream, nTransferDirection direction)
        {
            Transfer(targetDataStream, direction, null, null);
        }

        public void Transfer(Stream targetDataStream, nTransferDirection direction, FileOperationProgressChanged progressCallback, object progressContext)
        {
            if (direction == nTransferDirection.nUpload)
            {
                if (!CanTransfer(direction, targetDataStream.Length))
                    throw new SharpBoxException(SharpBoxErrorCodes.ErrorLimitExceeded);

                _service.UploadResourceContent(_session, _fsEntry, targetDataStream, progressCallback, progressContext);
            }
            else
            {
                if (!CanTransfer(direction, _fsEntry.Length))
                    throw new SharpBoxException(SharpBoxErrorCodes.ErrorLimitExceeded);

                _service.DownloadResourceContent(_session, _fsEntry, targetDataStream, progressCallback, progressContext);
            }
        }

        public void TransferAsyncProgress(Stream targetDataStream, nTransferDirection direction, FileOperationProgressChanged progressCallback, object progressContext)
        {
            var ctx = new BaseFileEntryDataTransferAsyncContext
                {
                    ProgressCallback = progressCallback,
                    ProgressContext = progressContext
                };

            Transfer(targetDataStream, direction, FileOperationProgressChangedAsyncHandler, ctx);
        }


        public Stream GetDownloadStream()
        {
            return _service.CreateDownloadStream(_session, _fsEntry);
        }

        public Stream GetUploadStream(long uploadSize)
        {
            return _service.CreateUploadStream(_session, _fsEntry, uploadSize);
        }

        public IResumableUploadSession CreateResumableSession(long bytesToTransfer)
        {
            if (!CanTransfer(nTransferDirection.nUpload, bytesToTransfer, true))
                throw new SharpBoxException(SharpBoxErrorCodes.ErrorLimitExceeded);

            return _service.SupportsChunking ? _service.CreateUploadSession(_session, _fsEntry, bytesToTransfer) : null;
        }

        public void AbortResumableSession(IResumableUploadSession transferSession)
        {
            _service.AbortUploadSession(_session, transferSession);
        }

        public void Transfer(IResumableUploadSession transferSession, Stream data, long dataLength)
        {
            _service.UploadChunk(_session, transferSession, data, dataLength);
        }

        void ICloudFileDataTransfer.Serialize(System.Runtime.Serialization.IFormatter dataFormatter, object objectGraph)
        {
            using (var cache = new MemoryStream())
            {
                // serialize into the cache
                dataFormatter.Serialize(cache, objectGraph);

                // go to start
                cache.Position = 0;

                // transfer the cache
                _fsEntry.GetDataTransferAccessor().Transfer(cache, nTransferDirection.nUpload);
            }
        }

        object ICloudFileDataTransfer.Deserialize(System.Runtime.Serialization.IFormatter dataFormatter)
        {
            using (var cache = new MemoryStream())
            {
                // get the data
                _fsEntry.GetDataTransferAccessor().Transfer(cache, nTransferDirection.nDownload);

                // go to the start
                cache.Position = 0;

                // go ahead
                return dataFormatter.Deserialize(cache);
            }
        }

        #endregion

        #region Internal callbacks

        private static void FileOperationProgressChangedAsyncHandler(object sender, FileDataTransferEventArgs e)
        {
            var ctx = e.CustomnContext as BaseFileEntryDataTransferAsyncContext;

            // define the thread
            ThreadPool.QueueUserWorkItem(state =>
                {
                    // change the transferevent args
                    var eAsync = e.Clone() as FileDataTransferEventArgs;
                    ctx.ProgressCallback(sender, eAsync);
                });
        }

        #endregion

        private bool CanTransfer(nTransferDirection direction, long bytes, bool useChunks = false)
        {
            var currentLimitss = _service.GetLimits(_session);

            long limit;
            if (direction == nTransferDirection.nDownload)
            {
                limit = currentLimitss.MaxDownloadFileSize;
            }
            else if (!useChunks)
            {
                limit = currentLimitss.MaxUploadFileSize;
            }
            else
            {
                limit = currentLimitss.MaxChunkedUploadFileSize;
            }

            return limit == -1 || limit >= bytes;
        }
    }
}