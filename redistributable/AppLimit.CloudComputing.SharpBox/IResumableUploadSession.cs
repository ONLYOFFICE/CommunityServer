using System;

namespace AppLimit.CloudComputing.SharpBox
{
    public enum ResumableUploadSessionStatus
    {
        None,
        Started,
        Completed,
        Aborted
    }

    public interface IResumableUploadSession
    {
        long BytesToTransfer { get; }

        long BytesTransfered { get; }

        ICloudFileSystemEntry File { get; }

        ResumableUploadSessionStatus Status { get; }

        DateTime CreatedOn { get; }

        object GetItem(string key);

        T GetItem<T>(string key);
    }
}