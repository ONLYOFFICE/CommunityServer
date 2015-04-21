using System;
using System.Collections.Generic;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.BaseObjects
{
    internal class ResumableUploadSession : IResumableUploadSession
    {
        private readonly Dictionary<string, object> _items = new Dictionary<string, object>();
        private readonly DateTime _createdOn = DateTime.UtcNow;

        public long BytesToTransfer { get; set; }

        public long BytesTransfered { get; set; }

        public ICloudFileSystemEntry File { get; set; }

        public ResumableUploadSessionStatus Status { get; set; }

        public DateTime CreatedOn
        {
            get { return _createdOn; }
        }

        public object this[string key]
        {
            get { return GetItem(key); }
            set { SetItem(key, value); }
        }

        public ResumableUploadSession(ICloudFileSystemEntry file, long bytesToTransfer)
        {
            File = file;
            BytesToTransfer = bytesToTransfer;
            Status = ResumableUploadSessionStatus.None;
        }

        public void SetItem(string key, object value)
        {
            _items[key] = value;
        }

        public object GetItem(string key)
        {
            return _items.ContainsKey(key) ? _items[key] : null;
        }

        public T GetItem<T>(string key)
        {
            var item = GetItem(key);
            if (item is T)
            {
                return (T)item;
            }
            return default(T);
        }
    }
}
