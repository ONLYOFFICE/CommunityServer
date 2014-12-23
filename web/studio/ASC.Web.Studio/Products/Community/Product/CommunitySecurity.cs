/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Security;
using ASC.Common.Security;
using ASC.Common.Security.Authorizing;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core;
using Action = ASC.Common.Security.Authorizing.Action;
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Web.Community.Product
{
    public static class CommunitySecurity
    {
        public static bool IsAdministrator()
        {
            return CoreContext.UserManager.IsUserInGroup(SecurityContext.CurrentAccount.ID, ASC.Core.Users.Constants.GroupAdmin.ID) ||
                WebItemSecurity.IsProductAdministrator(CommunityProduct.ID, SecurityContext.CurrentAccount.ID);
        }

        public static bool IsOutsider()
        {
            return CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsOutsider();
        }

        public static bool CheckPermissions(params IAction[] actions)
        {
            return CheckPermissions(null, null, actions);
        }

        public static bool CheckPermissions(ISecurityObject securityObject, params IAction[] actions)
        {
            return CheckPermissions(securityObject, null, actions);
        }

        public static bool CheckPermissions(ISecurityObjectId objectId, ISecurityObjectProvider securityObjProvider, params IAction[] actions)
        {
            if (IsAdministrator()) return true;
            if (IsOutsider())
            {
                var actionArray = actions ?? new IAction[0];
                var containsReadAction = false;
                foreach (var action in actionArray)
                {
                    containsReadAction = action.ID.Equals(new Guid("{E0759A42-47F0-4763-A26A-D5AA665BEC35}"));//"Read forum post action"
                }
                if (!containsReadAction) return false;
            }

            return SecurityContext.CheckPermissions(objectId, securityObjProvider, actions);
        }

        public static void DemandPermissions(params IAction[] actions)
        {
            DemandPermissions(null, null, actions);
        }

        public static void DemandPermissions(ISecurityObject securityObject, params IAction[] actions)
        {
            DemandPermissions(securityObject, null, actions);
        }

        public static void DemandPermissions(ISecurityObjectId objectId, ISecurityObjectProvider securityObjProvider, params IAction[] actions)
        {
            if (IsAdministrator()) return;
            if (IsOutsider()) throw new SecurityException();

            SecurityContext.DemandPermissions(objectId, securityObjProvider, actions);
        }
    }
}
