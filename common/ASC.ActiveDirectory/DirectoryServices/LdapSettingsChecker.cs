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


using System;
using System.Linq;
using System.Text;
using ASC.ActiveDirectory.DirectoryServices.LDAP;
using ASC.Security.Cryptography;
using log4net;

namespace ASC.ActiveDirectory.DirectoryServices
{
    public abstract class LdapSettingsChecker
    {
        public const byte OPERATION_OK = 0;
        public const byte WRONG_SERVER_OR_PORT = 1;
        public const byte WRONG_USER_DN = 2;
        public const byte INCORRECT_LDAP_FILTER = 3;
        public const byte USERS_NOT_FOUND = 4;
        public const byte WRONG_LOGIN_ATTRIBUTE = 5;
        public const byte WRONG_GROUP_DN = 6;
        public const byte INCORRECT_GROUP_LDAP_FILTER = 7;
        public const byte GROUPS_NOT_FOUND = 8;
        public const byte WRONG_GROUP_ATTRIBUTE = 9;
        public const byte WRONG_USER_ATTRIBUTE = 10;
        public const byte WRONG_GROUP_NAME_ATTRIBUTE = 11;
        public const byte CREDENTIALS_NOT_VALID = 12;
        public const byte CONNECT_ERROR = 13;
        public const byte STRONG_AUTH_REQUIRED = 14;
        public const byte WRONG_SID_ATTRIBUTE = 15;
        public const byte CERTIFICATE_REQUEST = 16;
        public const byte TLS_NOT_SUPPORTED = 17;
        public const byte DOMAIN_NOT_FOUND = 18;

        protected ILog log = LogManager.GetLogger(typeof(LdapSettingsChecker));

        public abstract byte CheckSettings(LDAPUserImporter importer, bool acceptCertificate = false);

        public abstract void CheckCredentials(string login, string password, string server, int portNumber, bool startTls);

        protected bool CheckLoginAttribute(LDAPObject user, string loginAttribute)
        {
            string memberUser = null;
            try
            {
                var member = user.InvokeGet(loginAttribute);
                memberUser = member != null ? member.ToString() : null;
                if (string.IsNullOrWhiteSpace(memberUser))
                {
                    log.ErrorFormat("Wrong Login Attribute parameter: {0}", memberUser);
                    return false;
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("Wrong Login Attribute parameter: memberUser = {0}. {1}", memberUser, e);
                return false;
            }
            return true;
        }

        protected bool CheckUserAttribute(LDAPObject user, string userAttr)
        {
            try
            {
                var userAttribute = user.InvokeGet(userAttr);
                if (userAttribute == null || string.IsNullOrWhiteSpace(userAttribute.ToString()))
                {
                    log.ErrorFormat("Wrong Group Attribute parameter: {0}", userAttr);
                    return false;
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("Wrong Group Attribute parameter: {0}. {1}", userAttr, e);
                return false;
            }
            return true;
        }

        protected bool CheckGroupAttribute(LDAPObject group, string groupAttr)
        {
            try
            {
                group.InvokeGet(groupAttr);
            }
            catch (Exception e)
            {
                log.ErrorFormat("Wrong Group Attribute parameter: {0}. {1}", groupAttr, e);
                return false;
            }
            return true;
        }

        protected bool CheckGroupNameAttribute(LDAPObject group, string groupAttr)
        {
            try
            {
                var groupNameAttribute = group.GetValues(groupAttr);
                if (groupNameAttribute == null)
                {
                    log.ErrorFormat("Wrong Group Name Attribute parameter: {0}", groupAttr);
                    return false;
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("Wrong Group Attribute parameter: {0}. {1}", groupAttr, e);
                return false;
            }
            return true;
        }

        protected string GetPassword(byte[] passwordBytes)
        {
            string password;
            try
            {
                password = new UnicodeEncoding().GetString(InstanceCrypto.Decrypt(passwordBytes));
            }
            catch
            {
                password = string.Empty;
            }
            return password;
        }

        public abstract string GetDomain(LDAPSupportSettings settings);

        public bool? IsLDAPDomainExists(string domain, LDAPSupportSettings settings)
        {
            try
            {
                if (string.IsNullOrEmpty(domain))
                    return null;

                var ldapDomain = GetDomain(settings);

                if (string.IsNullOrEmpty(ldapDomain))
                    return null;

                var domainTest = domain.Trim();
                ldapDomain = ldapDomain.Trim();

                return domainTest.Equals(ldapDomain, StringComparison.InvariantCultureIgnoreCase) ||
                       ldapDomain.StartsWith(domainTest);
            }
            catch (Exception e)
            {
                log.ErrorFormat("ExistsLDAPDomain(domain: {0}) error: {1}", domain, e);
            }

            return null;
        }

        public LDAPLogin ParseLogin(string login)
        {
            if (string.IsNullOrEmpty(login))
                return null;

            string username;
            string domain = null;

            if (login.Contains("\\"))
            {
                var splited = login.Split('\\');

                if (!splited.Any() || splited.Length != 2)
                    return null;

                domain = splited[0];
                username = splited[1];

            }
            else if (login.Contains("@"))
            {
                var splited = login.Split('@');

                if (!splited.Any() || splited.Length != 2)
                    return null;

                username = splited[0];
                domain = splited[1];
            }
            else
            {
                username = login;
            }

            var result = new LDAPLogin(username, domain);

            return result;
        }
    }
}
