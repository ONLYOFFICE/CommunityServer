/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
