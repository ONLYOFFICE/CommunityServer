/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using System.Linq;
using System.Web;
using System.Web.UI;
using ASC.Core;
using ASC.Core.Users;
using ASC.MessagingSystem;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.Utility;
using AjaxPro;
using Resources;

namespace ASC.Web.Studio.UserControls.Management
{
    [ManagementControl(ManagementType.DeletionPortal, Location)]
    [AjaxNamespace("AjaxPro.DeactivatePortal")]
    public partial class DeactivatePortal : UserControl
    {
        protected bool ShowAutoRenew;
        
        public const string Location = "~/UserControls/Management/DeactivatePortal/DeactivatePortal.ascx";

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(GetType(), Page);
            Page.RegisterBodyScripts("~/UserControls/Management/DeactivatePortal/js/deactivateportal.js");

            ShowAutoRenew = !CoreContext.Configuration.Standalone &&
                            CoreContext.PaymentManager.GetTariffPayments(TenantProvider.CurrentTenantID).Any() &&
                            !TenantExtra.GetTenantQuota().Trial;
        }

        [AjaxMethod]
        public object SendDeactivateInstructions()
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            var tenant = CoreContext.TenantManager.GetCurrentTenant();
            var owner = CoreContext.UserManager.GetUsers(tenant.OwnerId);

            var suspendUrl = CommonLinkUtility.GetConfirmationUrl(owner.Email, ConfirmType.PortalSuspend);
            var continueUrl = CommonLinkUtility.GetConfirmationUrl(owner.Email, ConfirmType.PortalContinue);
            StudioNotifyService.Instance.SendMsgPortalDeactivation(tenant, suspendUrl, continueUrl);

            MessageService.Send(HttpContext.Current.Request, MessageAction.OwnerSentPortalDeactivationInstructions, MessageTarget.Create(owner.ID), owner.DisplayUserName(false));

            var emailLink = string.Format("<a href=\"mailto:{0}\">{0}</a>", owner.Email);
            return Resource.AccountDeactivationMsg.Replace(":email", emailLink);
        }

        [AjaxMethod]
        public string SendDeleteInstructions()
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            var tenant = CoreContext.TenantManager.GetCurrentTenant();
            var owner = CoreContext.UserManager.GetUsers(tenant.OwnerId);

            var showAutoRenewText = !CoreContext.Configuration.Standalone &&
                            CoreContext.PaymentManager.GetTariffPayments(TenantProvider.CurrentTenantID).Any() &&
                            !TenantExtra.GetTenantQuota().Trial;

            StudioNotifyService.Instance.SendMsgPortalDeletion(tenant, CommonLinkUtility.GetConfirmationUrl(owner.Email, ConfirmType.PortalRemove), showAutoRenewText);

            MessageService.Send(HttpContext.Current.Request, MessageAction.OwnerSentPortalDeleteInstructions, MessageTarget.Create(owner.ID), owner.DisplayUserName(false));

            var emailLink = string.Format("<a href=\"mailto:{0}\">{0}</a>", owner.Email);
            return Resource.AccountDeletionMsg.Replace(":email", emailLink);
        }
    }
}