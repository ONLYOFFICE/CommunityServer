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

            var emailLink = string.Format("<a href=\"mailto:{0}\">{0}</a>", owner.Email.HtmlEncode());
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

            var emailLink = string.Format("<a href=\"mailto:{0}\">{0}</a>", owner.Email.HtmlEncode());
            return Resource.AccountDeletionMsg.Replace(":email", emailLink);
        }
    }
}