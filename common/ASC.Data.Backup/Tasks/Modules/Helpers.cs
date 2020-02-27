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
