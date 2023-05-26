/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
                if (!printableASCII.HasValue)
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


        private static int? limitMaxLength;

        [DataMember]
        public static int LimitMaxLength
        {
            get
            {
                if (!limitMaxLength.HasValue)
                {
                    limitMaxLength = 120;

                    if (int.TryParse(ConfigurationManagerExtension.AppSettings["web.password.max"], out int max))
                    {
                        limitMaxLength = max;
                    }
                }

                return limitMaxLength.Value;
            }
        }


        private static int? limitMinLength;

        [DataMember]
        public static int LimitMinLength
        {
            get
            {
                if (!limitMinLength.HasValue)
                {
                    limitMinLength = 8;

                    if (int.TryParse(ConfigurationManagerExtension.AppSettings["web.password.min"], out int min))
                    {
                        limitMinLength = min;
                    }
                }

                return limitMinLength.Value;
            }
        }


        private int maxLength;

        [DataMember]
        public int MaxLength
        {
            get
            {
                return maxLength == 0 ? LimitMaxLength : maxLength;
            }
            set
            {
                maxLength = GetLimitedValue(value);
            }
        }


        private int minLength;

        [DataMember]
        public int MinLength
        {
            get
            {
                return minLength == 0 ? LimitMinLength : minLength;
            }
            set
            {
                minLength = GetLimitedValue(value);
            }
        }


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


        public override ISettings GetDefault()
        {
            return new PasswordSettings { MaxLength = LimitMaxLength, MinLength = LimitMinLength, UpperCase = false, Digits = false, SpecSymbols = false };
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

            pwdBuilder.Append(AllowedCharactersRegexStr + "{" + passwordSettings.MinLength + "," + passwordSettings.MaxLength + "}$");

            return pwdBuilder.ToString();
        }


        public static bool CheckPasswordRegex(PasswordSettings passwordSettings, string password)
        {
            var passwordRegex = GetPasswordRegex(passwordSettings);

            return new Regex(passwordRegex).IsMatch(password);
        }


        private int GetLimitedValue(int value)
        {
            return value > LimitMaxLength
                ? LimitMaxLength
                : value < LimitMinLength
                    ? LimitMinLength
                    : value;
        }
    }
}