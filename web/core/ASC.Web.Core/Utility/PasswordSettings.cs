/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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
using System.Configuration;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;

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

        private static bool? printableASCII;

        private static bool PrintableASCII
        {
            get
            {
                if (printableASCII == null)
                {
                    printableASCII = true;

                    if (bool.TryParse(ConfigurationManagerExtension.AppSettings["web.password.ascii.cut"], out bool cut))
                    {
                        printableASCII = !cut;
                    }
                }

                return printableASCII.Value;
            }
        }


        [DataMember]
        public const int MaxLength = 30;

        [DataMember]
        public int MinLength { get; set; }


        [DataMember]
        public static string AllowedCharactersRegexStr
        {
            get
            {
                return PrintableASCII ? @"[\x21-\x7E]" : @"[0-9a-zA-Z!""#$%&()*+,.:;<>?@^_{}~]"; // excluding SPACE or (SPACE and '-/=[\]`|)
            }
        }


        [DataMember]
        public bool UpperCase { get; set; }

        [DataMember]
        public static string UpperCaseRegexStr
        {
            get
            {
                return @"(?=.*[A-Z])";
            }
        }


        [DataMember]
        public bool Digits { get; set; }

        [DataMember]
        public static string DigitsRegexStr
        {
            get
            {
                return @"(?=.*\d)";
            }
        }


        [DataMember]
        public bool SpecSymbols { get; set; }

        [DataMember]
        public static string SpecSymbolsRegexStr
        {
            get
            {
                return PrintableASCII ? @"(?=.*[\x21-\x2F\x3A-\x40\x5B-\x60\x7B-\x7E])" : @"(?=.*[!""#$%&()*+,.:;<>?@^_{}~])";
            }
        }


        private static PasswordSettings _default;

        public override ISettings GetDefault()
        {
            if (_default == null)
            {
                _default = new PasswordSettings { MinLength = 8, UpperCase = false, Digits = false, SpecSymbols = false };

                int defaultMinLength;
                if (int.TryParse(ConfigurationManagerExtension.AppSettings["web.password.min"], out defaultMinLength))
                {
                    _default.MinLength = Math.Max(1, Math.Min(MaxLength, defaultMinLength));
                }
            }

            return _default;
        }


        public static string GetPasswordRegex(PasswordSettings passwordSettings)
        {
            var pwdBuilder = new StringBuilder("^");

            if (passwordSettings.Digits)
                pwdBuilder.Append(DigitsRegexStr);

            if (passwordSettings.UpperCase)
                pwdBuilder.Append(UpperCaseRegexStr);

            if (passwordSettings.SpecSymbols)
                pwdBuilder.Append(SpecSymbolsRegexStr);

            pwdBuilder.Append(AllowedCharactersRegexStr + "{" + passwordSettings.MinLength + "," + MaxLength +"}$");

            return pwdBuilder.ToString();
        }


        public static bool CheckPasswordRegex(PasswordSettings passwordSettings, string password)
        {
            var passwordRegex = GetPasswordRegex(passwordSettings);

            return new Regex(passwordRegex).IsMatch(password);
        }
    }
}