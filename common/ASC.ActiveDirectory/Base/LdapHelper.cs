/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
