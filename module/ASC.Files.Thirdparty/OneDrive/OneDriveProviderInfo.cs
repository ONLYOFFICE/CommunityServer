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
using Microsoft.OneDrive.Sdk;

namespace ASC.Files.Thirdparty.OneDrive
{
    [DebuggerDisplay("{CustomerTitle}")]
    public class OneDriveProviderInfo : IProviderInfo, IDisposable
    {
        private OAuth20Token _token;
        private readonly FolderType _rootFolderType;
        private readonly DateTime _createOn;

        internal OneDriveStorage Storage
        {
            get
            {
                if (HttpContext.Current != null)
                {
                    var key = "__ONEDRIVE_STORAGE" + ID;
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
                    var key = "__ONEDRIVE_STORAGE" + ID;
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
            get { return "onedrive-" + ID; }
        }

        public string ProviderKey { get; private set; }

        public FolderType RootFolderType
        {
            get { return _rootFolderType; }
        }


        public OneDriveProviderInfo(int id, string providerKey, string customerTitle, string token, Guid owner, FolderType rootFolderType, DateTime createOn)
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
                return Storage.CheckAccess();
            }
            catch (AggregateException)
            {
                return false;
            }
        }

        public void InvalidateStorage()
        {
            if (HttpContext.Current != null)
            {
                var key = "__ONEDRIVE_STORAGE" + ID;
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

        private OneDriveStorage CreateStorage()
        {
            var onedriveStorage = new OneDriveStorage();

            CheckToken();

            onedriveStorage.Open(_token);
            return onedriveStorage;
        }

        private void CheckToken()
        {
            if (_token == null) throw new UnauthorizedAccessException("Cannot create GoogleDrive session with given token");
            if (_token.IsExpired)
            {
                _token = OAuth20TokenHelper.RefreshToken<OneDriveLoginProvider>(_token);

                using (var dbDao = new CachedProviderAccountDao(CoreContext.TenantManager.GetCurrentTenant().TenantId, FileConstant.DatabaseId))
                {
                    var authData = new AuthData(token: _token.ToJson());
                    dbDao.UpdateProviderInfo(ID, authData);
                }
            }
        }


        private class StorageDisposableWrapper : IDisposable
        {
            public OneDriveStorage Storage { get; private set; }


            public StorageDisposableWrapper(OneDriveStorage storage)
            {
                Storage = storage;
            }

            public void Dispose()
            {
                Storage.Close();
            }
        }


        private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(1);
        private static readonly ICache CacheItem = AscCache.Memory;
        private static readonly ICache CacheChildItems = AscCache.Memory;
        private static readonly ICacheNotify CacheNotify;

        static OneDriveProviderInfo()
        {
            CacheNotify = AscCache.Notify;
            CacheNotify.Subscribe<OneDriveCacheItem>((i, action) =>
                {
                    if (action != CacheNotifyAction.Remove) return;
                    if (i.ResetAll)
                    {
                        CacheChildItems.Remove(new Regex("^onedrivei-" + i.Key + ".*"));
                        CacheItem.Remove(new Regex("^onedrive-" + i.Key + ".*"));
                    }
                    else
                    {
                        CacheChildItems.Remove(new Regex("onedrivei-" + i.Key));
                        CacheItem.Remove("onedrive-" + i.Key);
                    }
                });
        }

        internal Item GetOneDriveItem(string itemId)
        {
            var file = CacheItem.Get<Item>("onedrive-" + ID + "-" + itemId);
            if (file == null)
            {
                file = Storage.GetItem(itemId);
                if (file != null)
                    CacheItem.Insert("onedrive-" + ID + "-" + itemId, file, DateTime.UtcNow.Add(CacheExpiration));
            }
            return file;
        }

        internal List<Item> GetOneDriveItems(string onedriveFolderId)
        {
            var items = CacheChildItems.Get<List<Item>>("onedrivei-" + ID + "-" + onedriveFolderId);

            if (items == null)
            {
                items = Storage.GetItems(onedriveFolderId);
                CacheChildItems.Insert("onedrivei-" + ID + "-" + onedriveFolderId, items, DateTime.UtcNow.Add(CacheExpiration));
            }
            return items;
        }

        internal void CacheReset(string onedriveId = null)
        {
            var key = ID + "-";
            if (string.IsNullOrEmpty(onedriveId))
            {
                CacheNotify.Publish(new OneDriveCacheItem { ResetAll = true, Key = key }, CacheNotifyAction.Remove);
            }
            else
            {
                key += onedriveId;

                CacheNotify.Publish(new OneDriveCacheItem { Key = key }, CacheNotifyAction.Remove);
            }
        }

        private class OneDriveCacheItem
        {
            public bool ResetAll { get; set; }
            public string Key { get; set; }
        }
    }
}