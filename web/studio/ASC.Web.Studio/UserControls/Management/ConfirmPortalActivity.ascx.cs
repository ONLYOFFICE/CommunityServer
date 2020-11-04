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
using System.ServiceModel.Security;
using System.Web;
using System.Web.UI;

using AjaxPro;
using Amazon.SecurityToken.Model;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.MessagingSystem;
using ASC.Security.Cryptography;
using ASC.Web.Core.Helpers;
using ASC.Web.Core.Security;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.Utility;

using Newtonsoft.Json;
using Resources;

namespace ASC.Web.Studio.UserControls.Management
{
    [AjaxNamespace("AjaxPro.ConfirmPortalActivity")]
    public partial class ConfirmPortalActivity : UserControl
    {
        protected ConfirmType _type;

        protected string _buttonTitle;
        protected string _successMessage;
        protected string _title;

        public static string Location
        {
            get { return "~/UserControls/Management/ConfirmPortalActivity.ascx"; }
        }

        private const string httpPrefix = "http://";

        protected void Page_Load(object sender, EventArgs e)
        {
            var dns = Request["dns"];
            var alias = Request["alias"];

            _type = GetConfirmType();
            switch (_type)
            {
                case ConfirmType.PortalContinue:
                    if (TenantExtra.Enterprise)
                    {
                        var countPortals = TenantExtra.GetTenantQuota().CountPortals;
                        var activePortals = CoreContext.TenantManager.GetTenants().Count();
                        if (countPortals <= activePortals)
                        {
                            _successMessage = UserControlsCommonResource.TariffPortalLimitHeaer;
                            _confirmContentHolder.Visible = false;
                            return;
                        }
                    }

                    _buttonTitle = Resource.ReactivatePortalButton;
                    _title = Resource.ConfirmReactivatePortalTitle;
                    break;
                case ConfirmType.PortalRemove:
                    _buttonTitle = Resource.DeletePortalButton;
                    _title = Resource.ConfirmDeletePortalTitle;
                    AjaxPro.Utility.RegisterTypeForAjax(GetType());
                    break;

                case ConfirmType.PortalSuspend:
                    _buttonTitle = Resource.DeactivatePortalButton;
                    _title = Resource.ConfirmDeactivatePortalTitle;
                    break;

                case ConfirmType.DnsChange:
                    _buttonTitle = Resource.SaveButton;
                    var portalAddress = GenerateLink(GetTenantBasePath(alias));
                    if (!string.IsNullOrEmpty(dns))
                    {
                        portalAddress += string.Format(" ({0})", GenerateLink(dns));
                    }
                    _title = string.Format(Resource.ConfirmDnsUpdateTitle, portalAddress);
                    break;
            }


            if (IsPostBack && _type != ConfirmType.PortalRemove)
            {
                _successMessage = "";

                var curTenant = CoreContext.TenantManager.GetCurrentTenant();
                var updatedFlag = false;

                var messageAction = MessageAction.None;
                switch (_type)
                {
                    case ConfirmType.PortalContinue:
                        curTenant.SetStatus(TenantStatus.Active);
                        _successMessage = string.Format(Resource.ReactivatePortalSuccessMessage, "<br/>", "<a href=\"{0}\">", "</a>");
                        break;

                    case ConfirmType.PortalSuspend:
                        curTenant.SetStatus(TenantStatus.Suspended);
                        _successMessage = string.Format(Resource.DeactivatePortalSuccessMessage, "<br/>", "<a href=\"{0}\">", "</a>");
                        messageAction = MessageAction.PortalDeactivated;
                        break;

                    case ConfirmType.DnsChange:
                        if (!string.IsNullOrEmpty(dns))
                        {
                            dns = dns.Trim().TrimEnd('/', '\\');
                        }
                        if (curTenant.MappedDomain != dns)
                        {
                            updatedFlag = true;
                        }
                        curTenant.MappedDomain = dns;
                        if (!string.IsNullOrEmpty(alias))
                        {
                            if (curTenant.TenantAlias != alias)
                            {
                                updatedFlag = true;
                            }
                            curTenant.TenantAlias = alias;
                        }
                        _successMessage = string.Format(Resource.DeactivatePortalSuccessMessage, "<br/>", "<a href=\"{0}\">", "</a>");
                        break;
                }

                bool authed = false;
                try
                {
                    if (!SecurityContext.IsAuthenticated)
                    {
                        SecurityContext.AuthenticateMe(ASC.Core.Configuration.Constants.CoreSystem);
                        authed = true;
                    }

                    #region Alias or dns update

                    if (IsChangeDnsMode)
                    {
                        if (updatedFlag)
                        {
                            CoreContext.TenantManager.SaveTenant(curTenant);
                        }
                        var redirectUrl = dns;
                        if (string.IsNullOrEmpty(redirectUrl))
                        {
                            redirectUrl = GetTenantBasePath(curTenant);
                        }
                        Response.Redirect(AddHttpToUrl(redirectUrl));
                        return;
                    }

                    #endregion


                    CoreContext.TenantManager.SaveTenant(curTenant);   
                    if (messageAction != MessageAction.None)
                    {
                        MessageService.Send(HttpContext.Current.Request, messageAction);
                    }
                }
                catch(Exception err)
                {
                    _successMessage = err.Message;
                    LogManager.GetLogger("ASC.Web.Confirm").Error(err);
                }
                finally
                {
                    if (authed) SecurityContext.Logout();
                }

                var redirectLink = CommonLinkUtility.GetDefault();
                _successMessage = string.Format(_successMessage, redirectLink);

                _messageHolder.Visible = true;
                _confirmContentHolder.Visible = false;
            }
            else
            {
                _messageHolder.Visible = false;
                _confirmContentHolder.Visible = true;

                if (_type == ConfirmType.PortalRemove)
                    _messageHolderPortalRemove.Visible = true;
                else
                    _messageHolderPortalRemove.Visible = false;
            }
        }

        [SecurityPassthrough]
        [AjaxMethod]
        public string PortalRemove(string email, string key)
        {
            email = (email ?? "").Trim();
            if (!string.IsNullOrEmpty(email) && !email.TestEmailRegex())
            {
                throw new ArgumentException(Resource.ErrorNotCorrectEmail);
            }

            var checkKeyResult = EmailValidationKeyProvider.ValidateEmailKey(email + ConfirmType.PortalRemove, key, SetupInfo.ValidEmailKeyInterval);

            if (checkKeyResult == EmailValidationKeyProvider.ValidationResult.Expired)
            {
                throw new ExpiredTokenException(Resource.ErrorExpiredActivationLink);
            }

            if (checkKeyResult == EmailValidationKeyProvider.ValidationResult.Invalid)
            {
                throw new SecurityAccessDeniedException(Resource.ErrorConfirmURLError);
            }

            var tenant = CoreContext.TenantManager.GetCurrentTenant();
            CoreContext.TenantManager.RemoveTenant(tenant.TenantId);

            if (!String.IsNullOrEmpty(ApiSystemHelper.ApiCacheUrl))
            {
                ApiSystemHelper.RemoveTenantFromCache(tenant.TenantAlias);
            }

            var owner = CoreContext.UserManager.GetUsers(tenant.OwnerId);
            var redirectLink = SetupInfo.TeamlabSiteRedirect + "/remove-portal-feedback-form.aspx#" +
                        Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("{\"firstname\":\"" + owner.FirstName +
                                                                                    "\",\"lastname\":\"" + owner.LastName +
                                                                                    "\",\"alias\":\"" + tenant.TenantAlias +
                                                                                    "\",\"email\":\"" + owner.Email + "\"}"));

            var authed = false;
            try
            {
                if (!SecurityContext.IsAuthenticated)
                {
                    SecurityContext.AuthenticateMe(ASC.Core.Configuration.Constants.CoreSystem);
                    authed = true;
                }

                MessageService.Send(HttpContext.Current.Request, MessageAction.PortalDeleted);

            }
            finally
            {
                if (authed) SecurityContext.Logout();
            }

            _successMessage = string.Format(Resource.DeletePortalSuccessMessage, "<br/>", "<a href=\"{0}\">", "</a>");
            _successMessage = string.Format(_successMessage, redirectLink);

            StudioNotifyService.Instance.SendMsgPortalDeletionSuccess(owner, redirectLink);

            return JsonConvert.SerializeObject(new
                {
                    successMessage = _successMessage,
                    redirectLink
                });
        }


        private ConfirmType GetConfirmType()
        {
            return typeof(ConfirmType).TryParseEnum(Request["type"] ?? "", ConfirmType.PortalContinue);
        }

        #region Tenant Base Path

        private static string GetTenantBasePath(string alias)
        {
            return String.Format("http://{0}.{1}", alias, CoreContext.Configuration.BaseDomain);
        }

        private static string GetTenantBasePath(Tenant tenant)
        {
            return GetTenantBasePath(tenant.TenantAlias);
        }

        private static string GenerateLink(string url)
        {
            url = AddHttpToUrl(url);
            return string.Format("<a href='{0}' class='link header-base middle bold' target='_blank'>{1}</a>", url, url.Substring(httpPrefix.Length));
        }

        private static string AddHttpToUrl(string url)
        {
            url = url ?? string.Empty;
            if (!url.StartsWith(httpPrefix))
            {
                url = httpPrefix + url;
            }
            return url;
        }

        #endregion

        protected bool IsChangeDnsMode
        {
            get { return GetConfirmType() == ConfirmType.DnsChange; }
        }
    }
}