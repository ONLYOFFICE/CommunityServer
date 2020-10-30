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


using ASC.ActiveDirectory.DirectoryServices;
using System;
using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using System.Runtime.InteropServices;
using System.Linq;

namespace ASC.ActiveDirectory.BuiltIn
{
    public class SystemLdapSettingsChecker : LdapSettingsChecker
    {
        private readonly LdapHelper _ldapHelper = new SystemLdapHelper();

        public override byte CheckSettings(LDAPUserImporter importer,
                                           bool acceptCertificate = false)
        {
            var settings = importer.Settings;

            if (!settings.EnableLdapAuthentication)
                return OPERATION_OK;

            var password = GetPassword(settings.PasswordBytes);

            try
            {
                if (settings.Authentication)
                {
                    CheckCredentials(settings.Login, password, settings.Server, settings.PortNumber, settings.StartTls);
                }
                if (!CheckServerAndPort(settings.Server,
                                        settings.PortNumber, settings.Authentication, settings.Login, password))
                {
                    return WRONG_SERVER_OR_PORT;
                }
            }
            catch (DirectoryServicesCOMException)
            {
                return CREDENTIALS_NOT_VALID;
            }
            catch (COMException)
            {
                return WRONG_SERVER_OR_PORT;
            }

            if (!CheckUserDN(settings.UserDN, settings.Server, settings.PortNumber,
                             settings.Authentication, settings.Login, password, settings.StartTls))
            {
                return WRONG_USER_DN;
            }

            if (settings.GroupMembership)
            {
                if (!CheckGroupDN(settings.UserDN, settings.Server, settings.PortNumber,
                                  settings.Authentication, settings.Login, password, settings.StartTls))
                {
                    return WRONG_USER_DN;
                }

                if (!importer.TryLoadLDAPGroups())
                    return INCORRECT_GROUP_LDAP_FILTER;

                if (!importer.AllDomainGroups.Any())
                    return GROUPS_NOT_FOUND;

                foreach (var group in importer.AllDomainGroups)
                {
                    if (!CheckGroupAttribute(group, settings.GroupAttribute))
                        return WRONG_GROUP_ATTRIBUTE;

                    if (!CheckGroupNameAttribute(group, settings.GroupNameAttribute))
                        return WRONG_GROUP_NAME_ATTRIBUTE;

                    if (group.Sid == null)
                        return WRONG_SID_ATTRIBUTE;
                }
            }

            if (!importer.TryLoadLDAPDomain())
                return DOMAIN_NOT_FOUND;

            if (!importer.TryLoadLDAPUsers())
                return INCORRECT_LDAP_FILTER;

            if (!importer.AllDomainUsers.Any())
                return USERS_NOT_FOUND;

            foreach (var user in importer.AllDomainUsers)
            {
                if (!CheckLoginAttribute(user, settings.LoginAttribute))
                    return WRONG_LOGIN_ATTRIBUTE;

                if (user.Sid == null)
                    return WRONG_SID_ATTRIBUTE;

                if (settings.GroupMembership && !CheckUserAttribute(user, settings.UserAttribute))
                    return WRONG_USER_ATTRIBUTE;
            }

            return OPERATION_OK;
        }

        public override void CheckCredentials(string login, string password, string server, int portNumber, bool startTls)
        {
            try
            {
                _ldapHelper.CheckCredentials(login, password, server, portNumber, startTls);
            }
            catch (LdapException e)
            {
                if (e.ErrorCode != Constants.LDAP_ERROR_INVALID_CREDENTIALS)
                {
                    log.ErrorFormat("Internal LDAP authentication error: {0}.", e);
                    throw new COMException();
                }
                throw new DirectoryServicesCOMException();
            }
            catch (Exception e)
            {
                log.ErrorFormat("Internal AD authentication error: {0}.", e);
                throw new COMException();
            }
        }

        private bool CheckGroupDN(string groupDN, string server, int portNumber,
            bool authentication, string login, string password, bool startTls)
        {
            try
            {
                return _ldapHelper.CheckGroupDN(groupDN, server, portNumber, authentication, login, password, startTls);
            }
            catch (Exception e)
            {
                log.ErrorFormat("Wrong User DN parameter: {0}. {1}", groupDN, e);
                return false;
            }
        }

        private bool CheckUserDN(string userDN, string server, int portNumber,
            bool authentication, string login, string password, bool startTls)
        {
            try
            {
                return _ldapHelper.CheckUserDN(userDN, server, portNumber, authentication, login, password, startTls);
            }
            catch (Exception e)
            {
                log.ErrorFormat("Wrong User DN parameter: {0}. {1}", userDN, e);
                return false;
            }
        }

        private bool CheckServerAndPort(string server, int portNumber,
            bool authentication, string login, string password)
        {
            try
            {
                var type = AuthenticationTypes.ReadonlyServer | AuthenticationTypes.Secure;

                if (portNumber == Constants.SSL_LDAP_PORT)
                {
                    type |= AuthenticationTypes.SecureSocketsLayer;
                }

                var rootEntry = authentication ?
                    new DirectoryEntry(server + ":" + portNumber, login, password, type) :
                    new DirectoryEntry(server + ":" + portNumber);

                if (rootEntry.SchemaClassName != Constants.ObjectClassKnowedValues.DOMAIN_DNS)
                {
                    log.ErrorFormat("Wrong Server Address or Port: {0}:{1}", server, portNumber);
                    return false;
                }
            }
            catch (DirectoryServicesCOMException e)
            {
                log.ErrorFormat("Wrong login or password: {0}:{1}. {2}", login, password, e);
                throw new DirectoryServicesCOMException(e.Message);
            }
            catch (COMException e)
            {
                log.ErrorFormat("Wrong Server Address or Port: {0}:{1}. {2}", server, portNumber, e);
                throw new COMException(e.Message);
            }

            return true;
        }

        public override string GetDomain(LDAPSupportSettings settings)
        {
            var dataInfo = _ldapHelper.GetDomain(settings);

            var domainName = dataInfo != null && dataInfo.DistinguishedName != null
                                 ? dataInfo.DistinguishedName.Remove(0, 3).Replace(",DC=", ".").Replace(",dc=", ".")
                                 : null;

            return domainName;
        }
    }
}
