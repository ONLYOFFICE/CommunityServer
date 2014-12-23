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
using System.Web;
using System.Web.UI;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.MessagingSystem;
using ASC.Web.Studio.Utility;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Notify;

namespace ASC.Web.Studio.UserControls.Management
{
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
                    _buttonTitle = Resources.Resource.ReactivatePortalButton;
                    _title = Resources.Resource.ConfirmReactivatePortalTitle;
                    break;
                case ConfirmType.PortalRemove:
                    _buttonTitle = Resources.Resource.DeletePortalButton;
                    _title = Resources.Resource.ConfirmDeletePortalTitle;
                    break;

                case ConfirmType.PortalSuspend:
                    _buttonTitle = Resources.Resource.DeactivatePortalButton;
                    _title = Resources.Resource.ConfirmDeactivatePortalTitle;
                    break;

                case ConfirmType.DnsChange:
                    _buttonTitle = Resources.Resource.SaveButton;
                    var portalAddress = GenerateLink(GetTenantBasePath(alias));
                    if (!string.IsNullOrEmpty(dns))
                    {
                        portalAddress += string.Format(" ({0})", GenerateLink(dns));
                    }
                    _title = string.Format(Resources.Resource.ConfirmDnsUpdateTitle, portalAddress);
                    break;
            }

            if (IsPostBack)
            {
                _successMessage = "";
                var curTenant = CoreContext.TenantManager.GetCurrentTenant();
                var updatedFlag = false;

                var messageAction = MessageAction.None;
                switch (_type)
                {
                    case ConfirmType.PortalContinue:
                        curTenant.SetStatus(TenantStatus.Active);
                        _successMessage = string.Format(Resources.Resource.ReactivatePortalSuccessMessage, "<br/>", "<a href=\"{0}\">", "</a>");
                        break;

                    case ConfirmType.PortalRemove:
                        CoreContext.TenantManager.RemoveTenant(curTenant.TenantId);
                        _successMessage = string.Format(Resources.Resource.DeletePortalSuccessMessage, "<br/>", "<a href=\"{0}\">", "</a>");
                        messageAction = MessageAction.PortalDeleted;
                        break;

                    case ConfirmType.PortalSuspend:
                        curTenant.SetStatus(TenantStatus.Suspended);
                        _successMessage = string.Format(Resources.Resource.DeactivatePortalSuccessMessage, "<br/>", "<a href=\"{0}\">", "</a>");
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
                        _successMessage = string.Format(Resources.Resource.DeactivatePortalSuccessMessage, "<br/>", "<a href=\"{0}\">", "</a>");
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

                    if (_type != ConfirmType.PortalRemove)
                    {
                        CoreContext.TenantManager.SaveTenant(curTenant);   
                    }
                    if (messageAction != MessageAction.None)
                    {
                        MessageService.Send(HttpContext.Current.Request, messageAction);
                    }
                }
                finally
                {
                    if (authed) SecurityContext.Logout();
                }
                var redirectLink = CommonLinkUtility.GetDefault();

                if (_type == ConfirmType.PortalRemove)
                {
                    var currentUser = CoreContext.UserManager.GetUsers(CoreContext.TenantManager.GetCurrentTenant().OwnerId);
                    redirectLink = SetupInfo.TeamlabSiteRedirect + "/remove-portal-feedback-form.aspx#" +
                                   Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("{\"firstname\":\"" + currentUser.FirstName +
                                                                                             "\",\"lastname\":\"" + currentUser.LastName +
                                                                                             "\",\"alias\":\"" + alias +
                                                                                             "\",\"email\":\"" + currentUser.Email + "\"}"));
                    StudioNotifyService.Instance.SendMsgPortalDeletionSuccess(curTenant, redirectLink);
                }
                _successMessage = string.Format(_successMessage, redirectLink);
                

                _messageHolder.Visible = true;
                _confirmContentHolder.Visible = false;
            }
            else
            {
                _messageHolder.Visible = false;
                _confirmContentHolder.Visible = true;
            }
        }

        private ConfirmType GetConfirmType()
        {
            return typeof(ConfirmType).TryParseEnum(Request["type"] ?? "", ConfirmType.PortalContinue);
        }

        #region Tenant Base Path

        private static string GetTenantBasePath(string alias)
        {
            return String.Format("http://{0}.{1}", alias, SetupInfo.BaseDomain);
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