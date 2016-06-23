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


using ASC.ActiveDirectory.Expressions;
using ASC.Security.Cryptography;
using log4net;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Text;
using System.Web.Hosting;

namespace ASC.ActiveDirectory.BuiltIn
{
    public class SystemLdapSearcher
    {
        private readonly ILog log = LogManager.GetLogger(typeof(SystemLdapSearcher));

        public List<LDAPObject> Search(string root, Criteria criteria, LDAPSupportSettings settings)
        {
            var type = AuthenticationTypes.ReadonlyServer | AuthenticationTypes.Secure;
            string password;
            try
            {
                password = new UnicodeEncoding().GetString(InstanceCrypto.Decrypt(settings.PasswordBytes));
            }
            catch (Exception)
            {
                password = string.Empty;
            }
            if (settings.PortNumber == Constants.SSL_LDAP_PORT)
            {
                type |= AuthenticationTypes.SecureSocketsLayer;
            }
            var entry = settings.Authentication ? new DirectoryEntry(root, settings.Login, password, type) : new DirectoryEntry(root);
            try
            {
                object nativeObject = entry.NativeObject;
            }
            catch (Exception e)
            {
                log.ErrorFormat("Error authenticating user. Current user has not access to read this directory: {0}. {1}", root, e);
                return null;
            }
            return SearchInternal(root, criteria != null ? criteria.ToString() : null, SearchScope.Subtree, settings);
        }

        public List<LDAPObject> Search(string root, Criteria criteria, string userFilter, LDAPSupportSettings settings)
        {
            var type = AuthenticationTypes.ReadonlyServer | AuthenticationTypes.Secure;
            string password;
            try
            {
                password = new UnicodeEncoding().GetString(InstanceCrypto.Decrypt(settings.PasswordBytes));
            }
            catch (Exception)
            {
                password = string.Empty;
            }
            if (settings.PortNumber == Constants.SSL_LDAP_PORT)
            {
                type |= AuthenticationTypes.SecureSocketsLayer;
            }
            var entry = settings.Authentication ? new DirectoryEntry(root, settings.Login, password, type) : new DirectoryEntry(root);
            try
            {
                object nativeObject = entry.NativeObject;
            }
            catch (Exception e)
            {
                log.ErrorFormat("Error authenticating user. Current user has not access to read this directory: {0}. {1}", root, e);
                return new List<LDAPObject>(0);
            }

            if (!string.IsNullOrEmpty(userFilter) && !userFilter.StartsWith("(") && !userFilter.EndsWith(")"))
            {
                userFilter = "(" + userFilter + ")";
            }

            return SearchInternal(root, criteria != null ? "(&" + criteria.ToString() + userFilter + ")" : userFilter, SearchScope.Subtree, settings);
        }

        private List<LDAPObject> SearchInternal(string root, string criteria, SearchScope scope, LDAPSupportSettings settings)
        {
            log.InfoFormat("ADDomain.Search(root: \"{0}\", criteria: \"{1}\", scope: {2})",
                root, criteria, scope);

            List<DirectoryEntry> entries = Search(root, criteria, scope, settings);
            if (entries == null)
            {
                entries = new List<DirectoryEntry>(0);
            }
            return new LDAPObjectFactory().CreateObjects(entries);
        }

        private List<DirectoryEntry> Search(string rootDistinguishedName, string filter, SearchScope scope, LDAPSupportSettings settings)
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

        private List<DirectoryEntry> Search(DirectoryEntry root, string filter, SearchScope scope)
        {
            if (root == null)
            {
                throw new ArgumentNullException("root");
            }
            using (HostingEnvironment.Impersonate())
            {
                DirectorySearcher directorySearcher = null;
                IEnumerable<SearchResult> result = null;
                List<DirectoryEntry> list = new List<DirectoryEntry>();
                try
                {
                    // create directory searcher

                    directorySearcher = new DirectorySearcher(root);
                    // PageSize = 1000 for receiving all (more then default 1000) results
                    directorySearcher.PageSize = 1000;
                    directorySearcher.SearchScope = scope;
                    directorySearcher.ReferralChasing = ReferralChasingOption.All;

                    if (!String.IsNullOrEmpty(filter))
                    {
                        directorySearcher.Filter = filter;
                    }

                    //search
                    result = SafeFindAll(directorySearcher);

                    //enumerating

                    foreach (SearchResult entry in result)
                    {
                        list.Add(entry.GetDirectoryEntry());
                    }
                }
                catch (ArgumentException e)
                {
                    log.InfoFormat("Wrong filter. {0}", e);
                    throw new ArgumentException(e.Message);
                }
                catch (Exception e)
                {
                    log.ErrorFormat("Internal error {0}", e);
                }
                finally
                {
                    if (directorySearcher != null)
                    {
                        directorySearcher.Dispose();
                    }
                }
                return list;
            }
        }

        private IEnumerable<SearchResult> SafeFindAll(DirectorySearcher searcher)
        {
            using (SearchResultCollection results = searcher.FindAll())
            {
                foreach (SearchResult result in results)
                {
                    yield return result;
                }
            } // SearchResultCollection will be disposed here to avoid memory leak 
        }

        private DirectoryEntry CreateDirectoryEntry(string rootDistinguishedName)
        {
            try
            {
                return new DirectoryEntry(rootDistinguishedName);
            }
            catch (Exception e)
            {
                log.ErrorFormat("Can't get access to directory: {0}. {1}", rootDistinguishedName, e);
                return null;
            }
        }

        private DirectoryEntry CreateDirectoryEntry(string rootDistinguishedName, string login, string password, AuthenticationTypes type)
        {
            try
            {
                return new DirectoryEntry(rootDistinguishedName, login, password, type);
            }
            catch (Exception e)
            {
                log.ErrorFormat("Can't get access to directory: {0}. {1}", rootDistinguishedName, e);
                return null;
            }
        }
    }
}
