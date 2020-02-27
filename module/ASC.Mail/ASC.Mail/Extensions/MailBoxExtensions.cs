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

                var apiHelper = new ApiHelper(httpContextScheme);
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

                var apiHelper = new ApiHelper(httpContextScheme);
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
