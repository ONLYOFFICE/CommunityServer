/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


using ASC.Api.Attributes;
using ASC.Api.Impl;
using ASC.Api.Interfaces;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Common.Contracts;
using ASC.Core.Common.Notify.Push;
using ASC.Core.Notify.Jabber;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Web.Core.Mobile;
using ASC.Web.Studio.Core.Backup;
using ASC.Web.Studio.Utility;
using System;
using System.Linq;

namespace ASC.Api.Portal
{
    ///<summary>
    /// Portal info access
    ///</summary>
    public class PortalApi : IApiEntryPoint
    {
        private readonly IMobileAppInstallRegistrator mobileAppRegistrator;
        private readonly BackupAjaxHandler backupHandler = new BackupAjaxHandler();


        ///<summary>
        /// Api name entry
        ///</summary>
        public string Name
        {
            get { return "portal"; }
        }

        public PortalApi(ApiContext context)
        {
            mobileAppRegistrator = new CachedMobileAppInstallRegistrator(new MobileAppInstallRegistrator());
        }

        ///<summary>
        ///Returns the current portal
        ///</summary>
        ///<short>
        ///Current portal
        ///</short>
        /// <category>Portal info</category>
        ///<returns>Portal</returns>
        [Read("")]
        public Tenant Get()
        {
            return CoreContext.TenantManager.GetCurrentTenant();
        }

        ///<summary>
        ///Returns the user with specified userID from the current portal
        ///</summary>
        ///<short>
        ///User with specified userID
        ///</short>
        /// <category>Portal info</category>
        ///<returns>User</returns>
        [Read("users/{userID}")]
        public UserInfo GetUser(Guid userID)
        {
            return CoreContext.UserManager.GetUsers(userID);
        }

        ///<summary>
        ///Returns the used space of the current portal
        ///</summary>
        ///<short>
        ///Used space of the current portal
        ///</short>
        /// <category>Portal info</category>
        ///<returns>Used space</returns>
        [Read("usedspace")]
        public double GetUsedSpace()
        {
            return Math.Round(
                CoreContext.TenantManager.FindTenantQuotaRows(new TenantQuotaRowQuery(CoreContext.TenantManager.GetCurrentTenant().TenantId))
                           .Where(q => !string.IsNullOrEmpty(q.Tag) && new Guid(q.Tag) != Guid.Empty)
                           .Sum(q => q.Counter) / 1024f / 1024f / 1024f, 2);
        }

        ///<summary>
        ///Returns the users count of the current portal
        ///</summary>
        ///<short>
        ///Users count of the current portal
        ///</short>
        /// <category>Portal info</category>
        ///<returns>Users count</returns>
        [Read("userscount")]
        public long GetUsersCount()
        {
            return CoreContext.UserManager.GetUserNames(EmployeeStatus.Active).Count();
        }

        ///<summary>
        ///Returns the current tariff of the current portal
        ///</summary>
        ///<short>
        ///Tariff of the current portal
        ///</short>
        /// <category>Portal info</category>
        ///<returns>Tariff</returns>
        [Read("tariff")]
        public Tariff GetTariff()
        {
            return CoreContext.PaymentManager.GetTariff(CoreContext.TenantManager.GetCurrentTenant().TenantId);
        }

        ///<summary>
        ///Returns the current quota of the current portal
        ///</summary>
        ///<short>
        ///Quota of the current portal
        ///</short>
        /// <category>Portal info</category>
        ///<returns>Quota</returns>
        [Read("quota")]
        public TenantQuota GetQuota()
        {
            return CoreContext.TenantManager.GetTenantQuota(CoreContext.TenantManager.GetCurrentTenant().TenantId);
        }

        ///<summary>
        ///Returns the recommended quota of the current portal
        ///</summary>
        ///<short>
        ///Quota of the current portal
        ///</short>
        /// <category>Portal info</category>
        ///<returns>Quota</returns>
        [Read("quota/right")]
        public TenantQuota GetRightQuota()
        {
            var usedSpace = GetUsedSpace();
            var needUsersCount = GetUsersCount();

            return CoreContext.TenantManager.GetTenantQuotas().OrderBy(r => r.Price)
                                    .FirstOrDefault(quota =>
                                                    quota.ActiveUsers > needUsersCount
                                                    && quota.MaxTotalSize > usedSpace
                                                    && quota.DocsEdition
                                                    && !quota.Year);
        }

        ///<summary>
        ///Returns path
        ///</summary>
        ///<short>
        ///path
        ///</short>
        ///<category>Portal info</category>
        ///<returns>path</returns>
        ///<visible>false</visible>
        [Read("path")]
        public string GetFullAbsolutePath(string virtualPath)
        {
            return CommonLinkUtility.GetFullAbsolutePath(virtualPath);
        }

        ///<visible>false</visible>
        [Read("talk/unreadmessages")]
        public int GetMessageCount()
        {
            try
            {
                var username = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).UserName;
                return new JabberServiceClient().GetNewMessagesCount(TenantProvider.CurrentTenantID, username);
            }
            catch { }
            return 0;
        }

        [Create("mobile/registration")]
        public void RegisterMobileAppInstall(MobileAppType type)
        {
            var currentUser = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
            mobileAppRegistrator.RegisterInstall(currentUser.Email, type);
        }


        [Create("createbackup")]
        public BackupProgress CreateBackup(bool withMail = true)
        {
            return backupHandler.StartBackup(BackupStorageType.DataStore, null, withMail);
        }

        [Create("restorebackup")]
        public BackupProgress RestoreBackup(string backupfile)
        {
            if (!CoreContext.Configuration.Standalone)
            {
                throw new NotSupportedException("Restore backup not available.");
            }

            return backupHandler.StartRestore(
                null,
                BackupStorageType.Local,
                new BackupAjaxHandler.StorageParams
                {
                    FilePath = backupfile,
                },
                false);
        }

        [Read("backupprogress")]
        public BackupProgress BackupProgress()
        {
            return backupHandler.GetBackupProgress();
        }

        [Read("restoreprogress")]
        public BackupProgress RestoreProgress()
        {
            if (!CoreContext.Configuration.Standalone)
            {
                throw new NotSupportedException("Restore backup not available.");
            }

            return backupHandler.GetRestoreProgress();
        }
    }
}