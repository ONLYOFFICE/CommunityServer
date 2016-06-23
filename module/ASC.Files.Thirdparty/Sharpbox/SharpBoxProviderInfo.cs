/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


using AppLimit.CloudComputing.SharpBox;
using AppLimit.CloudComputing.SharpBox.StorageProvider;
using ASC.Common.Web;
using ASC.Core;
using ASC.Files.Core;
using System;
using System.Web;

namespace ASC.Files.Thirdparty.Sharpbox
{
    public class SharpBoxProviderInfo : IProviderInfo
    {
        public int ID { get; set; }
        public Guid Owner { get; private set; }

        private readonly nSupportedCloudConfigurations _providerKey;
        private readonly AuthData _authData;
        private readonly FolderType _rootFolderType;
        private readonly DateTime _createOn;

        public SharpBoxProviderInfo(int id, string providerKey, string customerTitle, AuthData authData, Guid owner, FolderType rootFolderType, DateTime createOn)
        {
            if (string.IsNullOrEmpty(providerKey))
                throw new ArgumentNullException("providerKey");
            if (string.IsNullOrEmpty(authData.Token) && string.IsNullOrEmpty(authData.Password))
                throw new ArgumentNullException("token", "Both token and password can't be null");
            if (!string.IsNullOrEmpty(authData.Login) && string.IsNullOrEmpty(authData.Password) && string.IsNullOrEmpty(authData.Token))
                throw new ArgumentNullException("password", "Password can't be null");

            ID = id;
            CustomerTitle = customerTitle;
            Owner = owner == Guid.Empty ? SecurityContext.CurrentAccount.ID : owner;

            _providerKey = (nSupportedCloudConfigurations)Enum.Parse(typeof(nSupportedCloudConfigurations), providerKey, true);
            _authData = authData;
            _rootFolderType = rootFolderType;
            _createOn = createOn;
        }

        private CloudStorage CreateStorage()
        {
            var prms = string.IsNullOrEmpty(_authData.Url) ? new object[] { } : new object[] { new Uri(_authData.Url) };
            var storage = new CloudStorage();
            var config = CloudStorage.GetCloudConfigurationEasy(_providerKey, prms);
            if (!string.IsNullOrEmpty(_authData.Token))
            {
                if (_providerKey != nSupportedCloudConfigurations.BoxNet)
                {
                    var token = storage.DeserializeSecurityTokenFromBase64(_authData.Token);
                    storage.Open(config, token);
                }
            }
            else
            {
                storage.Open(config, new GenericNetworkCredentials { Password = _authData.Password, UserName = _authData.Login });
            }
            return storage;
        }

        internal CloudStorage Storage
        {
            get
            {
                if (HttpContext.Current != null)
                {
                    var key = "__CLOUD_STORAGE" + ID;
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

        internal void UpdateTitle(string newtitle)
        {
            CustomerTitle = newtitle;
        }

        public string CustomerTitle { get; private set; }

        public DateTime CreateOn
        {
            get { return _createOn; }
        }

        public object RootFolderId
        {
            get { return "sbox-" + ID; }
        }

        public bool CheckAccess()
        {
            try
            {
                return Storage.GetRoot() != null;
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

        public string ProviderKey
        {
            get { return _providerKey.ToString(); }
        }

        public FolderType RootFolderType
        {
            get { return _rootFolderType; }
        }


        class StorageDisposableWrapper : IDisposable
        {
            public CloudStorage Storage { get; private set; }


            public StorageDisposableWrapper(CloudStorage storage)
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