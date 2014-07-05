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
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using ASC.MessagingSystem;
using AjaxPro;
using ASC.Core;
using ASC.Core.Common.Logging;
using ASC.Core.Tenants;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.Core.SMS;
using ASC.Web.Studio.Utility;
using Resources;

namespace ASC.Web.Studio.UserControls.Management
{
    [ManagementControl(ManagementType.General, Location)]
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

            //transfer portal           
            _transferPortalSettings.Controls.Add(LoadControl(TransferPortal.Location));

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

            //main domain settings
            _mailDomainSettings.Controls.Add(LoadControl(MailDomainSettings.Location));

            //strong security password settings
            _strongPasswordSettings.Controls.Add(LoadControl(PasswordSettings.Location));

            //invitational link
            invLink.Controls.Add(LoadControl(InviteLink.Location));

            //sms settings
            if (SetupInfo.IsVisibleSettings<StudioSmsNotificationSettings>() && CoreContext.PaymentManager.GetApprovedPartner() == null)
                _smsValidationSettings.Controls.Add(LoadControl(SmsValidationSettings.Location));

            //admin message settings
            _admMessSettings.Controls.Add(LoadControl(AdminMessageSettings.Location));

            //default page settings
            _defaultPageSeettings.Controls.Add(LoadControl(DefaultPageSettings.Location));

            /*if (CoreContext.Configuration.Standalone)
            {
                _uploadHttpsSeettings.Controls.Add(LoadControl(UploadHttps.Location));
            }*/
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

                        AdminLog.PostAction("Settings: saved DNS settings with parameters dnsName={0}, alias={1}, enableDns={2}", dnsName, alias, enableDns);
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

        public static string ModifyHowToAdress(string adr)
        {
            var lang = CoreContext.TenantManager.GetCurrentTenant().Language;
            if (lang.Contains("-"))
            {
                lang = lang.Split('-')[0];
            }
            if (lang != "en") lang += "/";
            else lang = string.Empty;
            return string.Format("{0}/{1}{2}", "http://www.onlyoffice.com", lang, adr ?? string.Empty);
        }
    }
}