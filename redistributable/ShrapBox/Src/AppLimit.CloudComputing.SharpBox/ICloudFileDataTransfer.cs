using System;
using System.IO;
using System.Runtime.Serialization;

namespace AppLimit.CloudComputing.SharpBox
{
    /// <summary>
    /// This enum describes the data transfer direction
    /// </summary>
    public enum nTransferDirection
    {
        /// <summary>
        /// Defines that the target data stream should be uploaded into the cloud file container
        /// </summary>
        nUpload,

        /// <summary>
        /// Defines that the data from the cloud file container should be downloaded into the target data stream
        /// </summary>
        nDownload
    };


    /// <summary>
    /// This class contains the arguments which can be used when a data transfer operation is running
    /// </summary>
    public class FileDataTransferEventArgs : EventArgs, ICloneable
    {
        /// <summary>
        /// Set this to true to abort the transfer
        /// </summary>
        public Boolean Cancel { get; set; }

        /// <summary>
        /// The amount of bytes currently transfered
        /// </summary>
        public long CurrentBytes { get; internal set; }

        /// <summary>
        /// The amount of bytes which has to be transfered in total
        /// </summary>
        public long TotalBytes { get; internal set; }

        /// <summary>
        /// Overall progress in %
        /// </summary>
        public int PercentageProgress { get; internal set; }

        /// <summary>
        /// The calculated transfer rate for this operation overall(Kbits/s)
        /// </summary>
        public long TransferRateTotal { get; internal set; }

        /// <summary>
        /// The current messured transfer rate (Kbits/s)
        /// </summary>
        public long TransferRateCurrent { get; internal set; }

        /// <summary>
        /// The estimated finish time if this transfer
        /// </summary>
        public TimeSpan OpenTransferTime { get; internal set; }

        /// <summary>
        /// A reference to the associated cloud file system entry
        /// </summary>
        public ICloudFileSystemEntry FileSystemEntry { get; internal set; }

        /// <summary>
        /// A use specific context
        /// </summary>
        public Object CustomnContext { get; internal set; }

        /// <summary>
        /// ctor
        /// </summary>
        public FileDataTransferEventArgs()
        {
            Cancel = false;
        }

        #region ICloneable Members

        /// <summary>
        /// This method implements to clone interface to support a deep copy 
        /// of everything what we use to report the progress of a operation
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            var e = new FileDataTransferEventArgs
                {
                    CurrentBytes = CurrentBytes,
                    CustomnContext = CustomnContext,
                    FileSystemEntry = FileSystemEntry,
                    OpenTransferTime = OpenTransferTime,
                    PercentageProgress = PercentageProgress,
                    TotalBytes = TotalBytes,
                    TransferRateCurrent = TransferRateCurrent,
                    TransferRateTotal = TransferRateTotal,
                    Cancel = false
                };

            return e;
        }

        #endregion
    }

    /// <summary>
    /// This delegate can be used as callback for upload or download operation in the 
    /// data streams.
    /// </summary>
    /// <param name="sender">sender object of the event</param>
    /// <param name="e">event args</param>    
    public delegate void FileOperationProgressChanged(object sender, FileDataTransferEventArgs e);

    /// <summary>
    /// This interface implements a specifc transfer logic which can be used
    /// to transport data from a local data stream to a remote filesystem entry 
    /// and back
    /// </summary>
    public interface ICloudFileDataTransfer
    {
        /// <summary>
        /// This method transfers data between a local data stream and the remote filesystem entry on
        /// byte level
        /// </summary>
        /// <param name="targetDataStream"></param>
        /// <param name="direction"></param>        
        void Transfer(Stream targetDataStream, nTransferDirection direction);

        /// <summary>
        /// This method transfers data between a local data stream and the remote filesystem entry on
        /// byte level
        /// </summary>
        /// <param name="targetDataStream"></param>
        /// <param name="direction"></param>
        /// <param name="progressCallback"></param>
        /// <param name="progressContext"></param>
        void Transfer(Stream targetDataStream, nTransferDirection direction, FileOperationProgressChanged progressCallback, Object progressContext);

        /// <summary>
        /// This method transfers data between a local data stream and the remote filesystem entry on
        /// byte level. This API triggers the callback in an async manner this means the transfer process
        /// is not blocked during the consumer is in the handler
        /// </summary>
        /// <param name="targetDataStream"></param>
        /// <param name="direction"></param>
        /// <param name="progressCallback"></param>
        /// <param name="progressContext"></param>
        void TransferAsyncProgress(Stream targetDataStream, nTransferDirection direction, FileOperationProgressChanged progressCallback, Object progressContext);

        /// <summary>
        /// Allows native access to the download stream of the associated file. 
        /// Ensure that this stream will be disposed clearly!
        /// </summary>
        Stream GetDownloadStream();

        /// <summary>
        /// Allows native access to the upload stream of the associated file
        /// Ensure that this stream will be disposed clearly!
        /// </summary>
        /// <param name="uploadSize"></param>        
        Stream GetUploadStream(long uploadSize);

        /// <summary>
        /// This method supports the serialization of object graphs into the remote file container
        /// </summary>        
        /// <param name="dataFormatter"></param>
        /// <param name="objectGraph"></param>
        void Serialize(IFormatter dataFormatter, Object objectGraph);

        /// <summary>
        /// This method allows to deserialize an object graph from the remote file container
        /// </summary>        
        /// <param name="dataFormatter"></param>
        /// <returns></returns>
        Object Deserialize(IFormatter dataFormatter);

        IResumableUploadSession CreateResumableSession(long bytesToTransfer);

        void AbortResumableSession(IResumableUploadSession transferSession);

        void Transfer(IResumableUploadSession transferSession, Stream data, long dataLength);
    }
}