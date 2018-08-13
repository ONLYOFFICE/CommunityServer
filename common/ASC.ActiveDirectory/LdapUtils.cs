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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ASC.ActiveDirectory.Base.Data;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core.Utility;
using log4net;
using Monocert = Mono.Security.X509;
using Syscert = System.Security.Cryptography.X509Certificates;

namespace ASC.ActiveDirectory
{
    public static class LdapUtils
    {
        private static readonly Regex DcRegex = new Regex("dc=([^,]+)", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        public static string DistinguishedNameToDomain(string distinguishedName)
        {
            if (string.IsNullOrEmpty(distinguishedName))
                return null;

            var matchList = DcRegex.Matches(distinguishedName);

            var dcList = matchList.Cast<Match>().Select(match => match.Groups[1].Value).ToList();

            return !dcList.Any() ? null : string.Join(".", dcList);
        }

        public static bool IsLoginAccepted(LdapLogin ldapLogin, UserInfo ldapUser, string ldapDomain)
        {
            if (ldapLogin == null
                || string.IsNullOrEmpty(ldapLogin.ToString())
                || string.IsNullOrEmpty(ldapDomain)
                || ldapUser == null
                || ldapUser.Equals(Constants.LostUser)
                || string.IsNullOrEmpty(ldapUser.Email)
                || string.IsNullOrEmpty(ldapUser.UserName))
            {
                return false;
            }

            var hasDomain = !string.IsNullOrEmpty(ldapLogin.Domain);

            if (!hasDomain)
            {
                return ldapLogin.Username.Equals(ldapUser.UserName, StringComparison.InvariantCultureIgnoreCase);
            }

            var fullLogin = ldapLogin.ToString();

            if (fullLogin.Equals(ldapUser.Email, StringComparison.InvariantCultureIgnoreCase))
                return true;

            if (!ldapDomain.StartsWith(ldapLogin.Domain))
                return false;

            var alterEmail = ldapUser.UserName.Contains("@")
                ? ldapUser.UserName
                : string.Format("{0}@{1}", ldapUser.UserName, ldapDomain);

            return IsLoginAndEmailSuitable(fullLogin, alterEmail);
        }

        private static string GetLdapAccessableEmail(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                    return null;

                var login = LdapLogin.ParseLogin(email);

                if (string.IsNullOrEmpty(login.Domain))
                    return email;

                var dotIndex = login.Domain.LastIndexOf(".", StringComparison.Ordinal);

                var accessableEmail = dotIndex > -1 ? string.Format("{0}@{1}", login.Username, login.Domain.Remove(dotIndex)) : email;

                return accessableEmail;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static bool IsLoginAndEmailSuitable(string login, string email)
        {
            try
            {
                if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(email))
                    return false;

                var accessableLogin = GetLdapAccessableEmail(login);

                if (string.IsNullOrEmpty(accessableLogin))
                    return false;

                var accessableEmail = GetLdapAccessableEmail(email);

                if (string.IsNullOrEmpty(accessableEmail))
                    return false;

                return accessableLogin.Equals(accessableEmail, StringComparison.InvariantCultureIgnoreCase);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private const string NOISE = "1234567890mnbasdflkjqwerpoiqweyuvcxnzhdkqpsdk@%&;";

        public static string GeneratePassword(PasswordSettings ps)
        {
            var maxLength = PasswordSettings.MaxLength
                            - (ps.Digits ? 1 : 0)
                            - (ps.UpperCase ? 1 : 0)
                            - (ps.SpecSymbols ? 1 : 0);
            var minLength = Math.Min(ps.MinLength, maxLength);

            return string.Format("{0}{1}{2}{3}",
                                 GeneratePassword(minLength, minLength, NOISE.Substring(0, NOISE.Length - 4)),
                                 ps.Digits ? GeneratePassword(1, 1, NOISE.Substring(0, 10)) : string.Empty,
                                 ps.UpperCase ? GeneratePassword(1, 1, NOISE.Substring(10, 20).ToUpper()) : string.Empty,
                                 ps.SpecSymbols ? GeneratePassword(1, 1, NOISE.Substring(NOISE.Length - 4, 4).ToUpper()) : string.Empty);
        }

        private static readonly Random Rnd = new Random();

        internal static string GeneratePassword(int minLength, int maxLength, string noise)
        {
            var length = Rnd.Next(minLength, maxLength + 1);

            var pwd = string.Empty;
            while (length-- > 0)
            {
                pwd += noise.Substring(Rnd.Next(noise.Length - 1), 1);
            }
            return pwd;
        }

        public static bool IsCertInstalled(Syscert.X509Certificate certificate, ILog log = null)
        {
            try
            {
                var monoX509 = new Monocert.X509Certificate(certificate.GetRawCertData());

                var store = WorkContext.IsMono
                    ? Monocert.X509StoreManager.CurrentUser.TrustedRoot
                    : Monocert.X509StoreManager.LocalMachine.TrustedRoot;

                return store.Certificates.Contains(monoX509);
            }
            catch (Exception ex)
            {
                if (log != null)
                    log.ErrorFormat("IsCertInstalled() failed. Error: {0}", ex);
            }

            return false;
        }

        public static bool TryInstallCert(Syscert.X509Certificate certificate, ILog log = null)
        {
            try
            {
                var monoX509 = new Monocert.X509Certificate(certificate.GetRawCertData());

                var store = WorkContext.IsMono
                    ? Monocert.X509StoreManager.CurrentUser.TrustedRoot
                    : Monocert.X509StoreManager.LocalMachine.TrustedRoot;

                // Add the certificate to the store.
                store.Import(monoX509);
                store.Certificates.Add(monoX509);

                return true;
            }
            catch (Exception ex)
            {
                if (log != null)
                    log.ErrorFormat("TryInstallCert() failed. Error: {0}", ex);
            }

            return false;
        }

        public static void SkipErrors(Action method, ILog log = null)
        {
            try
            {
                method();
            }
            catch (Exception ex)
            {
                if (log != null)
                    log.ErrorFormat("SkipErrors() failed. Error: {0}", ex);
            }
        }

        public static string GetContactsString(this UserInfo userInfo)
        {
            if (userInfo.Contacts.Count == 0) return null;
            var sBuilder = new StringBuilder();
            foreach (var contact in userInfo.Contacts)
            {
                sBuilder.AppendFormat("{0}|", contact);
            }
            return sBuilder.ToString();
        }

        public static string GetUserInfoString(this UserInfo userInfo)
        {
            return string.Format(
                "{{ ID: '{0}' SID: '{1}' Email '{2}' UserName: '{3}' FirstName: '{4}' LastName: '{5}' Title: '{6}' Location: '{7}' Contacts: '{8}' Status: '{9}' }}",
                userInfo.ID,
                userInfo.Sid,
                userInfo.Email,
                userInfo.UserName,
                userInfo.FirstName,
                userInfo.LastName,
                userInfo.Title,
                userInfo.Location,
                userInfo.GetContactsString(),
                Enum.GetName(typeof(EmployeeStatus), userInfo.Status));
        }
    }
}
