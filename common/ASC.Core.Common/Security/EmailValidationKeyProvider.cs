/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
                return string.Format("{0}|{1}|{2}", email.ToLowerInvariant(), tenantId, ConfigurationManager.AppSettings["core.machinekey"]);
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