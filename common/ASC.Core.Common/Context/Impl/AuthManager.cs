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
using ASC.Common.Security.Authentication;
using ASC.Core.Security.Authentication;
using ASC.Core.Users;

namespace ASC.Core
{
    public class AuthManager
    {
        private readonly IUserService userService;


        public AuthManager(IUserService service)
        {
            this.userService = service;
        }


        public IUserAccount[] GetUserAccounts()
        {
            return CoreContext.UserManager.GetUsers(EmployeeStatus.Active).Select(u => ToAccount(u)).ToArray();
        }

        public void SetUserPasswordHash(Guid userID, string passwordHash)
        {
            userService.SetUserPasswordHash(CoreContext.TenantManager.GetCurrentTenant().TenantId, userID, passwordHash);
        }

        public DateTime GetUserPasswordStamp(Guid userID)
        {
            return userService.GetUserPasswordStamp(CoreContext.TenantManager.GetCurrentTenant().TenantId, userID);
        }

        public IAccount GetAccountByID(Guid id)
        {
            var s = ASC.Core.Configuration.Constants.SystemAccounts.FirstOrDefault(a => a.ID == id);
            if (s != null) return s;
 
            var u = CoreContext.UserManager.GetUsers(id);
            return !Constants.LostUser.Equals(u) && u.Status == EmployeeStatus.Active ? (IAccount)ToAccount(u) : ASC.Core.Configuration.Constants.Guest;
        }


        private IUserAccount ToAccount(UserInfo u)
        {
            return new UserAccount(u, CoreContext.TenantManager.GetCurrentTenant().TenantId);
        }
    }
}