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
using System.Linq;
using ASC.FederatedLogin;
using ASC.FederatedLogin.Profile;
using ASC.Security.Cryptography;
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Web.Studio.Core
{
    public static class EncryptionLoginProvider
    {
        public static void SetKeys(Guid userId, string keys)
        {
            if (string.IsNullOrEmpty(keys)) return;

            var loginProfile = new LoginProfile
                {
                    Provider = ProviderConstants.Encryption,
                    Name = InstanceCrypto.Encrypt(keys),
                };

            var linker = new AccountLinker("webstudio");
            linker.AddLink(userId.ToString(), loginProfile);
        }


        public static string GetKeys()
        {
            return GetKeys(SecurityContext.CurrentAccount.ID);
        }

        public static string GetKeys(Guid userId)
        {
            var linker = new AccountLinker("webstudio");
            var profile = linker.GetLinkedProfiles(userId.ToString(), ProviderConstants.Encryption).FirstOrDefault();
            if (profile == null) return null;

            var keys = InstanceCrypto.Decrypt(profile.Name);
            return keys;
        }
    }
}