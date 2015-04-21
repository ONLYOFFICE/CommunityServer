using System;
using System.Collections.Generic;
using AppLimit.CloudComputing.SharpBox.StorageProvider.API;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.BaseObjects
{
    internal class BaseFileEntry : ICloudFileSystemEntry
    {
        protected String _id;
        protected String _name;
        protected ICloudDirectoryEntry _parent;
        protected String _parentID;
        protected IStorageProviderService _service;
        protected IStorageProviderSession _session;

        private Dictionary<String, Object> _properties = new Dictionary<string, Object>();

        internal Boolean IsDeleted { get; set; }

        public BaseFileEntry(String Name, long Length, DateTime Modified, IStorageProviderService service, IStorageProviderSession session)
        {
            this.Name = Name;
            this.Length = Length;
            this.Modified = Modified;
            _service = new CachedServiceWrapper(service); //NOTE: Caching
            _session = session;

            IsDeleted = false;
        }

        #region ICloudFileSystemEntry Members

        public long Length { get; set; }
        public DateTime Modified { get; set; }

        public String Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public String Id
        {
            get { return !String.IsNullOrEmpty(_id) ? _id : _name; }
            set { _id = value; }
        }

        public String ParentID
        {
            get
            {
                if (!String.IsNullOrEmpty(_parentID))
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
                if (!String.IsNullOrEmpty(_parentID))
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

        public String this[String property]
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

        public void SetPropertyValue(string key, Object value)
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