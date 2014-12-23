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
using System.Security;
using System.Web.UI;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Tenants;
using ASC.IPSecurity;
using ASC.MessagingSystem;
using ASC.Web.Core;
using ASC.Web.Studio.UserControls.Statistics;
using AjaxPro;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using System.Web;
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

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(GetType());

            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/Management/PortalAccessSettings/js/portalAccess.js"));

            Page.RegisterStyleControl(VirtualPathUtility.ToAbsolute("~/usercontrols/management/PortalAccessSettings/css/portalAccess.less"));

            var managementPage = Page as Studio.Management;

            Settings = managementPage != null ? managementPage.TenantAccess : SettingsManager.Instance.LoadSettings<TenantAccessSettings>(TenantProvider.CurrentTenantID);

            var currentTenantQuota = CoreContext.TenantManager.GetTenantQuota(TenantProvider.CurrentTenantID);

            Enabled = (currentTenantQuota.Free || currentTenantQuota.NonProfit || currentTenantQuota.Trial) && !currentTenantQuota.Open;
        }

        [AjaxMethod]
        public object SaveSettings(bool anyone, bool registerUsers)
        {
            try
            {
                SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

                var currentTenantQuota = CoreContext.TenantManager.GetTenantQuota(TenantProvider.CurrentTenantID);

                var enabled = (currentTenantQuota.Free || currentTenantQuota.NonProfit || currentTenantQuota.Trial) && !currentTenantQuota.Open;

                if (!enabled)
                    throw new SecurityException(Resources.Resource.PortalAccessSettingsTariffException);

                var tenant = CoreContext.TenantManager.GetCurrentTenant();

                var currentSettings = SettingsManager.Instance.LoadSettings<TenantAccessSettings>(TenantProvider.CurrentTenantID);

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
                            WebItemSecurity.SetSecurity(item.ID.ToString(), item.ID != WebItemManager.CRMProductID, null);//disable crm product
                        }

                        SettingsManager.Instance.SaveSettings(new TenantAccessSettings { Anyone = true, RegisterUsersImmediately = registerUsers }, TenantProvider.CurrentTenantID);
                        SettingsManager.Instance.SaveSettings(new StudioTrustedDomainSettings { InviteUsersAsVisitors = false }, TenantProvider.CurrentTenantID);
                        SettingsManager.Instance.SaveSettings(new StudioAdminMessageSettings { Enable = true }, TenantProvider.CurrentTenantID);

                        IPRestrictionsService.Save(new List<string>(), TenantProvider.CurrentTenantID);

                        tenant.TrustedDomainsType = registerUsers ? TenantTrustedDomainsType.All : TenantTrustedDomainsType.None;
                        CoreContext.TenantManager.SaveTenant(tenant);
                    }
                    else
                    {
                        var freeQuota = CoreContext.TenantManager.GetTenantQuotas(true).FirstOrDefault(q => q.Id == Tariff.CreateDefault().QuotaId);
                        SetQuota(freeQuota);

                        SettingsManager.Instance.SaveSettings(new TenantAccessSettings { Anyone = false, RegisterUsersImmediately = false }, TenantProvider.CurrentTenantID);
                        SettingsManager.Instance.SaveSettings(new StudioTrustedDomainSettings { InviteUsersAsVisitors = false }, TenantProvider.CurrentTenantID);
                        SettingsManager.Instance.SaveSettings(new StudioAdminMessageSettings { Enable = false }, TenantProvider.CurrentTenantID);

                        foreach (var item in items)
                        {
                            WebItemSecurity.SetSecurity(item.ID.ToString(), true, null);
                        }

                        tenant.TrustedDomainsType = TenantTrustedDomainsType.None;
                        CoreContext.TenantManager.SaveTenant(tenant);
                    }

                    MessageService.Send(HttpContext.Current.Request, MessageAction.PortalAccessSettingsUpdated);
                }
                else if(anyone && currentSettings.RegisterUsersImmediately != registerUsers)
                {
                    SettingsManager.Instance.SaveSettings(new TenantAccessSettings { Anyone = true, RegisterUsersImmediately = registerUsers }, TenantProvider.CurrentTenantID);
                    tenant.TrustedDomainsType = registerUsers ? TenantTrustedDomainsType.All : TenantTrustedDomainsType.None;
                    CoreContext.TenantManager.SaveTenant(tenant);
                }

                return new
                    {
                        Status = 1,
                        Message = Resources.Resource.SuccessfullySaveSettingsMessage
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
                throw new Exception(string.Format(Resources.Resource.PortalAccessSettingsUserLimitException, quota.ActiveUsers));

            if (TenantStatisticsProvider.GetUsedSize() > quota.MaxTotalSize)
                throw new Exception(string.Format(Resources.Resource.PortalAccessSettingsDiscSpaceLimitException, quota.MaxTotalSize / 1024 / 1024));

            CoreContext.PaymentManager.SetTariff(TenantProvider.CurrentTenantID, new Tariff
            {
                QuotaId = quota.Id,
                DueDate = DateTime.MaxValue
            });
        }
    }
}