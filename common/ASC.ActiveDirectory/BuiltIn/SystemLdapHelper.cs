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


using ASC.ActiveDirectory.DirectoryServices;
using ASC.ActiveDirectory.Expressions;
using ASC.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Hosting;

namespace ASC.ActiveDirectory.BuiltIn
{
    public class SystemLdapHelper : LdapHelper
    {
        private readonly SystemLdapSearcher _systemLdapSearcher = new SystemLdapSearcher();

        #region common methods

        public override LDAPObject GetDomain(LDAPSupportSettings settings)
        {
            try
            {
                string password;
                var type = AuthenticationTypes.ReadonlyServer | AuthenticationTypes.Secure;
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
                                ? new DirectoryEntry(settings.Server + ":" + settings.PortNumber, settings.Login, password,
                                                     type)
                                : new DirectoryEntry(settings.Server + ":" + settings.PortNumber);

                return LDAPObjectFactory.CreateObject(entry);
            }
            catch (Exception e)
            {
                Log.WarnFormat("Can't get current domain. May be current user has not needed permissions. {0}", e);
                return null;
            }
        }

        public override string GetDefaultDistinguishedName(string server, int portNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(server) || portNumber < 0)
                    return null;

                using (HostingEnvironment.Impersonate())
                {
                    return
                        new DirectoryEntry(server + ":" + portNumber).InvokeGet(
                            Constants.ADSchemaAttributes.DISTINGUISHED_NAME).ToString();
                }
            }
            catch
            {
                return null;
            }
        }

        public override void CheckCredentials(string login, string password, string server, int portNumber,
                                              bool startTls)
        {
            var domain = server.Split('/').Last();
            var serverConnection = domain + ":" + portNumber;

            // if login with domain
            string loginDomain = null;
            var loginLocalPart = login;

            if (login.Contains("\\"))
            {
                var splited = login.Split('\\');
                loginLocalPart = splited[1];
                loginDomain = splited[0];
            }
            else if (login.Contains("@"))
            {
                var splited = login.Split('@');
                loginLocalPart = splited[0];
                loginDomain = splited[1];
            }

            if (!string.IsNullOrEmpty(loginDomain))
            {
                if (!domain.StartsWith(loginDomain, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new LdapException(string.Format("Domain '{0}' from login '{1}' not found", loginDomain, login));
                }
            }

            using (var ldap = new LdapConnection(serverConnection))
            {
                var networkCredential = new NetworkCredential(loginLocalPart, password, domain);
                ldap.SessionOptions.VerifyServerCertificate = (con, cer) => true;
                ldap.SessionOptions.SecureSocketLayer = (portNumber == Constants.SSL_LDAP_PORT);
                ldap.SessionOptions.ProtocolVersion = Constants.LDAP_V3;
                ldap.AuthType = AuthType.Negotiate;
                ldap.Bind(networkCredential);
            }
        }

        #endregion

        #region users

        public override List<LDAPObject> GetUsersByAttributes(LDAPSupportSettings settings)
        {
            var distinguishedName = settings.Server + ":" + settings.PortNumber + "/" + settings.UserDN;
            
            try
            {
                return _systemLdapSearcher.Search(distinguishedName,
                                                  Criteria.All(Expression.Exists(settings.LoginAttribute)),
                                                  settings.UserFilter, settings);
            }
            catch (ArgumentException)
            {
                throw new ArgumentException();
            }
            catch (Exception e)
            {
                Log.ErrorFormat("Can not access to directory: {0}. {1}", distinguishedName, e);
            }
            
            return null;
        }

        public override List<LDAPObject> GetUsersByAttributesAndFilter(LDAPSupportSettings settings, string filter)
        {
            var distinguishedName = settings.Server + ":" + settings.PortNumber + "/" + settings.UserDN;
            
            if (!string.IsNullOrEmpty(settings.UserFilter) && !settings.UserFilter.StartsWith("(") &&
                !settings.UserFilter.EndsWith(")"))
            {
                settings.UserFilter = "(" + settings.UserFilter + ")";
            }
            
            filter = "(&" + settings.UserFilter + filter + ")";
            
            try
            {
                return _systemLdapSearcher.Search(distinguishedName, null, filter, settings);
            }
            catch (Exception e)
            {
                Log.ErrorFormat("Can not access to directory: {0}. {1}", distinguishedName, e);
            }
            
            return null;
        }

        public override List<LDAPObject> GetUsersFromPrimaryGroup(LDAPSupportSettings settings, string primaryGroupId)
        {
            var distinguishedName = settings.Server + ":" + settings.PortNumber + "/" + settings.UserDN;
            
            try
            {
               return _systemLdapSearcher.Search(distinguishedName, Criteria.All(Expression.Equal(
                    Constants.ADSchemaAttributes.PRIMARY_GROUP_ID, primaryGroupId)), settings.UserFilter, settings);
            }
            catch (Exception e)
            {
                Log.ErrorFormat("Can not access to directory: {0}. {1}", distinguishedName, e);
            }
            
            return null;
        }

        public override LDAPObject GetUserBySid(LDAPSupportSettings settings, string sid)
        {
            var distinguishedName = settings.Server + ":" + settings.PortNumber + "/" + settings.UserDN;
            
            try
            {
                var list = _systemLdapSearcher.Search(distinguishedName, Criteria.All(
                    Expression.Equal(Constants.ADSchemaAttributes.OBJECT_SID, sid)), settings.UserFilter, settings);

                if (list.Count != 0)
                    return list[0];
            }
            catch (Exception e)
            {
                Log.ErrorFormat("Can not access to directory: {0}. {1}", distinguishedName, e);
            }

            return null;
        }

        public override bool CheckUserDN(string userDN, string server,
            int portNumber, bool authentication, string login, string password, bool startTls)
        {
            var type = AuthenticationTypes.ReadonlyServer | AuthenticationTypes.Secure;

            if (portNumber == Constants.SSL_LDAP_PORT)
                type |= AuthenticationTypes.SecureSocketsLayer;

            var userEntry = authentication ?
                new DirectoryEntry(server + ":" + portNumber + "/" + userDN, login, password, type) :
                new DirectoryEntry(server + ":" + portNumber + "/" + userDN);
            
            if (userEntry.SchemaClassName == string.Empty)
            {
                Log.ErrorFormat("Wrong User DN parameter: {0}", userDN);
                return false;
            }
            
            return true;
        }

        #endregion

        #region groups

        public override bool CheckGroupDN(string groupDN, string server,
            int portNumber, bool authentication, string login, string password, bool startTls)
        {
            var type = AuthenticationTypes.ReadonlyServer | AuthenticationTypes.Secure;
            
            if (portNumber == Constants.SSL_LDAP_PORT)
                type |= AuthenticationTypes.SecureSocketsLayer;
 
            var userEntry = authentication ?
                new DirectoryEntry(server + ":" + portNumber + "/" + groupDN, login, password, type) :
                new DirectoryEntry(server + ":" + portNumber + "/" + groupDN);

            if (userEntry.SchemaClassName == string.Empty)
            {
                Log.ErrorFormat("Wrong GroupDN DN parameter: {0}", groupDN);
                return false;
            }

            return true;
        }

        public override List<LDAPObject> GetGroupsByAttributes(LDAPSupportSettings settings)
        {
            try
            {
                var groups = _systemLdapSearcher.Search(
                    settings.Server + ":" + settings.PortNumber + "/" + settings.GroupDN, null, settings.GroupFilter, settings);
                return groups;
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Wrong Group DN or Group Filter parameter: GroupDN = {0}, GroupFilter = {1}, {2}",
                    settings.GroupDN, settings.GroupFilter, ex);
            }

            return null;
        }
        
        #endregion
    }
}
