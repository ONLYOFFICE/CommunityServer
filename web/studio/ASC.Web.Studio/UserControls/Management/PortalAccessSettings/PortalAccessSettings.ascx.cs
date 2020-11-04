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


using AjaxPro;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Tenants;
using ASC.IPSecurity;
using ASC.MessagingSystem;
using ASC.Web.Core;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.UserControls.Statistics;
using ASC.Web.Studio.Utility;
using Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Web;
using System.Web.UI;
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Web.Studio.UserControls.Management
{
    [ManagementControl(ManagementType.PortalSecurity, Location, SortOrder = 500)]
    [AjaxNamespace("PortalAccessController")]
    public partial class PortalAccessSettings : UserControl
    {
        public const string Location = "~/UserControls/Management/PortalAccessSettings/PortalAccessSettings.ascx";

        protected TenantAccessSettings Settings;

        protected bool Enabled;

        protected string HelpLink { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(GetType());

            Page.RegisterBodyScripts("~/UserControls/Management/PortalAccessSettings/js/portalaccess.js")
                .RegisterStyle("~/UserControls/Management/PortalAccessSettings/css/portalaccess.less");

            var managementPage = Page as Studio.Management;

            Settings = managementPage != null ? managementPage.TenantAccess : TenantAccessSettings.Load();

            var currentTenantQuota = CoreContext.TenantManager.GetTenantQuota(TenantProvider.CurrentTenantID);

            Enabled = SetupInfo.IsVisibleSettings("PublicPortal") &&
                      (currentTenantQuota.Free || currentTenantQuota.NonProfit || currentTenantQuota.Trial) && !currentTenantQuota.Open;

            HelpLink = CommonLinkUtility.GetHelpLink();
        }

        [AjaxMethod]
        public object SaveSettings(bool anyone, bool registerUsers)
        {
            try
            {
                SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

                var currentTenantQuota = CoreContext.TenantManager.GetTenantQuota(TenantProvider.CurrentTenantID);

                var enabled = SetupInfo.IsVisibleSettings("PublicPortal") &&
                              (currentTenantQuota.Free || currentTenantQuota.NonProfit || currentTenantQuota.Trial) && !currentTenantQuota.Open;

                if (!enabled)
                    throw new SecurityException(Resource.PortalAccessSettingsTariffException);

                var tenant = CoreContext.TenantManager.GetCurrentTenant();

                var currentSettings = TenantAccessSettings.Load();

                //do nothing if no changes detected
                if (currentSettings.Anyone != anyone)
                {
                    var items = WebItemManager.Instance.GetItemsAll();

                    if (anyone)
                    {
                        var openQuota = CoreContext.TenantManager.GetTenantQuotas(true).FirstOrDefault(q => q.Open);
                        SetQuota(openQuota);

                        foreach (var item in items)
                        {
                            WebItemSecurity.SetSecurity(item.ID.ToString(), item.ID != WebItemManager.CRMProductID, null); //disable crm product
                        }

                        new TenantAccessSettings { Anyone = true, RegisterUsersImmediately = registerUsers }.Save();
                        new StudioTrustedDomainSettings { InviteUsersAsVisitors = false }.Save();
                        new StudioAdminMessageSettings { Enable = true }.Save();

                        IPRestrictionsService.Save(new List<string>(), TenantProvider.CurrentTenantID);

                        tenant.TrustedDomainsType = registerUsers ? TenantTrustedDomainsType.All : TenantTrustedDomainsType.None;
                        CoreContext.TenantManager.SaveTenant(tenant);
                    }
                    else
                    {
                        var freeQuota = CoreContext.TenantManager.GetTenantQuotas(true).FirstOrDefault(q => q.Id == Tariff.CreateDefault().QuotaId);
                        SetQuota(freeQuota);

                        new TenantAccessSettings { Anyone = false, RegisterUsersImmediately = false }.Save();
                        new StudioTrustedDomainSettings { InviteUsersAsVisitors = false }.Save();
                        new StudioAdminMessageSettings { Enable = false }.Save();

                        foreach (var item in items)
                        {
                            WebItemSecurity.SetSecurity(item.ID.ToString(), true, null);
                        }

                        tenant.TrustedDomainsType = TenantTrustedDomainsType.None;
                        CoreContext.TenantManager.SaveTenant(tenant);
                    }

                    MessageService.Send(HttpContext.Current.Request, MessageAction.PortalAccessSettingsUpdated);
                }
                else if (anyone && currentSettings.RegisterUsersImmediately != registerUsers)
                {
                    new TenantAccessSettings { Anyone = true, RegisterUsersImmediately = registerUsers }.Save();
                    tenant.TrustedDomainsType = registerUsers ? TenantTrustedDomainsType.All : TenantTrustedDomainsType.None;
                    CoreContext.TenantManager.SaveTenant(tenant);
                }

                return new
                    {
                        Status = 1,
                        Message = Resource.SuccessfullySaveSettingsMessage
                    };
            }
            catch (Exception e)
            {
                return new
                    {
                        Status = 0,
                        Message = e.Message.HtmlEncode()
                    };
            }
        }

        private static void SetQuota(TenantQuota quota)
        {
            if (quota == null) throw new ArgumentNullException("quota");

            if (TenantStatisticsProvider.GetUsersCount() > quota.ActiveUsers)
                throw new Exception(string.Format(Resource.PortalAccessSettingsUserLimitException, quota.ActiveUsers));

            if (TenantStatisticsProvider.GetUsedSize() > quota.MaxTotalSize)
                throw new Exception(string.Format(Resource.PortalAccessSettingsDiscSpaceLimitException, FileSizeComment.FilesSizeToString(quota.MaxTotalSize)));

            CoreContext.PaymentManager.SetTariff(TenantProvider.CurrentTenantID, new Tariff
                {
                    QuotaId = quota.Id,
                    DueDate = DateTime.MaxValue
                });
        }
    }
}