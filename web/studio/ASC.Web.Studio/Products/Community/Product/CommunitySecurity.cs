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
