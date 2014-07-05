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
using System.Web;
using ASC.Core;
using ASC.Core.Common.Contracts;
using ASC.Core.Users;
using ASC.MessagingSystem;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.Utility;
using AjaxPro;
using Resources;

namespace ASC.Web.Studio.Core.Backup
{
    [AjaxNamespace("AjaxPro.Backup")]
    internal class BackupAjaxHandler
    {
        [AjaxMethod]
        public ProgressInfo StartBackup()
        {
            DemandPermissions();

            using (var service = new BackupServiceClient())
            {
                var response = service.CreateBackup(GetCurrentTenantId(), SecurityContext.CurrentAccount.ID);
                return ToProgressInfo(response);
            }
        }

        [AjaxMethod]
        public ProgressInfo GetBackupStatus()
        {
            DemandPermissions();

            using (var service = new BackupServiceClient())
            {
                var response = service.GetBackupStatus(GetCurrentTenantId().ToString());
                return ToProgressInfo(response);
            }
        }

        [AjaxMethod]
        public ProgressInfo StartRestore(string filename)
        {
            DemandPermissions();

            using (var service = new BackupServiceClient())
            {
                var response = service.RestorePortal(GetCurrentTenantId(), filename);
                return ToProgressInfo(response);
            }
        }

        [AjaxMethod]
        public ProgressInfo GetRestoreStatus()
        {
            DemandPermissions();

            using (var service = new BackupServiceClient())
            {
                var response = service.GetRestoreStatus(GetCurrentTenantId());
                return ToProgressInfo(response);
            }
        }

        [AjaxMethod]
        public ProgressInfo StartTransfer(string targetRegion, bool notifyOnlyOwner, bool transferMail)
        {
            DemandPermissions();

            using (var service = new BackupServiceClient())
            {
                var response = service.TransferPortal(
                    new TransferRequest
                        {
                            TenantId = GetCurrentTenantId(),
                            TargetRegion = targetRegion,
                            BackupMail = transferMail,
                            NotifyUsers = !notifyOnlyOwner
                        });

                return ToProgressInfo(response);
            }
        }

        [AjaxMethod]
        public ProgressInfo GetTransferStatus()
        {
            DemandPermissions();

            using (var service = new BackupServiceClient())
            {
                var response = service.GetTransferStatus(GetCurrentTenantId());
                return ToProgressInfo(response);
            }
        }

        [AjaxMethod]
        public object SendDeactivateInstructions()
        {
            DemandPermissions();

            var tenant = CoreContext.TenantManager.GetCurrentTenant();
            var owner = CoreContext.UserManager.GetUsers(tenant.OwnerId);

            var suspendUrl = CommonLinkUtility.GetConfirmationUrl(owner.Email, ConfirmType.PortalSuspend);
            var continueUrl = CommonLinkUtility.GetConfirmationUrl(owner.Email, ConfirmType.PortalContinue);
            StudioNotifyService.Instance.SendMsgPortalDeactivation(tenant, suspendUrl, continueUrl);

            MessageService.Send(HttpContext.Current.Request, MessageAction.OwnerSentPortalDeactivationInstructions, owner.DisplayUserName(false));

            var emailLink = string.Format("<a href=\"mailto:{0}\">{0}</a>", owner.Email);
            return Resource.AccountDeactivationMsg.Replace(":email", emailLink);
        }

        [AjaxMethod]
        public string SendDeleteInstructions()
        {
            DemandPermissions();

            var tenant = CoreContext.TenantManager.GetCurrentTenant();
            var owner = CoreContext.UserManager.GetUsers(tenant.OwnerId);

            StudioNotifyService.Instance.SendMsgPortalDeletion(tenant, CommonLinkUtility.GetConfirmationUrl(owner.Email, ConfirmType.PortalRemove));

            MessageService.Send(HttpContext.Current.Request, MessageAction.OwnerSentPortalDeleteInstructions, owner.DisplayUserName(false));

            var emailLink = string.Format("<a href=\"mailto:{0}\">{0}</a>", owner.Email);
            return Resource.AccountDeletionMsg.Replace(":email", emailLink);
        }

        private static void DemandPermissions()
        {
            if (!TenantExtra.GetTenantQuota().HasBackup)
                throw new InvalidOperationException(Resource.ErrorNotAllowedOption);

            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);
        }

        private static int GetCurrentTenantId()
        {
            return CoreContext.TenantManager.GetCurrentTenant().TenantId;
        }

        private static ProgressInfo ToProgressInfo(BackupResult response)
        {
            return new ProgressInfo
                {
                    IsCompleted = response.Completed,
                    Progress = response.Percent,
                    Link = response.Link
                };
        }

        public class ProgressInfo
        {
            public bool IsCompleted { get; set; }
            public int Progress { get; set; }
            public string Link { get; set; }
        }
    }
}