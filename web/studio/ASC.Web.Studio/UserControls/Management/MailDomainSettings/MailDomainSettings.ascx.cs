/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(GetType());

            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/management/maildomainsettings/js/maildomainsettings.js"));
            Page.RegisterStyleControl(VirtualPathUtility.ToAbsolute("~/usercontrols/management/maildomainsettings/css/maildomainsettings.less"));

            _currentTenant = CoreContext.TenantManager.GetCurrentTenant();
            _studioTrustedDomainSettings = SettingsManager.Instance.LoadSettings<StudioTrustedDomainSettings>(TenantProvider.CurrentTenantID);
            _enableInviteUsers = TenantStatisticsProvider.GetUsersCount() < TenantExtra.GetTenantQuota().ActiveUsers;

            if (!_enableInviteUsers)
            {
                _studioTrustedDomainSettings.InviteUsersAsVisitors = true;
            }

            var managementPage = Page as Studio.Management;
            _tenantAccessAnyone = managementPage != null ?
                                     managementPage.TenantAccess.Anyone :
                                     SettingsManager.Instance.LoadSettings<TenantAccessSettings>(TenantProvider.CurrentTenantID).Anyone;
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

                var domainSettingsObj = new StudioTrustedDomainSettings {InviteUsersAsVisitors = inviteUsersAsVisitors};
                SettingsManager.Instance.SaveSettings(domainSettingsObj, TenantProvider.CurrentTenantID);

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