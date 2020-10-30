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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ASC.ActiveDirectory.Base.Data;
using ASC.ActiveDirectory.Base.Expressions;
using ASC.ActiveDirectory.Base.Settings;
using ASC.Common.Logging;
using ASC.Security.Cryptography;

namespace ASC.ActiveDirectory.Base
{
    public abstract class LdapHelper : IDisposable
    {
        public LdapSettings Settings { get; private set; }

        public abstract bool IsConnected { get; }

        protected readonly ILog Log;

        protected LdapHelper(LdapSettings settings)
        {
            Settings = settings;
            Log = LogManager.GetLogger("ASC");
        }

        public abstract void Connect();

        public abstract Dictionary<string, string[]> GetCapabilities();

        public abstract string SearchDomain();

        public abstract void CheckCredentials(string login, string password, string server, int portNumber,
            bool startTls, bool ssl, bool acceptCertificate, string acceptCertificateHash);

        public abstract bool CheckUserDn(string userDn);

        public abstract List<LdapObject> GetUsers(string filter = null, int limit = -1);

        public abstract LdapObject GetUserBySid(string sid);

        public abstract bool CheckGroupDn(string groupDn);

        public abstract List<LdapObject> GetGroups(Criteria criteria = null);

        public bool UserExistsInGroup(LdapObject domainGroup, LdapObject domainUser, LdapSettings settings) // string memberString, string groupAttribute, string primaryGroupId)
        {
            try
            {
                if (domainGroup == null || domainUser == null)
                    return false;

                var memberString = domainUser.GetValue(Settings.UserAttribute) as string;
                if (string.IsNullOrEmpty(memberString))
                    return false;

                var groupAttribute = settings.GroupAttribute;
                if (string.IsNullOrEmpty(groupAttribute))
                    return false;

                var userPrimaryGroupId = domainUser.GetValue(LdapConstants.ADSchemaAttributes.PRIMARY_GROUP_ID) as string;

                if (!string.IsNullOrEmpty(userPrimaryGroupId) && domainGroup.Sid.EndsWith("-" + userPrimaryGroupId))
                {
                    // Domain Users found
                    return true;
                }
                else
                {
                    var members = domainGroup.GetValues(groupAttribute);

                    if (!members.Any())
                        return false;

                    if (members.Any(member => memberString.Equals(member, StringComparison.InvariantCultureIgnoreCase)))
                        return true;
                }
            }
            catch (Exception e)
            {
                Log.ErrorFormat("UserExistsInGroup() failed. Error: {0}", e);
            }

            return false;
        }

        public static string GetPassword(byte[] passwordBytes)
        {
            if (passwordBytes == null || passwordBytes.Length == 0)
                return string.Empty;
            
            string password;
            try
            {
                password = new UnicodeEncoding().GetString(InstanceCrypto.Decrypt(passwordBytes));
            }
            catch (Exception)
            {
                password = string.Empty;
            }
            return password;
        }

        public static byte[] GetPasswordBytes(string password)
        {
            byte[] passwordBytes;

            try
            {
                passwordBytes = InstanceCrypto.Encrypt(new UnicodeEncoding().GetBytes(password));
            }
            catch (Exception)
            {
                passwordBytes = null;
            }

            return passwordBytes;
        }

        public abstract void Dispose();
    }
}
