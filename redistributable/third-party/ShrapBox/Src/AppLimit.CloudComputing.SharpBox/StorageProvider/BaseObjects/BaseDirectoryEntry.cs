using System;
using System.Collections.Generic;
using System.Linq;
using AppLimit.CloudComputing.SharpBox.Exceptions;
using AppLimit.CloudComputing.SharpBox.StorageProvider.API;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.BaseObjects
{
    internal class BaseDirectoryEntry : BaseFileEntry, ICloudDirectoryEntry
    {
        private Dictionary<String, ICloudFileSystemEntry> _subDirectories = new Dictionary<string, ICloudFileSystemEntry>(StringComparer.OrdinalIgnoreCase);
        private List<ICloudFileSystemEntry> _subDirectoriesByIndex = new List<ICloudFileSystemEntry>();
        private Boolean _subDirectoriesRefreshedInitially = false;

        public BaseDirectoryEntry(String Name, long Length, DateTime Modified, IStorageProviderService service, IStorageProviderSession session)
            : base(Name, Length, Modified, service, session)
        {
        }

        #region ICloudDirectoryEntry Members

        public ICloudFileSystemEntry GetChild(string id)
        {
            return GetChild(id, true);
        }

        public ICloudFileSystemEntry GetChild(string id, Boolean bThrowException)
        {
            // refresh this item to ensure that the parents
            // are valud
            RefreshResource();

            // query subitems
            ICloudFileSystemEntry pe;
            _subDirectories.TryGetValue(id, out pe);

            // send the exception if needed
            if (bThrowException && pe == null)
                throw new SharpBoxException(SharpBoxErrorCodes.ErrorFileNotFound);

            // go ahead
            return pe;
        }

        public ICloudFileSystemEntry GetChild(int idx)
        {
            // refresh this item to ensure that the parents
            // are valud
            RefreshResource();

            // request the information
            return idx < _subDirectoriesByIndex.Count ? _subDirectoriesByIndex[idx] : null;
        }

        public int Count
        {
            get
            {
                // go ahead
                return _subDirectories.Count;
            }
        }

        #endregion

        #region IEnumerable<ICloudFileSystemEntry> Members

        public IEnumerator<ICloudFileSystemEntry> GetEnumerator()
        {
            // refresh this item to ensure that the parents
            // are valud
            RefreshResource();

            // go ahead
            return _subDirectories.Values.GetEnumerator();
        }

        internal IEnumerable<ICloudFileSystemEntry> GetSubdirectoriesWithoutRefresh()
        {
            // go ahead
            return _subDirectories.Values;
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion


        public void AddChild(BaseFileEntry child)
        {
            // remove child if available
            //RemoveChild(child);

            //don't need to remove child if contains; it's must be up to date due ProviderService's
            if (_subDirectories.ContainsKey(child.Id))
            {
                child.Parent = this;
                return;   
            }

            // add the new child
            _subDirectories.Add(child.Id, child);
            _subDirectoriesByIndex.Add(child);

            // set the parent
            child.Parent = this;
        }

        public void AddChilds(IEnumerable<BaseFileEntry> childs)
        {
            foreach (BaseFileEntry entry in childs)
                AddChild(entry);
        }

        public void RemoveChild(BaseFileEntry child)
        {
            RemoveChildById(child.Id);
        }

        public void RemoveChildById(String childId)
        {
            // check for replace
            if (_subDirectories.ContainsKey(childId))
            {
                // get the old child
                ICloudFileSystemEntry oldChild = _subDirectories[childId];

                // remove it
                _subDirectories.Remove(childId);
                _subDirectoriesByIndex.Remove(oldChild);

                // remove parent link
                oldChild.Parent = null;
            }
        }

        public void ClearChilds()
        {
            _subDirectories.Clear();
            _subDirectoriesByIndex.Clear();
        }

        public nChildState HasChildrens
        {
            get
            {
                if (_subDirectories.Count != 0)
                    return nChildState.HasChilds;
                else if (!_subDirectoriesRefreshedInitially)
                    return nChildState.HasNotEvaluated;
                else
                    return nChildState.HasNoChilds;
            }
        }

        private void RefreshResource()
        {
            // refresh the childs
            _service.RefreshResource(_session, this);

            // set the state
            _subDirectoriesRefreshedInitially = true;
        }
    }
}