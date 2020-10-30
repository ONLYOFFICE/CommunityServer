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


using System.Linq;
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
        private readonly ILog _log = LogManager.GetLogger(typeof(SystemLdapSearcher));

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
                type |= AuthenticationTypes.SecureSocketsLayer;

            var entry = settings.Authentication
                            ? new DirectoryEntry(root, settings.Login, password, type)
                            : new DirectoryEntry(root);
            try
            {
                // ReSharper disable UnusedVariable
                var nativeObject = entry.NativeObject;
                // ReSharper restore UnusedVariable
            }
            catch (Exception e)
            {
                _log.ErrorFormat(
                    "Error authenticating user. Current user has not access to read this directory: {0}. {1}", root, e);
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
                type |= AuthenticationTypes.SecureSocketsLayer;

            var entry = settings.Authentication
                            ? new DirectoryEntry(root, settings.Login, password, type)
                            : new DirectoryEntry(root);
            try
            {
                // ReSharper disable UnusedVariable
                var nativeObject = entry.NativeObject;
                // ReSharper restore UnusedVariable
            }
            catch (Exception e)
            {
                _log.ErrorFormat(
                    "Error authenticating user. Current user has not access to read this directory: {0}. {1}", root, e);
                return new List<LDAPObject>(0);
            }

            if (!string.IsNullOrEmpty(userFilter) && !userFilter.StartsWith("(") && !userFilter.EndsWith(")"))
            {
                userFilter = "(" + userFilter + ")";
            }

            return SearchInternal(root, criteria != null ? "(&" + criteria + userFilter + ")" : userFilter,
                                  SearchScope.Subtree, settings);
        }

        private List<LDAPObject> SearchInternal(string root, string criteria, SearchScope scope,
                                                LDAPSupportSettings settings)
        {
            _log.InfoFormat("ADDomain.Search(root: \"{0}\", criteria: \"{1}\", scope: {2})",
                            root, criteria, scope);

            var entries = Search(root, criteria, scope, settings) ?? new List<DirectoryEntry>(0);
            return LDAPObjectFactory.CreateObjects(entries);
        }

        private List<DirectoryEntry> Search(string rootDistinguishedName, string filter, SearchScope scope,
                                            LDAPSupportSettings settings)
        {
            var type = AuthenticationTypes.ReadonlyServer | AuthenticationTypes.Secure;

            if (settings.PortNumber == Constants.SSL_LDAP_PORT)
                type |= AuthenticationTypes.SecureSocketsLayer;

            string password;
            try
            {
                password = new UnicodeEncoding().GetString(InstanceCrypto.Decrypt(settings.PasswordBytes));
            }
            catch (Exception)
            {
                password = string.Empty;
            }

            var de = settings.Authentication
                                    ? CreateDirectoryEntry(rootDistinguishedName, settings.Login, password, type)
                                    : CreateDirectoryEntry(rootDistinguishedName);

            return de != null ? Search(de, filter, scope) : null;
        }

        private List<DirectoryEntry> Search(DirectoryEntry root, string filter, SearchScope scope)
        {
            if (root == null)
                throw new ArgumentNullException("root");

            using (HostingEnvironment.Impersonate())
            {
                DirectorySearcher directorySearcher = null;
                var list = new List<DirectoryEntry>();
                try
                {
                    // create directory searcher
                    directorySearcher = new DirectorySearcher(root)
                        {
                            PageSize = 1000,
                            SearchScope = scope,
                            ReferralChasing = ReferralChasingOption.All
                        };

                    // PageSize = 1000 for receiving all (more then default 1000) results
                    if (!string.IsNullOrEmpty(filter))
                        directorySearcher.Filter = filter;

                    //search
                    var result = SafeFindAll(directorySearcher);

                    //enumerating
                    list.AddRange(result.Select(entry => entry.GetDirectoryEntry()));
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
                    if (directorySearcher != null)
                    {
                        directorySearcher.Dispose();
                    }
                }

                return list;
            }
        }

        private static IEnumerable<SearchResult> SafeFindAll(DirectorySearcher searcher)
        {
            using (var results = searcher.FindAll())
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
                _log.ErrorFormat("Can't get access to directory: {0}. {1}", rootDistinguishedName, e);
                return null;
            }
        }

        private DirectoryEntry CreateDirectoryEntry(string rootDistinguishedName, string login, string password,
                                                    AuthenticationTypes type)
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
    }
}
