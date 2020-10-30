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
using System.Text;

using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace ASC.Security.Cryptography
{
    public static class PasswordHasher
    {
        static PasswordHasher()
        {
            int size;
            if (!int.TryParse(ConfigurationManagerExtension.AppSettings["core.password.size"], out size)) size = 256;
            PasswordHashSize = size;

            int iterations;
            if (!int.TryParse(ConfigurationManagerExtension.AppSettings["core.password.iterations"], out iterations)) iterations = 100000;
            PasswordHashIterations = iterations;

            PasswordHashSalt = (ConfigurationManagerExtension.AppSettings["core.password.salt"] ?? "").Trim();
            if (string.IsNullOrEmpty(PasswordHashSalt))
            {
                var salt = Hasher.Hash("{9450BEF7-7D9F-4E4F-A18A-971D8681722D}", HashAlg.SHA256);

                var PasswordHashSaltBytes = KeyDerivation.Pbkdf2(
                                                   Encoding.UTF8.GetString(MachinePseudoKeys.GetMachineConstant()),
                                                   salt,
                                                   KeyDerivationPrf.HMACSHA256,
                                                   PasswordHashIterations,
                                                   PasswordHashSize / 8);
                PasswordHashSalt = BitConverter.ToString(PasswordHashSaltBytes).Replace("-", string.Empty).ToLower();
            }
        }

        public static int PasswordHashSize
        {
            get;
            private set;
        }

        public static int PasswordHashIterations
        {
            get;
            private set;
        }

        public static string PasswordHashSalt
        {
            get;
            private set;
        }

        public static string GetClientPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password)) password = Guid.NewGuid().ToString();

            byte[] salt = new UTF8Encoding(false).GetBytes(PasswordHashSalt);

            var hashBytes = KeyDerivation.Pbkdf2(
                               password,
                               salt,
                               KeyDerivationPrf.HMACSHA256,
                               PasswordHashIterations,
                               PasswordHashSize / 8);

            var hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLower();

            return hash;
        }
    }
}