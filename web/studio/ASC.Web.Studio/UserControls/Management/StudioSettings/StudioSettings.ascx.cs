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


using System;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using ASC.MessagingSystem;
using AjaxPro;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.Core.SMS;
using ASC.Web.Studio.Utility;
using Resources;

namespace ASC.Web.Studio.UserControls.Management
{
    [ManagementControl(ManagementType.Customization, Location)]
    [AjaxNamespace("StudioSettingsAjax")]
    public partial class StudioSettings : UserControl
    {
        public const string Location = "~/UserControls/Management/StudioSettings/StudioSettings.ascx";

        public Guid ProductID { get; set; }

        protected bool EnableDomain
        {
            get { return TenantExtra.GetTenantQuota().HasDomain; }
        }

        protected static bool EnableDnsChange
        {
            get { return !string.IsNullOrEmpty(CoreContext.TenantManager.GetCurrentTenant().MappedDomain); }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(GetType());
            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/Management/StudioSettings/studiosettings.js"));

            //timezone & language
            _timelngHolder.Controls.Add(LoadControl(TimeAndLanguage.Location));

            if (SetupInfo.IsVisibleSettings<PromoCode>() &&
                TenantExtra.GetCurrentTariff().State == ASC.Core.Billing.TariffState.Trial &&
                string.IsNullOrEmpty(CoreContext.TenantManager.GetCurrentTenant().PartnerId))
            {
                promoCodeSettings.Controls.Add(LoadControl(PromoCode.Location));
            }

            //Portal version
            if (SetupInfo.IsVisibleSettings<VersionSettings.VersionSettings>() && 1 < CoreContext.TenantManager.GetTenantVersions().Count())
                _portalVersionSettings.Controls.Add(LoadControl(VersionSettings.VersionSettings.Location));

            //greeting settings
            _greetingSettings.Controls.Add(LoadControl(GreetingSettings.Location));
        }

        #region Check custom domain name

        /// <summary>
        /// Custom domain name shouldn't ends with tenant base domain name.
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        private static bool CheckCustomDomain(string domain)
        {
            if (string.IsNullOrEmpty(domain))
            {
                return false;
            }
            if (!string.IsNullOrEmpty(TenantBaseDomain) &&
                (domain.EndsWith(TenantBaseDomain, StringComparison.InvariantCultureIgnoreCase) || domain.Equals(TenantBaseDomain.TrimStart('.'), StringComparison.InvariantCultureIgnoreCase)))
            {
                return false;
            }
            Uri test;
            if (Uri.TryCreate(domain.Contains(Uri.SchemeDelimiter) ? domain : Uri.UriSchemeHttp + Uri.SchemeDelimiter + domain, UriKind.Absolute, out test))
            {
                try
                {
                    CoreContext.TenantManager.CheckTenantAddress(test.Host);
                }
                catch(TenantIncorrectCharsException)
                {
                }
                return true;
            }
            return false;
        }

        #endregion

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse SaveDnsSettings(string dnsName, string alias, bool enableDns)
        {
            var resp = new AjaxResponse {rs1 = "1"};
            try
            {
                if (!EnableDomain) throw new Exception(Resource.ErrorNotAllowedOption);

                SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

                var tenant = CoreContext.TenantManager.GetCurrentTenant();

                if (!enableDns || string.IsNullOrEmpty(dnsName))
                {
                    dnsName = null;
                }
                if (dnsName == null || CheckCustomDomain(dnsName))
                {
                    if (string.IsNullOrEmpty(alias))
                    {
                        alias = tenant.TenantAlias;
                    }

                    if (CoreContext.Configuration.Standalone)
                    {
                        tenant.MappedDomain = dnsName;
                        CoreContext.TenantManager.SaveTenant(tenant);
                        return resp;
                    }
                    if (!tenant.TenantAlias.Equals(alias))
                    {
                        CoreContext.TenantManager.CheckTenantAddress(alias);
                    }

                    if ((!string.IsNullOrEmpty(alias) && tenant.TenantAlias != alias) || tenant.MappedDomain != dnsName)
                    {
                        var portalAddress = string.Format("http://{0}.{1}", alias ?? String.Empty, SetupInfo.BaseDomain);

                        var u = CoreContext.UserManager.GetUsers(tenant.OwnerId);
                        StudioNotifyService.Instance.SendMsgDnsChange(tenant, GenerateDnsChangeConfirmUrl(u.Email, dnsName, alias, ConfirmType.DnsChange), portalAddress, dnsName);
                        resp.rs2 = string.Format(Resource.DnsChangeMsg, string.Format("<a href='mailto:{0}'>{0}</a>", u.Email));

                        MessageService.Send(HttpContext.Current.Request, MessageAction.DnsSettingsUpdated);
                    }
                }
                else
                {
                    resp.rs1 = "0";
                    resp.rs2 = Resource.ErrorNotCorrectTrustedDomain;
                }
            }
            catch(Exception e)
            {
                resp.rs1 = "0";
                resp.rs2 = e.Message.HtmlEncode();
            }
            return resp;
        }

        private static string GenerateDnsChangeConfirmUrl(string email, string dnsName, string tenantAlias, ConfirmType confirmType)
        {
            var postfix = string.Join(string.Empty, new[] {dnsName, tenantAlias});

            var sb = new StringBuilder();
            sb.Append(CommonLinkUtility.GetConfirmationUrl(email, confirmType, postfix));
            if (!string.IsNullOrEmpty(dnsName))
            {
                sb.AppendFormat("&dns={0}", dnsName);
            }
            if (!string.IsNullOrEmpty(tenantAlias))
            {
                sb.AppendFormat("&alias={0}", tenantAlias);
            }
            return sb.ToString();
        }

        protected static string TenantBaseDomain
        {
            get
            {
                return String.IsNullOrEmpty(SetupInfo.BaseDomain)
                           ? String.Empty
                           : String.Format(".{0}", SetupInfo.BaseDomain);
            }
        }
    }
}