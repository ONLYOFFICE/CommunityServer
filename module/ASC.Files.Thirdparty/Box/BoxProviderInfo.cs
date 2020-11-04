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
using ASC.FederatedLogin.Helpers;
using ASC.FederatedLogin.LoginProviders;
using ASC.Files.Core;
using Box.V2.Models;

namespace ASC.Files.Thirdparty.Box
{
    [DebuggerDisplay("{CustomerTitle}")]
    public class BoxProviderInfo : IProviderInfo, IDisposable
    {
        private OAuth20Token _token;
        private readonly FolderType _rootFolderType;
        private readonly DateTime _createOn;
        private string _rootId;

        internal BoxStorage Storage
        {
            get
            {
                if (HttpContext.Current != null)
                {
                    var key = "__BOX_STORAGE" + ID;
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
                    var key = "__BOX_STORAGE" + ID;
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
            get { return "box-" + ID; }
        }

        public string ProviderKey { get; private set; }

        public FolderType RootFolderType
        {
            get { return _rootFolderType; }
        }

        public string BoxRootId
        {
            get
            {
                if (string.IsNullOrEmpty(_rootId))
                {
                    _rootId = Storage.GetRootFolderId();
                }
                return _rootId;
            }
        }


        public BoxProviderInfo(int id, string providerKey, string customerTitle, string token, Guid owner, FolderType rootFolderType, DateTime createOn)
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
                return !string.IsNullOrEmpty(BoxRootId);
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        public void InvalidateStorage()
        {
            if (HttpContext.Current != null)
            {
                var key = "__BOX_STORAGE" + ID;
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

        private BoxStorage CreateStorage()
        {
            var boxStorage = new BoxStorage();

            CheckToken();

            boxStorage.Open(_token);
            return boxStorage;
        }

        private void CheckToken()
        {
            if (_token == null) throw new UnauthorizedAccessException("Cannot create Box session with given token");
            if (_token.IsExpired)
            {
                _token = OAuth20TokenHelper.RefreshToken<BoxLoginProvider>(_token);

                using (var dbDao = new CachedProviderAccountDao(CoreContext.TenantManager.GetCurrentTenant().TenantId, FileConstant.DatabaseId))
                {
                    dbDao.UpdateProviderInfo(ID, new AuthData(token: _token.ToJson()));
                }
            }
        }


        private class StorageDisposableWrapper : IDisposable
        {
            public BoxStorage Storage { get; private set; }


            public StorageDisposableWrapper(BoxStorage storage)
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

        static BoxProviderInfo()
        {
            CacheNotify = AscCache.Notify;
            CacheNotify.Subscribe<BoxCacheItem>((i, action) =>
                {
                    if (action != CacheNotifyAction.Remove) return;
                    if (i.ResetAll)
                    {
                        CacheChildItems.Remove(new Regex("^box-" + i.Key + ".*"));
                        CacheFile.Remove(new Regex("^boxf-" + i.Key + ".*"));
                        CacheFolder.Remove(new Regex("^boxd-" + i.Key + ".*"));
                    }

                    if (!i.IsFile.HasValue)
                    {
                        CacheChildItems.Remove("box-" + i.Key);

                        CacheFolder.Remove("boxd-" + i.Key);
                    }
                    else
                    {
                        if (i.IsFile.Value)
                        {
                            CacheFile.Remove("boxf-" + i.Key);
                        }
                        else
                        {
                            CacheFolder.Remove("boxd-" + i.Key);
                        }
                    }
                });
        }

        internal BoxFolder GetBoxFolder(string boxFolderId)
        {
            var folder = CacheFolder.Get<BoxFolder>("boxd-" + ID + "-" + boxFolderId);
            if (folder == null)
            {
                folder = Storage.GetFolder(boxFolderId);
                if (folder != null)
                    CacheFolder.Insert("boxd-" + ID + "-" + boxFolderId, folder, DateTime.UtcNow.Add(CacheExpiration));
            }
            return folder;
        }

        internal BoxFile GetBoxFile(string boxFileId)
        {
            var file = CacheFile.Get<BoxFile>("boxf-" + ID + "-" + boxFileId);
            if (file == null)
            {
                file = Storage.GetFile(boxFileId);
                if (file != null)
                    CacheFile.Insert("boxf-" + ID + "-" + boxFileId, file, DateTime.UtcNow.Add(CacheExpiration));
            }
            return file;
        }

        internal List<BoxItem> GetBoxItems(string boxFolderId)
        {
            var items = CacheChildItems.Get<List<BoxItem>>("box-" + ID + "-" + boxFolderId);

            if (items == null)
            {
                items = Storage.GetItems(boxFolderId);
                CacheChildItems.Insert("box-" + ID + "-" + boxFolderId, items, DateTime.UtcNow.Add(CacheExpiration));
            }
            return items;
        }

        internal void CacheReset(BoxItem boxItem)
        {
            if (boxItem != null)
            {
                CacheNotify.Publish(new BoxCacheItem { IsFile = boxItem is BoxFile, Key = ID + "-" + boxItem.Id }, CacheNotifyAction.Remove);
            }
        }

        internal void CacheReset(string boxId = null, bool? isFile = null)
        {
            var key = ID + "-";
            if (boxId == null)
            {
                CacheNotify.Publish(new BoxCacheItem { ResetAll = true, Key = key }, CacheNotifyAction.Remove);
            }
            else
            {
                if (boxId == BoxRootId)
                {
                    boxId = "0";
                }
                key += boxId;

                CacheNotify.Publish(new BoxCacheItem { IsFile = isFile, Key = key }, CacheNotifyAction.Remove);
            }
        }

        private class BoxCacheItem
        {
            public bool ResetAll { get; set; }
            public bool? IsFile { get; set; }
            public string Key { get; set; }
        }
    }
}