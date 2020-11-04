/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Web;
using ASC.Common.Caching;
using ASC.Common.Web;
using ASC.Core;
using ASC.FederatedLogin;
using ASC.Files.Core;
using Dropbox.Api.Files;

namespace ASC.Files.Thirdparty.Dropbox
{
    [DebuggerDisplay("{CustomerTitle}")]
    public class DropboxProviderInfo : IProviderInfo, IDisposable
    {
        private readonly OAuth20Token _token;
        private readonly FolderType _rootFolderType;
        private readonly DateTime _createOn;

        internal DropboxStorage Storage
        {
            get
            {
                if (HttpContext.Current != null)
                {
                    var key = "__DROPBOX_STORAGE" + ID;
                    var wrapper = (StorageDisposableWrapper)DisposableHttpContext.Current[key];
                    if (wrapper == null || !wrapper.Storage.IsOpened)
                    {
                        wrapper = new StorageDisposableWrapper(CreateStorage());
                        DisposableHttpContext.Current[key] = wrapper;
                    }
                    return wrapper.Storage;
                }
                return CreateStorage();
            }
        }

        internal bool StorageOpened
        {
            get
            {
                if (HttpContext.Current != null)
                {
                    var key = "__DROPBOX_STORAGE" + ID;
                    var wrapper = (StorageDisposableWrapper)DisposableHttpContext.Current[key];
                    return wrapper != null && wrapper.Storage.IsOpened;
                }
                return false;
            }
        }

        public int ID { get; set; }

        public Guid Owner { get; private set; }

        public string CustomerTitle { get; private set; }

        public DateTime CreateOn
        {
            get { return _createOn; }
        }

        public object RootFolderId
        {
            get { return "dropbox-" + ID; }
        }

        public string ProviderKey { get; private set; }

        public FolderType RootFolderType
        {
            get { return _rootFolderType; }
        }


        public DropboxProviderInfo(int id, string providerKey, string customerTitle, string token, Guid owner, FolderType rootFolderType, DateTime createOn)
        {
            if (string.IsNullOrEmpty(providerKey)) throw new ArgumentNullException("providerKey");
            if (string.IsNullOrEmpty(token)) throw new ArgumentException("Token can't be null");

            ID = id;
            CustomerTitle = customerTitle;
            Owner = owner == Guid.Empty ? SecurityContext.CurrentAccount.ID : owner;

            ProviderKey = providerKey;
            _token = OAuth20Token.FromJson(token);
            _rootFolderType = rootFolderType;
            _createOn = createOn;
        }

        public void Dispose()
        {
            if (StorageOpened)
                Storage.Close();
        }

        public bool CheckAccess()
        {
            try
            {
                Storage.GetUsedSpace();
            }
            catch (AggregateException)
            {
                return false;
            }
            return true;
        }

        public void InvalidateStorage()
        {
            if (HttpContext.Current != null)
            {
                var key = "__DROPBOX_STORAGE" + ID;
                var storage = (StorageDisposableWrapper)DisposableHttpContext.Current[key];
                if (storage != null)
                {
                    storage.Dispose();
                }
            }
            CacheReset();
        }

        internal void UpdateTitle(string newtitle)
        {
            CustomerTitle = newtitle;
        }

        private DropboxStorage CreateStorage()
        {
            var dropboxStorage = new DropboxStorage();

            dropboxStorage.Open(_token);
            return dropboxStorage;
        }


        private class StorageDisposableWrapper : IDisposable
        {
            public DropboxStorage Storage { get; private set; }


            public StorageDisposableWrapper(DropboxStorage storage)
            {
                Storage = storage;
            }

            public void Dispose()
            {
                Storage.Close();
            }
        }


        private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(1);
        private static readonly ICache CacheFile = AscCache.Memory;
        private static readonly ICache CacheFolder = AscCache.Memory;
        private static readonly ICache CacheChildItems = AscCache.Memory;
        private static readonly ICacheNotify CacheNotify;

        static DropboxProviderInfo()
        {
            CacheNotify = AscCache.Notify;
            CacheNotify.Subscribe<DropboxCacheItem>((i, action) =>
                {
                    if (action != CacheNotifyAction.Remove) return;
                    if (i.ResetAll)
                    {
                        CacheFile.Remove(new Regex("^dropboxf-" + i.Key + ".*"));
                        CacheFolder.Remove(new Regex("^dropboxd-" + i.Key + ".*"));
                        CacheChildItems.Remove(new Regex("^dropbox-" + i.Key + ".*"));
                    }

                    if (!i.IsFile.HasValue)
                    {
                        CacheChildItems.Remove("dropbox-" + i.Key);

                        CacheFolder.Remove("dropboxd-" + i.Key);
                    }
                    else
                    {
                        if (i.IsFile.Value)
                        {
                            CacheFile.Remove("dropboxf-" + i.Key);
                        }
                        else
                        {
                            CacheFolder.Remove("dropboxd-" + i.Key);
                        }
                    }
                });
        }

        internal FolderMetadata GetDropboxFolder(string dropboxFolderPath)
        {
            var folder = CacheFolder.Get<FolderMetadata>("dropboxd-" + ID + "-" + dropboxFolderPath);
            if (folder == null)
            {
                folder = Storage.GetFolder(dropboxFolderPath);
                if (folder != null)
                    CacheFolder.Insert("dropboxd-" + ID + "-" + dropboxFolderPath, folder, DateTime.UtcNow.Add(CacheExpiration));
            }
            return folder;
        }

        internal FileMetadata GetDropboxFile(string dropboxFilePath)
        {
            var file = CacheFile.Get<FileMetadata>("dropboxf-" + ID + "-" + dropboxFilePath);
            if (file == null)
            {
                file = Storage.GetFile(dropboxFilePath);
                if (file != null)
                    CacheFile.Insert("dropboxf-" + ID + "-" + dropboxFilePath, file, DateTime.UtcNow.Add(CacheExpiration));
            }
            return file;
        }

        internal List<Metadata> GetDropboxItems(string dropboxFolderPath)
        {
            var items = CacheChildItems.Get<List<Metadata>>("dropbox-" + ID + "-" + dropboxFolderPath);

            if (items == null)
            {
                items = Storage.GetItems(dropboxFolderPath);
                CacheChildItems.Insert("dropbox-" + ID + "-" + dropboxFolderPath, items, DateTime.UtcNow.Add(CacheExpiration));
            }
            return items;
        }

        internal void CacheReset(Metadata dropboxItem)
        {
            if (dropboxItem != null)
            {
                CacheNotify.Publish(new DropboxCacheItem { IsFile = dropboxItem.AsFolder != null, Key = ID + "-" + dropboxItem.PathDisplay }, CacheNotifyAction.Remove);
            }
        }

        internal void CacheReset(string dropboxPath = null, bool? isFile = null)
        {
            var key = ID + "-";
            if (dropboxPath == null)
            {
                CacheNotify.Publish(new DropboxCacheItem { ResetAll = true, Key = key }, CacheNotifyAction.Remove);
            }
            else
            {
                key += dropboxPath;

                CacheNotify.Publish(new DropboxCacheItem { IsFile = isFile, Key = key }, CacheNotifyAction.Remove);
            }
        }

        private class DropboxCacheItem
        {
            public bool ResetAll { get; set; }
            public bool? IsFile { get; set; }
            public string Key { get; set; }
        }
    }
}