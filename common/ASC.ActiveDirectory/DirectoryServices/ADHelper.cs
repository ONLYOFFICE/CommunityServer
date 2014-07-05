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

using ASC.Security.Cryptography;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Utility;
using log4net;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Text;
using System.Web.Hosting;

namespace ASC.ActiveDirectory
{
    internal sealed class ADHelper
    {
        private static ILog _log = LogManager.GetLogger(typeof(ADDomain));

        #region вспомогательные методы для работы с DirectoryEntry

        private static DirectoryEntry CreateDirectoryEntry(string rootDistinguishedName)
        {
            try
            {
                return new DirectoryEntry(rootDistinguishedName);
            }
            catch (Exception e)
            {
                _log.ErrorFormat("Can't get access to directory: {0}. {1}", rootDistinguishedName, e);
                return null;
            }
        }

        private static DirectoryEntry CreateDirectoryEntry(string rootDistinguishedName, string login, string password, AuthenticationTypes type)
        {
            try
            {
                return new DirectoryEntry(rootDistinguishedName, login, password, type);
            }
            catch (Exception e)
            {
                _log.ErrorFormat("Can't get access to directory: {0}. {1}", rootDistinguishedName, e);
                return null;
            }
        }

        #endregion

        #region различные поиски

        public static List<DirectoryEntry> Search(string rootDistinguishedName, string filter, SearchScope scope, LDAPSupportSettings settings)
        {
            DirectoryEntry de;
            var type = AuthenticationTypes.ReadonlyServer | AuthenticationTypes.Secure;
            if (settings.PortNumber == Constants.SSL_LDAP_PORT)
            {
                type |= AuthenticationTypes.SecureSocketsLayer;
            }
            string password;
            try
            {
                password = new UnicodeEncoding().GetString(InstanceCrypto.Decrypt(settings.PasswordBytes));
            }
            catch (Exception)
            {
                password = string.Empty;
            }
            de = settings.Authentication ? CreateDirectoryEntry(rootDistinguishedName, settings.Login, password, type) :
                CreateDirectoryEntry(rootDistinguishedName);
            if (de != null)
            {
                return Search(de, filter, scope);
            }
            else
            {
                return null;
            }
        }

        public static List<DirectoryEntry> Search(DirectoryEntry root, string filter, SearchScope scope)
        {
            if (root == null)
            {
                throw new ArgumentNullException("root");
            }
            using (HostingEnvironment.Impersonate())
            {
                DirectorySearcher dsSearcher = null;
                SearchResultCollection result = null;
                List<DirectoryEntry> list = new List<DirectoryEntry>();
                try
                {
                    //create direcotry searcher
                    //

                    dsSearcher = new DirectorySearcher(root);
                    dsSearcher.SearchScope = scope;
                    dsSearcher.ReferralChasing = ReferralChasingOption.All;

                    if (!String.IsNullOrEmpty(filter))
                    {
                        dsSearcher.Filter = filter;
                    }
                    //search
                    //
                    result = dsSearcher.FindAll();
                    //enumerating
                    //
                    foreach (SearchResult entry in result)
                    {
                        list.Add(entry.GetDirectoryEntry());
                    }
                }
                catch (ArgumentException e)
                {
                    _log.InfoFormat("Wrong filter. {0}", e);
                    throw new ArgumentException(e.Message);
                }
                catch (Exception e)
                {
                    _log.ErrorFormat("Internal error {0}", e);
                }
                finally
                {
                    if (result != null)
                    {
                        result.Dispose();
                    }
                    if (dsSearcher != null)
                    {
                        dsSearcher.Dispose();
                    }
                }
                return list;
            }
        }

        #endregion
    }
}
