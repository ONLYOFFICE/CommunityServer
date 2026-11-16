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
using System.Text;

using ASC.Common.Logging;
using ASC.Core;

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

        private const string Purpose = "EmailValidationKey";

        private static readonly ILog log = LogManager.GetLogger("ASC.KeyValidation.EmailSignature");
        private static readonly DateTime _from = new DateTime(2010, 01, 01, 0, 0, 0, DateTimeKind.Utc);
        private static readonly TimeSpan _clockSkew = TimeSpan.FromMinutes(5);

        public static string GetEmailKey(string email)
        {
            return GetEmailKey(CoreContext.TenantManager.GetCurrentTenant().TenantId, email);
        }

        public static string GetEmailKey(int tenantId, string email)
        {
            if (string.IsNullOrEmpty(email)) throw new ArgumentNullException("email");

            var ms = (long)(DateTime.UtcNow - _from).TotalMilliseconds;
            var hash = GetHashedData(tenantId, email, ms);
            return string.Format("{0}.{1}", ms, DoStringFromBytes(hash));
        }

        private static string FormatEmail(int tenantId, string email)
        {
            if (email == null) throw new ArgumentNullException("email");
            return string.Format("{0}|{1}", email.ToLowerInvariant(), tenantId);
        }

        public static ValidationResult ValidateEmailKey(string email, string key)
        {
            return ValidateEmailKey(email, key, TimeSpan.MaxValue);
        }

        public static ValidationResult ValidateEmailKey(int tenantId, string email, string key)
        {
            return ValidateEmailKey(tenantId, email, key, TimeSpan.MaxValue);
        }

        public static ValidationResult ValidateEmailKey(string email, string key, TimeSpan validInterval)
        {
            return ValidateEmailKey(CoreContext.TenantManager.GetCurrentTenant().TenantId, email, key, validInterval);
        }

        public static ValidationResult ValidateEmailKey(int tenantId, string email, string key, TimeSpan validInterval)
        {
            var result = ValidateEmailKeyInternal(tenantId, email, key, validInterval, GetHashedData);
            log.DebugFormat("validation result: {0}, source: {1} with key: {2} interval: {3} tenant: {4}", result, email, key, validInterval, tenantId);
            return result;
        }

        /// <summary>
        /// Validates a key signed with core.machinekey by versions before AuthSecretProvider.
        /// Use only for a short transition period, where the request is protected by other means as well.
        /// </summary>
        public static ValidationResult ValidateEmailKeyLegacy(string email, string key, TimeSpan validInterval)
        {
            var tenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId;
            var result = ValidateEmailKeyInternal(tenantId, email, key, validInterval, GetLegacyHashedData);
            log.DebugFormat("legacy validation result: {0}, source: {1} with key: {2} interval: {3} tenant: {4}", result, email, key, validInterval, tenantId);
            return result;
        }

        private static ValidationResult ValidateEmailKeyInternal(int tenantId, string email, string key, TimeSpan validInterval, Func<int, string, long, byte[]> getHashedData)
        {
            if (string.IsNullOrEmpty(email)) throw new ArgumentNullException("email");
            if (key == null) throw new ArgumentNullException("key");

            var parts = key.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2) return ValidationResult.Invalid;

            long ms = 0;
            if (!Int64.TryParse(parts[0], out ms)) return ValidationResult.Invalid;

            var ms_current = (long)(DateTime.UtcNow - _from).TotalMilliseconds;
            if (ms - ms_current > _clockSkew.TotalMilliseconds) return ValidationResult.Invalid;

            var hash = getHashedData(tenantId, email, ms);
            var key2 = DoStringFromBytes(hash);
            var key2_good = AuthSecretProvider.FixedTimeEquals(parts[1].ToUpperInvariant(), key2);
            if (!key2_good) return ValidationResult.Invalid;
            return validInterval >= TimeSpan.FromMilliseconds(Math.Max(0, ms_current - ms)) ? ValidationResult.Ok : ValidationResult.Expired;
        }

        internal static string DoStringFromBytes(byte[] data)
        {
            string str = Convert.ToBase64String(data);
            str = str.Replace("=", "").Replace("+", "").Replace("/", "").Replace("\\", "");
            return str.ToUpperInvariant();
        }

        private static byte[] GetHashedData(int tenantId, string email, long ms)
        {
            return AuthSecretProvider.ComputeHash(Purpose, Concat(Encoding.UTF8.GetBytes(FormatEmail(tenantId, email)), BitConverter.GetBytes(ms)));
        }

        private static byte[] GetLegacyHashedData(int tenantId, string email, long ms)
        {
            var formatted = string.Format("{0}|{1}", FormatEmail(tenantId, email), Encoding.UTF8.GetString(MachinePseudoKeys.GetMachineConstant()));
            return Hasher.Hash(Concat(Encoding.ASCII.GetBytes(formatted), BitConverter.GetBytes(ms)), HashAlg.SHA256);
        }

        private static byte[] Concat(byte[] data, byte[] salt)
        {
            var alldata = new byte[data.Length + salt.Length];
            Array.Copy(data, alldata, data.Length);
            Array.Copy(salt, 0, alldata, data.Length, salt.Length);
            return alldata;
        }
    }
}