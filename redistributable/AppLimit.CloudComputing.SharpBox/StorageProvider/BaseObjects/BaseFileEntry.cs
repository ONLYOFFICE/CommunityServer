using System;
using System.Collections.Generic;
using AppLimit.CloudComputing.SharpBox.StorageProvider.API;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.BaseObjects
{
    internal class BaseFileEntry : ICloudFileSystemEntry
    {
        protected string _id;
        protected string _name;
        protected ICloudDirectoryEntry _parent;
        protected string _parentID;
        protected IStorageProviderService _service;
        protected IStorageProviderSession _session;

        private Dictionary<string, object> _properties = new Dictionary<string, object>();

        internal bool IsDeleted { get; set; }

        public BaseFileEntry(string name, long length, DateTime modified, IStorageProviderService service, IStorageProviderSession session)
        {
            Name = name;
            Length = length;
            Modified = modified;
            _service = new CachedServiceWrapper(service); //NOTE: Caching
            _session = session;

            IsDeleted = false;
        }

        #region ICloudFileSystemEntry Members

        public long Length { get; set; }
        public DateTime Modified { get; set; }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string Id
        {
            get { return !string.IsNullOrEmpty(_id) ? _id : _name; }
            set { _id = value; }
        }

        public string ParentID
        {
            get
            {
                if (!string.IsNullOrEmpty(_parentID))
                    return _parentID;
                if (_parent != null)
                {
                    _parentID = _parent.Id;
                    return _parentID;
                }
                return null;
            }
            set { _parentID = value; }
        }

        public ICloudDirectoryEntry Parent
        {
            get
            {
                if (_parent != null)
                    return _parent;
                if (!string.IsNullOrEmpty(_parentID))
                {
                    _parent = (ICloudDirectoryEntry)_service.RequestResource(_session, _parentID, null); //get parent by ID
                    return _parent;
                }
                return null;
            }
            set
            {
                _parent = value;
                if (_parent != null)
                    _parentID = _parent.Id;
            }
        }

        public string this[string property]
        {
            get { return GetPropertyValue(property); }
            set { SetPropertyValue(property, value); }
        }

        public string GetPropertyValue(string key)
        {
            object value;
            if (_properties.TryGetValue(key, out value))
            {
                return value.ToString();
            }
            return string.Empty;
        }

        public void SetPropertyValue(string key, object value)
        {
            if (_properties.ContainsKey(key))
                _properties.Remove(key);

            _properties.Add(key, value);
        }

        public ICloudFileDataTransfer GetDataTransferAccessor()
        {
            return new BaseFileEntryDataTransfer(this, _service, _session);
        }

        #endregion
    }
}