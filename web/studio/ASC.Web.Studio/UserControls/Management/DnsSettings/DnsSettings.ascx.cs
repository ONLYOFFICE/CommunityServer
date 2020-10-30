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
using System.Text;
using System.Web.UI;
using System.Web;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.MessagingSystem;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.Utility;
using AjaxPro;
using Resources;

namespace ASC.Web.Studio.UserControls.Management.DnsSettings
{
    [AjaxNamespace("DnsSettingsAjax")]
    public partial class DnsSettings : UserControl
    {
        public const string Location = "~/UserControls/Management/DnsSettings/DnsSettings.ascx";

        protected string HelpLink { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(GetType());

            Page.RegisterBodyScripts("~/UserControls/Management/DnsSettings/dnssettings.js");

            HelpLink = CommonLinkUtility.GetHelpLink();
        }

        protected bool EnableDomain
        {
            get { return TenantExtra.GetTenantQuota().HasDomain; }
        }

        protected static bool EnableDnsChange
        {
            get { return !string.IsNullOrEmpty(CoreContext.TenantManager.GetCurrentTenant().MappedDomain); }
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse SaveDnsSettings(string dnsName, bool enableDns)
        {
            var resp = new AjaxResponse {rs1 = "1"};
            try
            {
                if (!EnableDomain || !SetupInfo.IsVisibleSettings<DnsSettings>()) throw new Exception(Resource.ErrorNotAllowedOption);

                SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

                var tenant = CoreContext.TenantManager.GetCurrentTenant();

                if (!enableDns || string.IsNullOrEmpty(dnsName))
                {
                    dnsName = null;
                }
                if (dnsName == null || CheckCustomDomain(dnsName))
                {
                    if (CoreContext.Configuration.Standalone)
                    {
                        tenant.MappedDomain = dnsName;
                        CoreContext.TenantManager.SaveTenant(tenant);
                        return resp;
                    }

                    if (tenant.MappedDomain != dnsName)
                    {
                        var portalAddress = string.Format("http://{0}.{1}", tenant.TenantAlias ?? String.Empty, CoreContext.Configuration.BaseDomain);

                        var u = CoreContext.UserManager.GetUsers(tenant.OwnerId);
                        StudioNotifyService.Instance.SendMsgDnsChange(tenant, GenerateDnsChangeConfirmUrl(u.Email, dnsName, tenant.TenantAlias, ConfirmType.DnsChange), portalAddress, dnsName);
                        resp.rs2 = string.Format(Resource.DnsChangeMsg, string.Format("<a href=\"mailto:{0}\">{0}</a>", u.Email.HtmlEncode()));

                        MessageService.Send(HttpContext.Current.Request, MessageAction.DnsSettingsUpdated);
                    }
                }
                else
                {
                    resp.rs1 = "0";
                    resp.rs2 = Resource.ErrorNotCorrectTrustedDomain;
                }
            }
            catch (Exception e)
            {
                resp.rs1 = "0";
                resp.rs2 = e.Message.HtmlEncode();
            }
            return resp;
        }

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
                catch (TenantTooShortException ex)
                {
                    var minLength = ex.MinLength;
                    var maxLength = ex.MaxLength;
                    if (minLength > 0 && maxLength > 0)
                    {
                        throw new TenantTooShortException(string.Format(Resource.ErrorTenantTooShortFormat, minLength, maxLength));
                    }

                    throw new TenantTooShortException(Resource.ErrorTenantTooShort);
                }
                catch (TenantIncorrectCharsException)
                {
                }
                return true;
            }
            return false;
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
                return String.IsNullOrEmpty(CoreContext.Configuration.BaseDomain)
                           ? String.Empty
                           : String.Format(".{0}", CoreContext.Configuration.BaseDomain);
            }
        }
    }
}