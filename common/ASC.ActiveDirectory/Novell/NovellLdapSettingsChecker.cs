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


using Novell.Directory.Ldap;
using Novell.Directory.Ldap.Events.Edir;
using System;
using System.Security;

namespace ASC.ActiveDirectory.Novell
{
    public class NovellLdapSettingsChecker : LdapSettingsChecker
    {
        private readonly NovellLdapHelper novellLdapHelper = new NovellLdapHelper();
        public NovellLdapCertificateConfirmRequest CertificateConfirmRequest { get; set; }

        public override byte CheckSettings(LDAPSupportSettings settings, LDAPUserImporter importer, bool acceptCertificate = false)
        {
            // call static constructor of MonitorEventRequest class
            MonitorEventRequest.RegisterResponseTypes = true;
            novellLdapHelper.AcceptCertificate = acceptCertificate;
            if (!settings.EnableLdapAuthentication)
            {
                return OPERATION_OK;
            }
            string password = GetPassword(settings.PasswordBytes);
            if (settings.Server.Equals("LDAP://", StringComparison.InvariantCultureIgnoreCase))
            {
                return WRONG_SERVER_OR_PORT;
            }
            try
            {
                if (settings.Authentication)
                {
                    CheckCredentials(settings.Login, password, settings.Server, settings.PortNumber, settings.StartTls);
                }
            }
            catch (NovellLdapTlsCertificateRequestedException ex)
            {
                CertificateConfirmRequest = ex.CertificateConfirmRequest;
                return CERTIFICATE_REQUEST;
            }
            catch (NotSupportedException)
            {
                return TLS_NOT_SUPPORTED;
            }
            catch (InvalidOperationException)
            {
                return CONNECT_ERROR;
            }
            catch (ArgumentException)
            {
                return WRONG_SERVER_OR_PORT;
            }
            catch (SecurityException)
            {
                return STRONG_AUTH_REQUIRED;
            }
            catch (SystemException)
            {
                return WRONG_SERVER_OR_PORT;
            }
            catch (Exception)
            {
                return CREDENTIALS_NOT_VALID;
            }
            if (!CheckUserDN(settings.UserDN, settings.Server, settings.PortNumber,
                settings.Authentication, settings.Login, password, settings.StartTls))
            {
                return WRONG_USER_DN;
            }
            try
            {
                importer.AllDomainUsers = novellLdapHelper.GetUsersByAttributes(settings);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Incorrect filter. userFilter = {0}, {1}", settings.UserFilter, ex);
                return INCORRECT_LDAP_FILTER;
            }
            if (importer.AllDomainUsers == null || importer.AllDomainUsers.Count == 0)
            {
                log.ErrorFormat("Any user is not found. userDN = {0}", settings.UserDN);
                return USERS_NOT_FOUND;
            }
            foreach (var user in importer.AllDomainUsers)
            {
                if (!CheckLoginAttribute(user, settings.LoginAttribute))
                {
                    return WRONG_LOGIN_ATTRIBUTE;
                }
                if (user.Sid == null)
                {
                    return WRONG_SID_ATTRIBUTE;
                }
            }
            if (settings.GroupMembership)
            {
                if (!CheckGroupDN(settings.GroupDN, settings.Server, settings.PortNumber,
                    settings.Authentication, settings.Login, password, settings.StartTls))
                {
                    return WRONG_GROUP_DN;
                }
                try
                {
                    importer.DomainGroups = novellLdapHelper.GetGroupsByAttributes(settings);
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Incorrect filter. groupFilter = {0}, {1}", settings.GroupFilter, ex);
                    return INCORRECT_GROUP_LDAP_FILTER;
                }
                if (importer.DomainGroups == null || importer.DomainGroups.Count == 0)
                {
                    log.ErrorFormat("Any group is not found. groupDN = {0}", settings.GroupDN);
                    return GROUPS_NOT_FOUND;
                }
                foreach (var group in importer.DomainGroups)
                {
                    if (!CheckGroupAttribute(group, settings.GroupAttribute))
                    {
                        return WRONG_GROUP_ATTRIBUTE;
                    }
                    if (!CheckGroupNameAttribute(group, settings.GroupNameAttribute))
                    {
                        return WRONG_GROUP_NAME_ATTRIBUTE;
                    }
                    if (group.Sid == null)
                    {
                        return WRONG_SID_ATTRIBUTE;
                    }
                }
                foreach (var user in importer.AllDomainUsers)
                {
                    if (!CheckUserAttribute(user, settings.UserAttribute))
                    {
                        return WRONG_USER_ATTRIBUTE;
                    }
                }
            }
            return OPERATION_OK;
        }

        public override void CheckCredentials(string login, string password, string server, int portNumber, bool startTls)
        {
            try
            {
                // call static constructor of MonitorEventRequest class
                MonitorEventRequest.RegisterResponseTypes = true;

                novellLdapHelper.CheckCredentials(login, password, server, portNumber, startTls);
            }
            catch (InterThreadException e)
            {
                if (e.ResultCode == LdapException.CONNECT_ERROR)
                {
                    log.ErrorFormat("LDAP connect error. {0}.", e);
                    throw new InvalidOperationException(e.Message);
                }
            }
            catch (ArgumentException e)
            {
                log.ErrorFormat("Internal LDAP authentication error. Invalid address. {0}", e);
                throw new ArgumentException(e.Message);
            }
            catch (SystemException e)
            {
                log.ErrorFormat("Internal LDAP authentication error. Wrong port. {0}", e);
                throw new SystemException(e.Message);
            }
            catch (LdapException e)
            {
                if (e.ResultCode == LdapException.NO_SUCH_OBJECT)
                {
                    log.ErrorFormat("Internal LDAP authentication error. No such entry. {0}.", e);
                }
                else if (e.ResultCode == LdapException.NO_SUCH_ATTRIBUTE)
                {
                    log.ErrorFormat("Internal LDAP authentication error. No such attribute. {0}.", e);
                }
                else if (e.ResultCode == LdapException.STRONG_AUTH_REQUIRED || e.ResultCode == LdapException.CONFIDENTIALITY_REQUIRED)
                {
                    log.ErrorFormat("Internal LDAP authentication error. Strong auth required. {0}. e.ResultCode = {1}", e, e.ResultCode);
                    throw new SecurityException(e.Message);
                }
                else if (e.ResultCode == LdapException.CONNECT_ERROR)
                {
                    log.ErrorFormat("Internal LDAP authentication error. LDAP connect error. {0}. e.ResultCode = {1}", e, e.ResultCode);
                    throw new InvalidOperationException(e.Message);
                }
                else if (e.ResultCode == LdapException.PROTOCOL_ERROR)
                {
                    log.ErrorFormat("TLS not supported exception. {0}. e.ResultCode = {1}", e, e.ResultCode);
                    throw new NotSupportedException(e.Message);
                }
                else
                {
                    log.ErrorFormat("Internal LDAP authentication error. {0}.", e);
                }
                throw new Exception(e.Message);
            }
        }

        private bool CheckUserDN(string userDN, string server, int portNumber, bool authentication, string login, string password, bool startTls)
        {
            try
            {
                return novellLdapHelper.CheckUserDN(userDN, server, portNumber, authentication, login, password, startTls);
            }
            catch (Exception e)
            {
                log.ErrorFormat("Wrong User DN parameter: {0}. {1}", userDN, e);
                return false;
            }
        }

        private bool CheckGroupDN(string groupDN, string server, int portNumber, bool authentication, string login, string password, bool startTls)
        {
            try
            {
                return novellLdapHelper.CheckGroupDN(groupDN, server, portNumber, authentication, login, password, startTls);
            }
            catch (Exception e)
            {
                log.ErrorFormat("Wrong Group DN parameter: {0}. {1}", groupDN, e);
                return false;
            }
        }
    }
}
