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
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI;
using ASC.MessagingSystem;
using AjaxPro;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Web.Studio.Core;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Utility;
using ASC.Web.Studio.UserControls.Statistics;
using System.Web;
using Resources;

namespace ASC.Web.Studio.UserControls.Management
{
    [ManagementControl(ManagementType.PortalSecurity, Location, SortOrder = 150)]
    [AjaxNamespace("MailDomainSettingsController")]
    public partial class MailDomainSettings : UserControl
    {
        public const string Location = "~/UserControls/Management/MailDomainSettings/MailDomainSettings.ascx";

        protected Tenant _currentTenant = null;
        protected StudioTrustedDomainSettings _studioTrustedDomainSettings;
        protected bool _enableInviteUsers;
        protected bool _tenantAccessAnyone;
        protected string HelpLink { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(GetType());

            Page.RegisterBodyScripts("~/UserControls/Management/MailDomainSettings/js/maildomainsettings.js")
                .RegisterStyle("~/UserControls/Management/MailDomainSettings/css/maildomainsettings.less");

            _currentTenant = CoreContext.TenantManager.GetCurrentTenant();
            _studioTrustedDomainSettings = StudioTrustedDomainSettings.Load();
            _enableInviteUsers = TenantStatisticsProvider.GetUsersCount() < TenantExtra.GetTenantQuota().ActiveUsers;

            if (!_enableInviteUsers)
            {
                _studioTrustedDomainSettings.InviteUsersAsVisitors = true;
            }

            var managementPage = Page as Studio.Management;
            _tenantAccessAnyone = managementPage != null ?
                                     managementPage.TenantAccess.Anyone :
                                     TenantAccessSettings.Load().Anyone;

            HelpLink = CommonLinkUtility.GetHelpLink();
        }

        private bool CheckTrustedDomain(string domain)
        {
            return !string.IsNullOrEmpty(domain) && new Regex("^[a-z0-9]([a-z0-9-.]){1,98}[a-z0-9]$").IsMatch(domain);
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public object SaveMailDomainSettings(TenantTrustedDomainsType type, List<string> domains, bool inviteUsersAsVisitors)
        {
            try
            {
                SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

                var tenant = CoreContext.TenantManager.GetCurrentTenant();

                if (type == TenantTrustedDomainsType.Custom)
                {
                    tenant.TrustedDomains.Clear();
                    foreach (var d in domains.Select(domain => (domain ?? "").Trim().ToLower()))
                    {
                        if (!CheckTrustedDomain(d))
                            return new {Status = 0, Message = Resource.ErrorNotCorrectTrustedDomain};

                        tenant.TrustedDomains.Add(d);
                    }

                    if (tenant.TrustedDomains.Count == 0)
                        type = TenantTrustedDomainsType.None;
                }

                tenant.TrustedDomainsType = type;

                new StudioTrustedDomainSettings {InviteUsersAsVisitors = inviteUsersAsVisitors}.Save();

                CoreContext.TenantManager.SaveTenant(tenant);

                MessageService.Send(HttpContext.Current.Request, MessageAction.TrustedMailDomainSettingsUpdated);

                return new {Status = 1, Message = Resource.SuccessfullySaveSettingsMessage};
            }
            catch(Exception e)
            {
                return new {Status = 0, Message = e.Message.HtmlEncode()};
            }
        }
    }
}