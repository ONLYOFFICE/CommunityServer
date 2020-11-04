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


using ASC.Core;
using System;
using System.Configuration;
using System.Text;
using ASC.Common.Logging;

namespace ASC.Security.Cryptography
{
    public class EmailValidationKeyProvider
    {
        public enum ValidationResult
        {
            Ok,
            Invalid,
            Expired
        }

        private static readonly ILog log = LogManager.GetLogger("ASC.KeyValidation.EmailSignature");
        private static readonly DateTime _from = new DateTime(2010, 01, 01, 0, 0, 0, DateTimeKind.Utc);

        public static string GetEmailKey(string email)
        {
            return GetEmailKey(CoreContext.TenantManager.GetCurrentTenant().TenantId, email);
        }

        public static string GetEmailKey(int tenantId, string email)
        {
            if (string.IsNullOrEmpty(email)) throw new ArgumentNullException("email");

            email = FormatEmail(tenantId, email);

            var ms = (long)(DateTime.UtcNow - _from).TotalMilliseconds;
            var hash = GetMashineHashedData(BitConverter.GetBytes(ms), Encoding.ASCII.GetBytes(email));
            return string.Format("{0}.{1}", ms, DoStringFromBytes(hash));
        }

        private static string FormatEmail(int tenantId, string email)
        {
            if (email == null) throw new ArgumentNullException("email");
            try
            {
                return string.Format("{0}|{1}|{2}", email.ToLowerInvariant(), tenantId, Encoding.UTF8.GetString(MachinePseudoKeys.GetMachineConstant()));
            }
            catch (Exception e)
            {
                log.Fatal("Failed to format tenant specific email", e);
                return email.ToLowerInvariant();
            }
        }

        public static ValidationResult ValidateEmailKey(string email, string key)
        {
            return ValidateEmailKey(email, key, TimeSpan.MaxValue);
        }

        public static ValidationResult ValidateEmailKey(string email, string key, TimeSpan validInterval)
        {
            var result = ValidateEmailKeyInternal(email, key, validInterval);
            log.DebugFormat("validation result: {0}, source: {1} with key: {2} interval: {3} tenant: {4}", result, email, key, validInterval, CoreContext.TenantManager.GetCurrentTenant().TenantId);
            return result;
        }


        private static ValidationResult ValidateEmailKeyInternal(string email, string key, TimeSpan validInterval)
        {
            if (string.IsNullOrEmpty(email)) throw new ArgumentNullException("email");
            if (key == null) throw new ArgumentNullException("key");

            email = FormatEmail(CoreContext.TenantManager.GetCurrentTenant().TenantId, email);
            var parts = key.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2) return ValidationResult.Invalid;

            long ms = 0;
            if (!Int64.TryParse(parts[0], out ms)) return ValidationResult.Invalid;

            var hash = GetMashineHashedData(BitConverter.GetBytes(ms), Encoding.ASCII.GetBytes(email));
            var key2 = DoStringFromBytes(hash);
            var key2_good = String.Compare(parts[1], key2, StringComparison.InvariantCultureIgnoreCase) == 0;
            if (!key2_good) return ValidationResult.Invalid;
            var ms_current = (long)(DateTime.UtcNow - _from).TotalMilliseconds;
            return validInterval >= TimeSpan.FromMilliseconds(ms_current - ms)?ValidationResult.Ok : ValidationResult.Expired;
        }

        internal static string DoStringFromBytes(byte[] data)
        {
            string str = Convert.ToBase64String(data);
            str = str.Replace("=", "").Replace("+", "").Replace("/", "").Replace("\\", "");
            return str.ToUpperInvariant();
        }

        internal static byte[] GetMashineHashedData(byte[] salt, byte[] data)
        {
            var alldata = new byte[salt.Length + data.Length];
            Array.Copy(data, alldata, data.Length);
            Array.Copy(salt, 0, alldata, data.Length, salt.Length);
            return Hasher.Hash(alldata, HashAlg.SHA256);
        }
    }
}