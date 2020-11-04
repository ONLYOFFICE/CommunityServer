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
using System.Text.RegularExpressions;

namespace ASC.Core.Tenants
{
    public class TenantDomainValidator
    {
        private static readonly Regex ValidDomain = new Regex("^[a-z0-9]([a-z0-9-]){1,98}[a-z0-9]$",
                                                              RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        private static readonly int MinLength;
        private const int MaxLength = 100;

        static TenantDomainValidator()
        {
            MinLength = 6;

            int defaultMinLength;
            if (int.TryParse(ConfigurationManagerExtension.AppSettings["web.alias.min"], out defaultMinLength))
            {
                MinLength = Math.Max(1, Math.Min(MaxLength, defaultMinLength));
            }
        }

        public static void ValidateDomainLength(string domain)
        {
            if (string.IsNullOrEmpty(domain)
                || domain.Length < MinLength || MaxLength < domain.Length)
            {
                throw new TenantTooShortException("The domain name must be between " + MinLength + " and " + MaxLength + " characters long.", MinLength, MaxLength);
            }
        }

        public static void ValidateDomainCharacters(string domain)
        {
            if (!ValidDomain.IsMatch(domain))
            {
                throw new TenantIncorrectCharsException("Domain contains invalid characters.");
            }
        }
    }
}