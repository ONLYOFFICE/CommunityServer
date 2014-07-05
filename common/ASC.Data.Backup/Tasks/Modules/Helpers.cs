/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

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
    }
}
