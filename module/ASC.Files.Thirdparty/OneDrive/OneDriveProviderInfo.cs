/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
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