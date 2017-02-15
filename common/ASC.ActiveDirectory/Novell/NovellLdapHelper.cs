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


using System.Linq;
using ASC.ActiveDirectory.DirectoryServices;
using ASC.ActiveDirectory.Expressions;
using ASC.Security.Cryptography;
using Novell.Directory.Ldap;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Text;

namespace ASC.ActiveDirectory.Novell
{
    public class NovellLdapHelper : LdapHelper
    {
        public bool AcceptCertificate { get; set; }
        
        private const string SearchFilter = "(ObjectClass=*)";

        public override LDAPObject GetDomain(LDAPSupportSettings settings)
        {
            try
            {
                var password = GetPassword(settings.PasswordBytes);

                var novellLdapSearcher = new NovellLdapSearcher(AcceptCertificate);

                var searchResult = novellLdapSearcher.Search(settings.Login, password, settings.Server,
                                                             settings.PortNumber, LdapConnection.SCOPE_BASE,
                                                             settings.StartTls, SearchFilter, settings.UserDN);

                if (searchResult.Count == 0)
                {
                    var domainDn = GetPossibleDomainDn(settings.Server);

                    searchResult = novellLdapSearcher.Search(settings.Login, password, settings.Server,
                                                             settings.PortNumber, LdapConnection.SCOPE_BASE,
                                                             settings.StartTls, SearchFilter, domainDn);

                    if (searchResult.Count == 0)
                        return null;

                }

                return searchResult[0];
            }
            catch (Exception e)
            {
                Log.WarnFormat("Can't get current domain. May be current user has not needed permissions. {0}", e);
                return null;
            }
        }

        public override string GetDefaultDistinguishedName(string server, int portNumber)
        {
            return null;
        }

        public override void CheckCredentials(string login, string password, string server, int portNumber,
                                              bool startTls)
        {
            var novellLdapSearcher = new NovellLdapSearcher(AcceptCertificate);
            novellLdapSearcher.Search(login, password, server, portNumber, LdapConnection.SCOPE_BASE, startTls);
        }

        public override bool CheckUserDN(string userDN, string server,
                                         int portNumber, bool authentication, string login, string password,
                                         bool startTls)
        {
            string[] attributes = {Constants.ADSchemaAttributes.OBJECT_CLASS};

            var novellLdapSearcher = new NovellLdapSearcher(AcceptCertificate);

            var searchResult = novellLdapSearcher.Search(login, password, server,
                                                         portNumber, LdapConnection.SCOPE_BASE, startTls, SearchFilter,
                                                         userDN, attributes);

            if (searchResult.Count != 0)
                return true;

            Log.ErrorFormat("Wrong User DN parameter: {0}.", userDN);
            return false;
        }

        public override bool CheckGroupDN(string groupDN, string server,
                                          int portNumber, bool authentication, string login, string password,
                                          bool startTls)
        {
            string[] attributes = {Constants.ADSchemaAttributes.OBJECT_CLASS};

            var novellLdapSearcher = new NovellLdapSearcher(AcceptCertificate);

            var searchResult = novellLdapSearcher.Search(login, password, server,
                                                         portNumber, LdapConnection.SCOPE_BASE, startTls, SearchFilter,
                                                         groupDN, attributes);

            if (searchResult.Count != 0)
                return true;

            Log.ErrorFormat("Wrong Group DN parameter: {0}.", groupDN);
            return false;
        }

        public override List<LDAPObject> GetUsersByAttributes(LDAPSupportSettings settings)
        {
            var password = GetPassword(settings.PasswordBytes);

            var criteria = Criteria.All(Expression.Exists(settings.LoginAttribute));

            var novellLdapSearcher = new NovellLdapSearcher(AcceptCertificate);

            var searchResult = novellLdapSearcher.Search(settings.Login, password, settings.Server,
                                                         settings.PortNumber, LdapConnection.SCOPE_SUB,
                                                         settings.StartTls, criteria, settings.UserFilter,
                                                         settings.UserDN);

            return searchResult;
        }

        public override List<LDAPObject> GetUsersByAttributesAndFilter(LDAPSupportSettings settings, string filter)
        {
            var password = GetPassword(settings.PasswordBytes);

            if (!string.IsNullOrEmpty(settings.UserFilter) && !settings.UserFilter.StartsWith("(") &&
                !settings.UserFilter.EndsWith(")"))
                settings.UserFilter = "(" + settings.UserFilter + ")";

            filter = "(&" + settings.UserFilter + filter + ")";

            try
            {
                var novellLdapSearcher = new NovellLdapSearcher(AcceptCertificate);
                return novellLdapSearcher.Search(settings.Login, password, settings.Server, settings.PortNumber,
                                                 LdapConnection.SCOPE_SUB, settings.StartTls, null, filter,
                                                 settings.UserDN);
            }
            catch (Exception e)
            {
                Log.ErrorFormat("Can not access to directory: {0}. {1}", settings.UserDN, e);
            }

            return null;
        }

        public override LDAPObject GetUserBySid(LDAPSupportSettings settings, string sid)
        {
            var password = GetPassword(settings.PasswordBytes);
            try
            {
                var ldapUniqueIdAttribute = ConfigurationManager.AppSettings["ldap.unique.id"];

                List<LDAPObject> list;

                var novellLdapSearcher = new NovellLdapSearcher(AcceptCertificate);

                if (ldapUniqueIdAttribute == null)
                {
                    list = novellLdapSearcher.Search(settings.Login, password, settings.Server, settings.PortNumber,
                                                     LdapConnection.SCOPE_SUB, settings.StartTls,
                                                     Criteria.All(Expression.Equal(
                                                         Constants.RfcLDAPAttributes.ENTRY_UUID, sid)),
                                                     settings.UserFilter, settings.UserDN);

                    if (list == null || list.Count == 0)
                    {
                        list = novellLdapSearcher.Search(settings.Login, password, settings.Server, settings.PortNumber,
                                                         LdapConnection.SCOPE_SUB, settings.StartTls,
                                                         Criteria.All(
                                                             Expression.Equal(Constants.RfcLDAPAttributes.NS_UNIQUE_ID,
                                                                              sid)),
                                                         settings.UserFilter, settings.UserDN);

                        if (list == null || list.Count == 0)
                        {
                            list = novellLdapSearcher.Search(settings.Login, password, settings.Server,
                                                             settings.PortNumber,
                                                             LdapConnection.SCOPE_SUB, settings.StartTls,
                                                             Criteria.All(
                                                                 Expression.Equal(Constants.RfcLDAPAttributes.GUID, sid)),
                                                             settings.UserFilter, settings.UserDN);

                            if (list == null || list.Count == 0)
                            {
                                list = novellLdapSearcher.Search(settings.Login, password, settings.Server,
                                                                 settings.PortNumber,
                                                                 LdapConnection.SCOPE_SUB, settings.StartTls,
                                                                 Criteria.All(
                                                                     Expression.Equal(
                                                                         Constants.ADSchemaAttributes.OBJECT_SID, sid)),
                                                                 settings.UserFilter, settings.UserDN);
                            }
                        }
                    }
                }
                else
                {
                    list = novellLdapSearcher.Search(settings.Login, password, settings.Server, settings.PortNumber,
                                                     LdapConnection.SCOPE_SUB, settings.StartTls,
                                                     Criteria.All(Expression.Equal(ldapUniqueIdAttribute, sid)),
                                                     settings.UserFilter, settings.UserDN);
                }

                if (list.Count != 0)
                    return list[0];
            }
            catch (Exception e)
            {
                Log.ErrorFormat("Can not access to directory: {0}. {1}", settings.UserDN, e);
            }

            return null;
        }

        public override List<LDAPObject> GetGroupsByAttributes(LDAPSupportSettings settings)
        {
            try
            {
                var password = GetPassword(settings.PasswordBytes);

                var novellLdapSearcher = new NovellLdapSearcher(AcceptCertificate);

                var groups = novellLdapSearcher.Search(settings.Login, password, settings.Server,
                                                       settings.PortNumber, LdapConnection.SCOPE_SUB, settings.StartTls,
                                                       null, settings.GroupFilter, settings.GroupDN);

                return groups;
            }
            catch (Exception e)
            {
                Log.ErrorFormat("Bad GroupDN or GroupName parameter. {0}", e);
            }

            return null;
        }

        public override List<LDAPObject> GetUsersFromPrimaryGroup(LDAPSupportSettings settings, string primaryGroupId)
        {
            var distinguishedName = settings.Server + ":" + settings.PortNumber + "/" + settings.UserDN;

            var password = GetPassword(settings.PasswordBytes);

            var novellLdapSearcher = new NovellLdapSearcher(AcceptCertificate);

            try
            {
                return novellLdapSearcher.Search(settings.Login, password, settings.Server, settings.PortNumber,
                                                 LdapConnection.SCOPE_SUB, settings.StartTls,
                                                 Criteria.All(
                                                     Expression.Equal(Constants.ADSchemaAttributes.PRIMARY_GROUP_ID,
                                                                      primaryGroupId)),
                                                 settings.UserFilter, distinguishedName);
            }
            catch (Exception e)
            {
                Log.ErrorFormat("Can not access to directory: {0}. {1}", distinguishedName, e);
            }

            return null;
        }

        private static string GetPassword(byte[] passwordBytes)
        {
            string password;
            try
            {
                password = new UnicodeEncoding().GetString(InstanceCrypto.Decrypt(passwordBytes));
            }
            catch (Exception)
            {
                password = string.Empty;
            }
            return password;
        }

        private static string GetPossibleDomainDn(string server)
        {
            IPAddress address;

            var domainDn = string.Empty;

            if (server.StartsWith("LDAP://"))
                server = server.Substring("LDAP://".Length);

            if (IPAddress.TryParse(server, out address))
                return null;

            var domainDnArray = server.Split('.');

            domainDn = domainDnArray.Aggregate(domainDn, (current, t) => current + ("DC=" + t + ","));

            domainDn = domainDn.Remove(domainDn.Length - 1);

            return domainDn;
        }
    }
}
