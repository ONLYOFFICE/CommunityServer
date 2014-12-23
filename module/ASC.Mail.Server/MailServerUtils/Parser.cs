/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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
    }
}
