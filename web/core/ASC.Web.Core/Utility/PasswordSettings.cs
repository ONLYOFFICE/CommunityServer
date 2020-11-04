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
using System.Configuration;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;

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

            return pwdBuilder.ToString();
        }


        public static bool CheckPasswordRegex(PasswordSettings passwordSettings, string password)
        {
            var passwordRegex = GetPasswordRegex(passwordSettings);

            return new Regex(passwordRegex).IsMatch(password);
        }
    }
}