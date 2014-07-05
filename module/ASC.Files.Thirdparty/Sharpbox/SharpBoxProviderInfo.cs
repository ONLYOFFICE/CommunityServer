/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using ASC.Core;
using ASC.Files.Core;
using AppLimit.CloudComputing.SharpBox;
using AppLimit.CloudComputing.SharpBox.StorageProvider;

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

            _providerKey = (nSupportedCloudConfigurations) Enum.Parse(typeof (nSupportedCloudConfigurations), providerKey, true);
            _authData = authData;
            _rootFolderType = rootFolderType;
            _createOn = createOn;
        }

        private void CreateStorage()
        {
            _storage = new CloudStorage();
            var config = CloudStorage.GetCloudConfigurationEasy(_providerKey);
            if (!string.IsNullOrEmpty(_authData.Token))
            {
                if (_providerKey != nSupportedCloudConfigurations.BoxNet)
                {
                    var token = _storage.DeserializeSecurityTokenFromBase64(_authData.Token);
                    _storage.Open(config, token);
                }
            }
            else
            {
                _storage.Open(config, new GenericNetworkCredentials {Password = _authData.Password, UserName = _authData.Login});
            }
        }

        private CloudStorage _storage;

        internal CloudStorage Storage
        {
            get
            {
                if (_storage == null)
                {
                    CreateStorage();
                }
                else
                {
                    if (!_storage.IsOpened)
                    {
                        //TODO: Check corrupted storage
                        CreateStorage();
                    }
                }
                return _storage;
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
            if (_storage != null)
            {
                _storage.Close();
                _storage = null;
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
    }
}