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