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
using System.Security.Authentication;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Data.Storage;
using ASC.Mail.Data.Contracts;
using ASC.Mail.Utils;

namespace ASC.Mail.Extensions
{
    public static class MailBoxExtensions
    {
        public static bool IsUserTerminated(this MailBoxData mailbox)
        {
            try
            {
                CoreContext.TenantManager.SetCurrentTenant(mailbox.TenantId);

                var user = CoreContext.UserManager.GetUsers(new Guid(mailbox.UserId));

                return user.Status == EmployeeStatus.Terminated;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsUserRemoved(this MailBoxData mailbox)
        {
            try
            {
                CoreContext.TenantManager.SetCurrentTenant(mailbox.TenantId);
                Guid user;
                if (!Guid.TryParse(mailbox.UserId, out user))
                    return true;

                return !CoreContext.UserManager.UserExists(user) || CoreContext.UserManager.IsSystemUser(user);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static UserInfo GetUserInfo(this MailBoxData mailbox)
        {
            try
            {
                CoreContext.TenantManager.SetCurrentTenant(mailbox.TenantId);
                var userInfo = CoreContext.UserManager.GetUsers(new Guid(mailbox.UserId));

                return userInfo;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static Defines.TariffType GetTenantStatus(this MailBoxData mailbox, int tenantOverdueDays,
            string httpContextScheme, ILog log = null)
        {
            log = log ?? new NullLog();

            Defines.TariffType type;

            try
            {
                CoreContext.TenantManager.SetCurrentTenant(mailbox.TenantId);

                var tenantInfo = CoreContext.TenantManager.GetCurrentTenant();

                if (tenantInfo.Status == TenantStatus.RemovePending)
                    return Defines.TariffType.LongDead;

                try
                {
                    SecurityContext.AuthenticateMe(tenantInfo.OwnerId);
                }
                catch (InvalidCredentialException)
                {
                    SecurityContext.AuthenticateMe(new Guid(mailbox.UserId));
                }

                var apiHelper = new ApiHelper(httpContextScheme, log);
                type = apiHelper.GetTenantTariff(tenantOverdueDays);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("GetTenantStatus(Tenant={0}, User='{1}') Exception: {2}",
                    mailbox.TenantId, mailbox.UserId, ex.InnerException != null ? ex.InnerException.Message : ex.Message);
                type = Defines.TariffType.Active;
            }

            return type;
        }

        public static bool IsTenantQuotaEnded(this MailBoxData mailbox, long minBalance, ILog log = null)
        {
            var quotaEnded = false;
            log = log ?? new NullLog();

            try
            {
                var quotaController = new TenantQuotaController(mailbox.TenantId);
                var quota = CoreContext.TenantManager.GetTenantQuota(mailbox.TenantId);
                var usedQuota = quotaController.QuotaCurrentGet();
                quotaEnded = quota.MaxTotalSize - usedQuota < minBalance;
                log.DebugFormat("IsTenantQuotaEnded: {0} Tenant = {1}. Tenant quota = {2}Mb ({3}), used quota = {4}Mb ({5}) ", 
                    quotaEnded,
                    mailbox.TenantId,
                    MailUtil.BytesToMegabytes(quota.MaxTotalSize), quota.MaxTotalSize,
                    MailUtil.BytesToMegabytes(usedQuota), usedQuota);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("IsQuotaExhausted(Tenant={0}) Exception: {1}", mailbox.TenantId, ex.Message);
            }

            return quotaEnded;
        }

        public static bool IsCrmAvailable(this MailBoxData mailbox,
            string httpContextScheme, ILog log = null)
        {
            log = log ?? new NullLog();

            try
            {
                CoreContext.TenantManager.SetCurrentTenant(mailbox.TenantId);

                var tenantInfo = CoreContext.TenantManager.GetCurrentTenant();

                if (tenantInfo.Status == TenantStatus.RemovePending)
                    return false;

                SecurityContext.AuthenticateMe(new Guid(mailbox.UserId));

                var apiHelper = new ApiHelper(httpContextScheme, log);
                return apiHelper.IsCrmModuleAvailable();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("GetTenantStatus(Tenant={0}, User='{1}') Exception: {2}",
                    mailbox.TenantId, mailbox.UserId, ex.InnerException != null ? ex.InnerException.Message : ex.Message);
            }

            return true;
        }
    }
}
