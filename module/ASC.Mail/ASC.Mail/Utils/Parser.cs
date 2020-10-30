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


using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using ASC.Mail.Data.Contracts;
using ASC.Web.Core.Utility;
using MimeKit;
using Resources;

namespace ASC.Mail.Utils
{
    public static class Parser
    {
        private static readonly Regex RegxWhiteSpaces = new Regex(@"\s+", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex RegxClean = new Regex(@"(\(((\\\))|[^)])*\))", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        //private static readonly Regex RegxEmail = new Regex("<(.|[.])*?>", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex RegxDomain = new Regex(@"(?=^.{4,253}$)(^((?!-)[a-zA-Z0-9-]{1,63}(?<!-)\.)+[a-zA-Z]{2,63}\.?$)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex RegxEmailLocalPart = new Regex(@"^([a-zA-Z0-9]+)([_\-\.\+][a-zA-Z0-9]+)*$", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);
        private static readonly Regex RegxNoneAscii = new Regex(@"[^\x00-\x7F]+", RegexOptions.Compiled | RegexOptions.Singleline);

        /// <summary>
        /// Parses the address.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static Address ParseAddress(string input)
        {
            try
            {
                var addressObj = MailboxAddress.Parse(ParserOptions.Default, input);

                var adr = new MailAddress(addressObj.Address, addressObj.Name);

                var address = new Address(addressObj.Address, addressObj.Name)
                {
                    LocalPart = adr.User,
                    Domain = adr.Host
                };

                if (!RegxEmailLocalPart.IsMatch(address.LocalPart))
                    throw new Exception("Local-part is invalid");

                if (!RegxDomain.IsMatch(address.Domain))
                    throw new Exception("Domain is invalid");

                return address;
            }
            catch (Exception ex)
            {
                throw new Exception("Parse failed", ex);
            }
        }

        public static bool TryParseAddress(string input, out Address address)
        {
            try
            {
                address = ParseAddress(input);

                return true;
            }
            catch (Exception)
            {
                address = null;
                return false;
            }
        }

        /// <summary>
        /// Check email's local part validity.
        /// </summary>
        /// <param name="input">Email's local part.</param>
        /// <returns>true - valid, false - invalid</returns>
        public static bool IsEmailLocalPartValid(string input)
        {
            try
            {
                var address = ParseAddress(input + "@test.com");

                return address != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Check domain validity.
        /// </summary>
        /// <param name="input">Domain string.</param>
        /// <returns>true - valid, false - invalid</returns>
        public static bool IsDomainValid(string input)
        {
            try
            {
                var address = ParseAddress("test@" + input);

                return address != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Removes the white spaces.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        internal static string RemoveWhiteSpaces(string input)
        {
            return RegxWhiteSpaces.Replace(input, "");
        }

        /// <summary>
        /// Cleans the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        internal static string Clean(string input)
        {
            return RegxClean.Replace(input, "").Trim(' ');
        }

        /// <summary>
        /// Get valid password or throw exception.
        /// </summary>
        /// <param name="password">String contains valid password according to portal settings</param>
        /// <returns>trimmed password or exception</returns>
        public static string GetValidPassword(string password)
        {
            var trimPwd = password.Trim();

            if (string.IsNullOrEmpty(trimPwd))
                throw new ArgumentException(Resource.ErrorPasswordEmpty);

            var pwdSettings = PasswordSettings.Load();

            if (!PasswordSettings.CheckPasswordRegex(pwdSettings, trimPwd)
                || RegxNoneAscii.IsMatch(trimPwd)
                || RegxWhiteSpaces.IsMatch(trimPwd))
            {
                throw new ArgumentException(GeneratePasswordErrorMessage(pwdSettings));
            }

            return trimPwd;
        }

        internal static string GeneratePasswordErrorMessage(PasswordSettings passwordSettings)
        {
            var error = new StringBuilder();

            error.AppendFormat("{0} ", Resource.ErrorPasswordMessage);
            error.AppendFormat(Resource.ErrorPasswordLength, passwordSettings.MinLength, PasswordSettings.MaxLength);

            error.AppendFormat(", {0}", Resource.ErrorPasswordOnlyLatinLetters);
            error.AppendFormat(", {0}", Resource.ErrorPasswordNoSpaces);

            if (passwordSettings.UpperCase)
                error.AppendFormat(", {0}", Resource.ErrorPasswordNoUpperCase);
            if (passwordSettings.Digits)
                error.AppendFormat(", {0}", Resource.ErrorPasswordNoDigits);
            if (passwordSettings.SpecSymbols)
                error.AppendFormat(", {0}", Resource.ErrorPasswordNoSpecialSymbols);

            return error.ToString();
        }

        public static List<MailAddress> ToMailAddresses(this List<string> addresses)
        {
            var toList = new List<MailAddress>();

            foreach (var strAddress in addresses)
            {
                Address address;

                if (!TryParseAddress(strAddress, out address))
                    throw new Exception(string.Format("Address '{0}' is invalid", strAddress));

                toList.Add(new MailAddress(address.Email, address.Name));
            }

            return toList;
        }
    }
}
