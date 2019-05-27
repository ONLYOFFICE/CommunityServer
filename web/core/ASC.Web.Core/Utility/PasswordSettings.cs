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
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using ASC.Core;
using ASC.Core.Common.Settings;

namespace ASC.Web.Core.Utility
{
    [Serializable]
    [DataContract]
    public sealed class PasswordSettings : BaseSettings<PasswordSettings>
    {
        public override Guid ID
        {
            get { return new Guid("aa93a4d1-012d-4ccd-895a-e094e809c840"); }
        }

        public const int MaxLength = 30;

        /// <summary>
        /// Minimal length password has
        /// </summary>
        [DataMember]
        public int MinLength { get; set; }

        /// <summary>
        /// Password must contains upper case
        /// </summary>
        [DataMember]
        public bool UpperCase { get; set; }

        /// <summary>
        /// Password must contains digits
        /// </summary>
        [DataMember]
        public bool Digits { get; set; }

        /// <summary>
        /// Password must contains special symbols
        /// </summary>
        [DataMember]
        public bool SpecSymbols { get; set; }

        private static PasswordSettings _default;

        public override ISettings GetDefault()
        {
            if (_default == null)
            {
                _default = new PasswordSettings { MinLength = 6, UpperCase = false, Digits = false, SpecSymbols = false };

                int defaultMinLength;
                if (int.TryParse(WebConfigurationManager.AppSettings["web.password.min"], out defaultMinLength))
                {
                    _default.MinLength = Math.Max(1, Math.Min(MaxLength, defaultMinLength));
                }
            }

            return _default;
        }


        public static bool CheckPasswordRegex(PasswordSettings passwordSettings, string password)
        {
            var pwdBuilder = new StringBuilder();

            if (CoreContext.Configuration.CustomMode)
            {
                pwdBuilder.Append(@"^(?=.*[a-z]{0,})");

                if (passwordSettings.Digits)
                    pwdBuilder.Append(@"(?=.*\d)");

                if (passwordSettings.UpperCase)
                    pwdBuilder.Append(@"(?=.*[A-Z])");

                if (passwordSettings.SpecSymbols)
                    pwdBuilder.Append(@"(?=.*[_\-.~!$^*()=|])");

                pwdBuilder.Append(@"[0-9a-zA-Z_\-.~!$^*()=|]");
            }
            else
            {
                pwdBuilder.Append(@"^(?=.*\p{Ll}{0,})");

                if (passwordSettings.Digits)
                    pwdBuilder.Append(@"(?=.*\d)");

                if (passwordSettings.UpperCase)
                    pwdBuilder.Append(@"(?=.*\p{Lu})");

                if (passwordSettings.SpecSymbols)
                    pwdBuilder.Append(@"(?=.*[\W])");

                pwdBuilder.Append(@".");
            }

            pwdBuilder.Append(@"{");
            pwdBuilder.Append(passwordSettings.MinLength);
            pwdBuilder.Append(@",");
            pwdBuilder.Append(MaxLength);
            pwdBuilder.Append(@"}$");

            return new Regex(pwdBuilder.ToString()).IsMatch(password);
        }
    }
}