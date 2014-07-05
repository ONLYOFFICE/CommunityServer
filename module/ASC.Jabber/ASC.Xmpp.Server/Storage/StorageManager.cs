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
using System.Collections.Generic;
using ASC.Collections;
using ASC.Xmpp.Server.Storage.Interface;

namespace ASC.Xmpp.Server.Storage
{
    public class StorageManager : IDisposable
    {
        private IDictionary<string, object> storages = new SynchronizedDictionary<string, object>();

        public IOfflineStore OfflineStorage
        {
            get { return GetStorage<IOfflineStore>("offline"); }
        }

        public IRosterStore RosterStorage
        {
            get { return GetStorage<IRosterStore>("roster"); }
        }

        public IVCardStore VCardStorage
        {
            get { return GetStorage<IVCardStore>("vcard"); }
        }

        public IPrivateStore PrivateStorage
        {
            get { return GetStorage<IPrivateStore>("private"); }
        }

        public IMucStore MucStorage
        {
            get { return GetStorage<IMucStore>("muc"); }
        }

        public IUserStore UserStorage
        {
            get { return GetStorage<IUserStore>("users"); }
        }

        public object this[string storageName]
        {
            get { return storages.ContainsKey(storageName) ? storages[storageName] : null; }
            set { storages[storageName] = value; }
        }

        public T GetStorage<T>(string storageName)
        {
            return (T)this[storageName];
        }

        public void SetStorage(string storageName, object storage)
        {
            this[storageName] = storage;
        }

        public void Dispose()
        {
            foreach (var s in storages.Values)
            {
                var disposable = s as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
        }
    }
}