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
using System.Text.RegularExpressions;

namespace ASC.Mail.Server.Utils
{
    public static class Parser
    {
        private static readonly Regex RegxWhiteSpaces = new Regex(@"\s+", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex RegxClean = new Regex(@"(\(((\\\))|[^)])*\))", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex RegxEmail = new Regex("<(.|[.])*?>", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex RegxDomain = new Regex(@"(?=^.{5,254}$)(^(?:(?!\d+\.)[a-zA-Z0-9_\-]{1,63}\.?)+\.(?:[a-zA-Z]{2,})$)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex RegxEmailLocalPart = new Regex(@"^([a-zA-Z0-9]+)([_\-\.][a-zA-Z0-9]+)*$", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);
        private static readonly Regex RegxPasswordValidation = new Regex(@"^[a-zA-Z]\w{5,14}$", RegexOptions.Compiled | RegexOptions.Singleline);

        /// <summary>
        /// Parses the address.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static Address ParseAddress(string input)
        {
            var address = new Address();
            input = input.TrimEnd(';');

            if (input.IndexOf("<", StringComparison.Ordinal) == -1)
            {
                address.Email = RemoveWhiteSpaces(input);
            }
            else
            {
                foreach (Match match in RegxEmail.Matches(input))
                {
                    //This needed because only last match is email. SAmple name: name<teamlab>endname<teamlab@teamlab.com>.
                    //Two matches: <teamlab>, <teamlab@teamlab.com> - only last match - email.
                    // if format like name<teamlab@teamlab.com>endname<teamlab>.Its incorrect address.
                    address.Email = match.Value.TrimStart('<').TrimEnd('>');
                }

                address.Name = input.Replace("<" + address.Email + ">", "");
                address.Email = Clean(RemoveWhiteSpaces(address.Email));
                if (address.Name.IndexOf("\"", StringComparison.Ordinal) == -1) address.Name = Clean(address.Name);
                address.Name = address.Name.Trim(new[] {' ', '\"'});
            }

            var index = address.Email.IndexOf('@');
            if (index > -1)
            {
                var parts = address.Email.Split(new[] {'@'});
                address.LocalPart = parts[0];
                address.Domain = parts[1];
            }

            return address;
        }

        /// <summary>
        /// Check email's local part validity.
        /// </summary>
        /// <param name="input">Email's local part.</param>
        /// <returns>true - valid, false - invalid</returns>
        public static bool IsEmailLocalPartValid(string input)
        {
            return RegxEmailLocalPart.IsMatch(input);
        }

        /// <summary>
        /// Check domain validity.
        /// </summary>
        /// <param name="input">Domain string.</param>
        /// <returns>true - valid, false - invalid</returns>
        public static bool IsDomainValid(string input)
        {
            return RegxDomain.IsMatch(input);
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
        /// Check password validity.
        /// </summary>
        /// <param name="password">The password's first character must be a letter, it must contain at least 6 characters and no more than 15 characters and no characters other than letters, numbers and the underscore may be used</param>
        /// <returns>true - valid, false - invalid</returns>
        public static bool IsPasswordValid(string password)
        {
            return RegxPasswordValidation.IsMatch(password);
        }
    }
}
