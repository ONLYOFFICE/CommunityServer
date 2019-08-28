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