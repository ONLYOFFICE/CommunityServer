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
using System.Web;
using ASC.Web.Files.Classes;
using AppLimit.CloudComputing.SharpBox;
using AppLimit.CloudComputing.SharpBox.Exceptions;
using AppLimit.CloudComputing.SharpBox.StorageProvider;
using ASC.Common.Web;
using ASC.Core;
using ASC.Files.Core;

namespace ASC.Files.Thirdparty.Sharpbox
{
    public class SharpBoxProviderInfo : IProviderInfo, IDisposable
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

        public void Dispose()
        {
            if (StorageOpened)
            {
                Storage.Close();
            }
        }

        private CloudStorage CreateStorage()
        {
            var prms = new object[] { };
            if (!string.IsNullOrEmpty(_authData.Url))
            {
                var uri = _authData.Url;
                if (Uri.IsWellFormedUriString(uri, UriKind.Relative))
                {
                    uri = Uri.UriSchemeHttp + Uri.SchemeDelimiter + uri;
                }
                prms = new object[] { new Uri(uri) };
            }

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

        internal bool StorageOpened
        {
            get
            {
                if (HttpContext.Current != null)
                {
                    var key = "__CLOUD_STORAGE" + ID;
                    var wrapper = (StorageDisposableWrapper)DisposableHttpContext.Current[key];
                    return wrapper != null && wrapper.Storage.IsOpened;
                }
                return false;
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
            catch (SharpBoxException ex)
            {
                Global.Logger.Error("Sharpbox CheckAccess error", ex);
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
                if (Storage.IsOpened)
                    Storage.Close();
            }
        }
    }
}