/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using System.Diagnostics;
using ASC.Core;
using ASC.FederatedLogin;
using ASC.Files.Core;
using ASC.Common.Caching;
using System.Web;
using ASC.Common.Web;

namespace ASC.Files.Thirdparty.GoogleDrive
{
    [DebuggerDisplay("{CustomerTitle}")]
    public class GoogleDriveProviderInfo : IProviderInfo
    {
        private readonly OAuth20Token _token;
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

        public DateTime CreateOn { get { return _createOn; } }

        public object RootFolderId { get { return "drive-" + ID; } }

        public string ProviderKey { get; private set; }

        public FolderType RootFolderType { get { return _rootFolderType; } }

        public string DriveRootId
        {
            get
            {
                if (string.IsNullOrEmpty(_driveRootId))
                {
                    _driveRootId = Storage.GetRootFolderId();
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
                var key = "__CLOUD_STORAGE" + ID;
                var storage = (StorageDisposableWrapper)DisposableHttpContext.Current[key];
                if (storage != null)
                {
                    storage.Dispose();
                }
            }
        }

        internal void UpdateTitle(string newtitle)
        {
            CustomerTitle = newtitle;
        }

        private GoogleDriveStorage CreateStorage()
        {
            var driveStorage = new GoogleDriveStorage();
            driveStorage.Open(_token);
            return driveStorage;
        }


        class StorageDisposableWrapper : IDisposable
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
    }
}