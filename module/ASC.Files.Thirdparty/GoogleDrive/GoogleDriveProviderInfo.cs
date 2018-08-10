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
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using ASC.Common.Caching;
using ASC.Common.Web;
using ASC.Core;
using ASC.FederatedLogin;
using ASC.FederatedLogin.Helpers;
using ASC.FederatedLogin.LoginProviders;
using ASC.Files.Core;
using ASC.Web.Files.Classes;
using DriveFile = Google.Apis.Drive.v3.Data.File;

namespace ASC.Files.Thirdparty.GoogleDrive
{
    [DebuggerDisplay("{CustomerTitle}")]
    public class GoogleDriveProviderInfo : IProviderInfo, IDisposable
    {
        private OAuth20Token _token;
        private readonly FolderType _rootFolderType;
        private readonly DateTime _createOn;
        private string _driveRootId;

        internal GoogleDriveStorage Storage
        {
            get
            {
                if (HttpContext.Current != null)
                {
                    var key = "__GOOGLE_STORAGE" + ID;
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

        public int ID { get; set; }

        public Guid Owner { get; private set; }

        public string CustomerTitle { get; private set; }

        public DateTime CreateOn
        {
            get { return _createOn; }
        }

        public object RootFolderId
        {
            get { return "drive-" + ID; }
        }

        public string ProviderKey { get; private set; }

        public FolderType RootFolderType
        {
            get { return _rootFolderType; }
        }

        public string DriveRootId
        {
            get
            {
                if (string.IsNullOrEmpty(_driveRootId))
                {
                    try
                    {
                        _driveRootId = Storage.GetRootFolderId();
                    }
                    catch (Exception ex)
                    {
                        Global.Logger.Error("GoogleDrive error", ex);
                        return null;
                    }
                }
                return _driveRootId;
            }
        }


        public GoogleDriveProviderInfo(int id, string providerKey, string customerTitle, string token, Guid owner, FolderType rootFolderType, DateTime createOn)
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
            Storage.Close();
        }

        public bool CheckAccess()
        {
            try
            {
                return !string.IsNullOrEmpty(DriveRootId);
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
                var key = "__GOOGLE_STORAGE" + ID;
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

        private GoogleDriveStorage CreateStorage()
        {
            var driveStorage = new GoogleDriveStorage();

            CheckToken();

            driveStorage.Open(_token);
            return driveStorage;
        }

        private void CheckToken()
        {
            if (_token == null) throw new UnauthorizedAccessException("Cannot create GoogleDrive session with given token");
            if (_token.IsExpired)
            {
                _token = OAuth20TokenHelper.RefreshToken(GoogleLoginProvider.GoogleOauthTokenUrl, _token);

                using (var dbDao = new CachedProviderAccountDao(CoreContext.TenantManager.GetCurrentTenant().TenantId, FileConstant.DatabaseId))
                {
                    var authData = new AuthData(token: _token.ToJson());
                    dbDao.UpdateProviderInfo(ID, authData);
                }
            }
        }


        private class StorageDisposableWrapper : IDisposable
        {
            public GoogleDriveStorage Storage { get; private set; }


            public StorageDisposableWrapper(GoogleDriveStorage storage)
            {
                Storage = storage;
            }

            public void Dispose()
            {
                Storage.Close();
            }
        }


        private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(1);
        private static readonly ICache CacheEntry = AscCache.Memory;
        private static readonly ICache CacheChildFiles = AscCache.Memory;
        private static readonly ICache CacheChildFolders = AscCache.Memory;
        private static readonly ICacheNotify CacheNotify;

        static GoogleDriveProviderInfo()
        {
            CacheNotify = AscCache.Notify;
            CacheNotify.Subscribe<GoogleDriveCacheItem>((i, action) =>
                {
                    if (action != CacheNotifyAction.Remove) return;
                    if (i.ResetEntry)
                    {
                        CacheEntry.Remove("drive-" + i.Key);
                    }
                    if (i.ResetAll)
                    {
                        CacheEntry.Remove(new Regex("^drive-" + i.Key + ".*"));
                        CacheChildFiles.Remove(new Regex("^drivef-" + i.Key + ".*"));
                        CacheChildFolders.Remove(new Regex("^drived-" + i.Key + ".*"));
                    }
                    if (i.ResetChilds)
                    {
                        if (!i.ChildFolder.HasValue || !i.ChildFolder.Value)
                        {
                            CacheChildFiles.Remove("drivef-" + i.Key);
                        }
                        if (!i.ChildFolder.HasValue || i.ChildFolder.Value)
                        {
                            CacheChildFolders.Remove("drived-" + i.Key);
                        }
                    }
                });
        }

        internal DriveFile GetDriveEntry(string driveId)
        {
            var entry = CacheEntry.Get<DriveFile>("drive-" + ID + driveId);
            if (entry == null)
            {
                entry = Storage.GetEntry(driveId);
                CacheEntry.Insert("drive-" + ID + driveId, entry, DateTime.UtcNow.Add(CacheExpiration));
            }
            return entry;
        }

        internal List<DriveFile> GetDriveEntries(string parentDriveId, bool? folder = null)
        {
            if (folder.HasValue)
            {
                if (folder.Value)
                {
                    var value = CacheChildFolders.Get<List<DriveFile>>("drived-" + ID + "-" + parentDriveId);
                    if (value == null)
                    {
                        value = Storage.GetEntries(parentDriveId, true);
                        CacheChildFolders.Insert("drived-" + ID + "-" + parentDriveId, value, DateTime.UtcNow.Add(CacheExpiration));
                    }
                    return value;
                }
                else
                {
                    var value = CacheChildFiles.Get<List<DriveFile>>("drivef-" + ID + "-" + parentDriveId);
                    if (value == null)
                    {
                        value = Storage.GetEntries(parentDriveId, false);
                        CacheChildFiles.Insert("drivef-" + ID + "-" + parentDriveId, value, DateTime.UtcNow.Add(CacheExpiration));
                    }
                    return value;
                }
            }

            if (CacheChildFiles.Get<List<DriveFile>>("drivef-" + ID + "-" + parentDriveId) == null &&
                CacheChildFolders.Get<List<DriveFile>>("drived-" + ID + "-" + parentDriveId) == null)
            {
                var entries = Storage.GetEntries(parentDriveId);

                CacheChildFiles.Insert("drivef-" + ID + "-" + parentDriveId, entries.Where(entry => entry.MimeType != GoogleLoginProvider.GoogleDriveMimeTypeFolder).ToList(), DateTime.UtcNow.Add(CacheExpiration));
                CacheChildFolders.Insert("drived-" + ID + "-" + parentDriveId, entries.Where(entry => entry.MimeType == GoogleLoginProvider.GoogleDriveMimeTypeFolder).ToList(), DateTime.UtcNow.Add(CacheExpiration));

                return entries;
            }

            var folders = CacheChildFolders.Get<List<DriveFile>>("drived-" + ID + "-" + parentDriveId);
            if (folders == null)
            {
                folders = Storage.GetEntries(parentDriveId, true);
                CacheChildFolders.Insert("drived-" + ID + "-" + parentDriveId, folders, DateTime.UtcNow.Add(CacheExpiration));
            }
            var files = CacheChildFiles.Get<List<DriveFile>>("drivef-" + ID + "-" + parentDriveId);
            if (files == null)
            {
                files = Storage.GetEntries(parentDriveId, false);
                CacheChildFiles.Insert("drivef-" + ID + "-" + parentDriveId, files, DateTime.UtcNow.Add(CacheExpiration));
            }
            return folders.Concat(files).ToList();
        }

        internal void CacheReset(DriveFile driveEntry)
        {
            if (driveEntry != null)
            {
                CacheNotify.Publish(new GoogleDriveCacheItem { ResetEntry = true, Key = ID + "-" + driveEntry.Id }, CacheNotifyAction.Remove);
            }
        }

        internal void CacheReset(string driveId = null, bool? childFolder = null)
        {
            var key = ID + "-";
            if (driveId == null)
            {
                CacheNotify.Publish(new GoogleDriveCacheItem { ResetAll = true, Key = key }, CacheNotifyAction.Remove);
            }
            else
            {
                if (driveId == DriveRootId)
                {
                    driveId = "root";
                }
                key += driveId;

                CacheNotify.Publish(new GoogleDriveCacheItem { ResetEntry = true, ResetChilds = true, Key = key, ChildFolder = childFolder }, CacheNotifyAction.Remove);
            }
        }

        internal void CacheResetChilds(string parentDriveId, bool? childFolder = null)
        {
            CacheNotify.Publish(new GoogleDriveCacheItem { ResetChilds = true, Key = ID + "-" + parentDriveId, ChildFolder = childFolder }, CacheNotifyAction.Remove);
        }

        private class GoogleDriveCacheItem
        {
            public bool ResetAll { get; set; }
            public bool ResetEntry { get; set; }
            public bool ResetChilds { get; set; }
            public string Key { get; set; }
            public bool? ChildFolder { get; set; }
        }
    }
}