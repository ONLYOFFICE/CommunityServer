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
using ASC.Common.Security;
using ASC.Common.Security.Authentication;
using ASC.Common.Security.Authorizing;
using ASC.Core.Users;
using AuthConst = ASC.Common.Security.Authorizing.Constants;
using ConfConst = ASC.Core.Configuration.Constants;

namespace ASC.Core.Security.Authorizing
{
    class RoleProvider : IRoleProvider
    {
        public List<IRole> GetRoles(ISubject account)
        {
            var roles = new List<IRole>();
            if (!(account is ISystemAccount))
            {
                if (account is IRole)
                {
                    roles = GetParentRoles(account.ID).ToList();
                }
                else if (account is IUserAccount)
                {
                    roles = CoreContext.UserManager
                                       .GetUserGroups(account.ID, IncludeType.Distinct | IncludeType.InParent)
                                       .Select(g => (IRole) g)
                                       .ToList();
                }
            }
            return roles;
        }

        public bool IsSubjectInRole(ISubject account, IRole role)
        {
            return CoreContext.UserManager.IsUserInGroup(account.ID, role.ID);
        }

        private static List<IRole> GetParentRoles(Guid roleID)
        {
            var roles = new List<IRole>();
            var gi = CoreContext.UserManager.GetGroupInfo(roleID);
            if (gi != null)
            {
                var parent = gi.Parent;
                while (parent != null)
                {
                    roles.Add(parent);
                    parent = parent.Parent;
                }
            }
            return roles;
        }
    }
}