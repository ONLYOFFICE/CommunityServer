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
using System.IO;
using System.Security.Cryptography;
using System.Text;

using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Core.Tenants;

namespace ASC.Security.Cryptography
{
    /// <summary>
    /// Per-installation random secret for authentication tokens (confirmation links, auth cookies, login profiles).
    /// Unlike core.machinekey it is never shipped in configs: it is generated on first use and kept in core_settings,
    /// so all web nodes and services sharing the database use the same value.
    /// </summary>
    public static class AuthSecretProvider
    {
        private const string SettingsKey = "AuthSecret";
        private const int SecretSize = 32;
        private const int IvSize = 16;
        private const int MacSize = 32;

        private static readonly object locker = new object();
        private static byte[] secret;
        private static DateTime created;


        public static DateTime Created
        {
            get
            {
                EnsureLoaded();
                return created;
            }
        }

        public static byte[] ComputeHash(string purpose, byte[] data)
        {
            using (var hmac = new HMACSHA256(DeriveKey(purpose)))
            {
                return hmac.ComputeHash(data);
            }
        }

        public static string GetSecretString(string purpose)
        {
            return Convert.ToBase64String(DeriveKey(purpose));
        }

        public static byte[] Protect(string purpose, byte[] data)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = DeriveKey(purpose + ".enc");
                aes.GenerateIV();

                using (var ms = new MemoryStream())
                {
                    ms.Write(aes.IV, 0, aes.IV.Length);
                    using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                        cs.FlushFinalBlock();

                        var encrypted = ms.ToArray();
                        var mac = ComputeHash(purpose + ".mac", encrypted);

                        var result = new byte[encrypted.Length + mac.Length];
                        Buffer.BlockCopy(encrypted, 0, result, 0, encrypted.Length);
                        Buffer.BlockCopy(mac, 0, result, encrypted.Length, mac.Length);
                        return result;
                    }
                }
            }
        }

        public static byte[] Unprotect(string purpose, byte[] data)
        {
            if (data == null || data.Length < IvSize + MacSize + 16) return null;

            var encrypted = new byte[data.Length - MacSize];
            var mac = new byte[MacSize];
            Buffer.BlockCopy(data, 0, encrypted, 0, encrypted.Length);
            Buffer.BlockCopy(data, encrypted.Length, mac, 0, MacSize);

            if (!FixedTimeEquals(mac, ComputeHash(purpose + ".mac", encrypted))) return null;

            using (var aes = Aes.Create())
            {
                aes.Key = DeriveKey(purpose + ".enc");
                var iv = new byte[IvSize];
                Buffer.BlockCopy(encrypted, 0, iv, 0, IvSize);
                aes.IV = iv;

                using (var decryptor = aes.CreateDecryptor())
                {
                    return decryptor.TransformFinalBlock(encrypted, IvSize, encrypted.Length - IvSize);
                }
            }
        }

        public static bool FixedTimeEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;

            var diff = 0;
            for (var i = 0; i < a.Length; i++)
            {
                diff |= a[i] ^ b[i];
            }
            return diff == 0;
        }

        public static bool FixedTimeEquals(string a, string b)
        {
            if (a == null || b == null) return false;
            return FixedTimeEquals(Encoding.UTF8.GetBytes(a), Encoding.UTF8.GetBytes(b));
        }

        private static byte[] DeriveKey(string purpose)
        {
            EnsureLoaded();
            using (var hmac = new HMACSHA256(secret))
            {
                return hmac.ComputeHash(Encoding.UTF8.GetBytes(purpose));
            }
        }

        private static void EnsureLoaded()
        {
            if (secret != null) return;

            lock (locker)
            {
                if (secret != null) return;

                using (var db = new DbManager("default"))
                {
                    var value = Read(db);
                    if (value == null)
                    {
                        var random = new byte[SecretSize];
                        using (var rng = new RNGCryptoServiceProvider())
                        {
                            rng.GetBytes(random);
                        }

                        value = new byte[sizeof(long) + SecretSize];
                        Buffer.BlockCopy(BitConverter.GetBytes(DateTime.UtcNow.Ticks), 0, value, 0, sizeof(long));
                        Buffer.BlockCopy(random, 0, value, sizeof(long), SecretSize);

                        // insert ignore: if another node has just created the secret, keep its value
                        db.ExecuteNonQuery(new SqlInsert("core_settings")
                            .IgnoreExists(true)
                            .InColumns("tenant", "id", "value")
                            .Values(Tenant.DEFAULT_TENANT, SettingsKey, value));

                        value = Read(db);
                    }

                    if (value == null || value.Length != sizeof(long) + SecretSize)
                    {
                        throw new InvalidOperationException("Invalid " + SettingsKey + " value in core_settings");
                    }

                    var bytes = new byte[SecretSize];
                    Buffer.BlockCopy(value, sizeof(long), bytes, 0, SecretSize);

                    created = new DateTime(BitConverter.ToInt64(value, 0), DateTimeKind.Utc);
                    secret = bytes;
                }
            }
        }

        private static byte[] Read(IDbManager db)
        {
            return db.ExecuteScalar<byte[]>(new SqlQuery("core_settings")
                .Select("value")
                .Where("tenant", Tenant.DEFAULT_TENANT)
                .Where("id", SettingsKey));
        }
    }
}