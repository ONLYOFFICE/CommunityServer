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
using ASC.Security.Cryptography;
using System;
using System.Linq;
using ConfigurationConstants = ASC.Core.Configuration.Constants;
using UserConstants = ASC.Core.Users.Constants;

namespace ASC.Data.Backup.Tasks.Modules
{
    internal static class Helpers
    {
        private static readonly Guid[] SystemUsers = new[]
            {
                Guid.Empty,
                ConfigurationConstants.CoreSystem.ID,
                ConfigurationConstants.Guest.ID,
                UserConstants.LostUser.ID
            };

        private static readonly Guid[] SystemGroups = new[]
            {
                Guid.Empty,
                UserConstants.LostGroupInfo.ID,
                UserConstants.GroupAdmin.ID,
                UserConstants.GroupEveryone.ID,
                UserConstants.GroupVisitor.ID,
                UserConstants.GroupUser.ID,
                new Guid("{EA942538-E68E-4907-9394-035336EE0BA8}"), //community product
                new Guid("{1e044602-43b5-4d79-82f3-fd6208a11960}"), //projects product
                new Guid("{6743007C-6F95-4d20-8C88-A8601CE5E76D}"), //crm product
                new Guid("{E67BE73D-F9AE-4ce1-8FEC-1880CB518CB4}"), //documents product
                new Guid("{F4D98AFD-D336-4332-8778-3C6945C81EA0}"), //people product
                new Guid("{2A923037-8B2D-487b-9A22-5AC0918ACF3F}"), //mail product
                new Guid("{32D24CB5-7ECE-4606-9C94-19216BA42086}"), //calendar product
                new Guid("{37620AE5-C40B-45ce-855A-39DD7D76A1FA}"), //birthdays product
                new Guid("{BF88953E-3C43-4850-A3FB-B1E43AD53A3E}")  //talk product
            };

        public static bool IsEmptyOrSystemUser(string id)
        {
            Guid g;
            return string.IsNullOrEmpty(id) || Guid.TryParse(id, out g) && SystemUsers.Contains(g);
        }

        public static bool IsEmptyOrSystemGroup(string id)
        {
            Guid g;
            return string.IsNullOrEmpty(id) || Guid.TryParse(id, out g) && SystemGroups.Contains(g);
        }

        public static string CreateHash(string s)
        {
            return !string.IsNullOrEmpty(s) && s.StartsWith("S|") ? InstanceCrypto.Encrypt(Crypto.GetV(s.Substring(2), 1, false)) : s;
        }

        public static string CreateHash2(string s)
        {
            return !string.IsNullOrEmpty(s) ? "S|" + Crypto.GetV(InstanceCrypto.Decrypt(s), 1, true) : s;
        }
    }
}
