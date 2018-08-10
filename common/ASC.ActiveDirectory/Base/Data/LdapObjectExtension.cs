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


using System;
using System.Collections.Generic;
using System.Linq;
using ASC.ActiveDirectory.Base.Settings;
using ASC.Core.Tenants;
using ASC.Core.Users;
using log4net;

namespace ASC.ActiveDirectory.Base.Data
{
    /// <summary>
    /// LDAP object extensions class
    /// </summary>
    public static class LdapObjectExtension
    {
        public static string GetAttribute(this LdapObject ldapObject, string attribute, ILog log = null)
        {
            if (string.IsNullOrEmpty(attribute))
                return string.Empty;

            try
            {
                return ldapObject.GetValue(attribute) as string;
            }
            catch (Exception e)
            {
                if (log != null)
                    log.ErrorFormat("Can't get attribute from ldap object (attr = {0}, dn = {1}) error: {2}",
                        attribute, ldapObject.DistinguishedName, e);

                return string.Empty;
            }
        }

        public static List<string> GetAttributes(this LdapObject ldapObject, string attribute, ILog log = null)
        {
            var list = new List<string>();

            if (string.IsNullOrEmpty(attribute))
                return list;

            try
            {
                return ldapObject.GetValues(attribute);
            }
            catch (Exception e)
            {
                if (log != null)
                    log.ErrorFormat("Can't get attributes from ldap object (attr = {0}, dn = {1}) error: {2}",
                        attribute, ldapObject.DistinguishedName, e);

                return list;
            }
        }

        private const int MAX_NUMBER_OF_SYMBOLS = 64;
        private const string EXT_MOB_PHONE = "extmobphone";
        private const string EXT_MAIL = "extmail";

        public static UserInfo ToUserInfo(this LdapObject ldapUser, LdapUserImporter ldapUserImporter, ILog log = null)
        {
            var settings = ldapUserImporter.Settings;
            var resource = ldapUserImporter.Resource;

            var userName = ldapUser.GetAttribute(settings.LoginAttribute, log);
            var firstName = ldapUser.GetAttribute(settings.FirstNameAttribute, log);
            var secondName = ldapUser.GetAttribute(settings.SecondNameAttribute, log);
            var mail = ldapUser.GetAttribute(settings.MailAttribute, log);
            var emails = ldapUser.GetAttributes(settings.MailAttribute, log);
            var mobilePhone = ldapUser.GetAttribute(settings.MobilePhoneAttribute, log);
            var title = ldapUser.GetAttribute(settings.TitleAttribute, log);
            var location = ldapUser.GetAttribute(settings.LocationAttribute, log);

            if (string.IsNullOrEmpty(userName))
                throw new Exception("LDAP LoginAttribute is empty");

            var contacts = new List<string>();

            if (!string.IsNullOrEmpty(mobilePhone))
            {
                contacts.Add(EXT_MOB_PHONE);
                contacts.Add(mobilePhone);
            }

            if (emails.Any())
            {
                foreach (var email in emails)
                {
                    if (email.Equals(mail))
                        continue;

                    contacts.Add(EXT_MAIL);
                    contacts.Add(email);
                }
            }

            var user = new UserInfo
            {
                ID = Guid.Empty,
                UserName = userName,
                Sid = ldapUser.Sid,
                ActivationStatus = EmployeeActivationStatus.NotActivated,
                Status = ldapUser.IsDisabled ? EmployeeStatus.Terminated : EmployeeStatus.Active,
                Title = !string.IsNullOrEmpty(title) ? title : string.Empty,
                Location = !string.IsNullOrEmpty(location) ? location : string.Empty,
                WorkFromDate = TenantUtil.DateTimeNow(),
                Contacts = contacts
            };

            if (!string.IsNullOrEmpty(firstName))
            {
                user.FirstName = firstName.Length > MAX_NUMBER_OF_SYMBOLS
                    ? firstName.Substring(0, MAX_NUMBER_OF_SYMBOLS)
                    : firstName;
            }
            else
            {
                user.FirstName = resource.FirstName;
            }

            if (!string.IsNullOrEmpty(secondName))
            {
                user.LastName = secondName.Length > MAX_NUMBER_OF_SYMBOLS
                    ? secondName.Substring(0, MAX_NUMBER_OF_SYMBOLS)
                    : secondName;
            }
            else
            {
                user.LastName = resource.LastName;
            }

            user.Email = string.IsNullOrEmpty(mail)
                ? (userName.Contains("@")
                    ? userName
                    : string.Format("{0}@{1}", userName, ldapUserImporter.LDAPDomain))
                : mail;

            return user;
        }

        public static GroupInfo ToGroupInfo(this LdapObject ldapGroup, LdapSettings settings, ILog log = null)
        {
            var name = ldapGroup.GetAttribute(settings.GroupNameAttribute, log);

            if (string.IsNullOrEmpty(name))
                throw new Exception("LDAP GroupNameAttribute is empty");

            var group = new GroupInfo
            {
                Name = name,
                Sid = ldapGroup.Sid
            };

            return group;
        }

        public static string GetDomainFromDn(this LdapObject ldapObject)
        {
            if (ldapObject == null || string.IsNullOrEmpty(ldapObject.DistinguishedName))
                return null;

            return LdapUtils.DistinguishedNameToDomain(ldapObject.DistinguishedName);
        }
    }
}
