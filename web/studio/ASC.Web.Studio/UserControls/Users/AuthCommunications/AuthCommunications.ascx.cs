/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
using System.Web;
using System.Web.UI;
using ASC.MessagingSystem;
using ASC.Web.Studio.UserControls.Statistics;
using AjaxPro;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Web.Core.Security;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.Utility;
using System.Net.Mail;
using ASC.Core.Users;
using Resources;

namespace ASC.Web.Studio.UserControls
{
    [AjaxNamespace("AuthCommunicationsController")]
    public partial class AuthCommunications : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Users/AuthCommunications/AuthCommunications.ascx"; }
        }

        protected bool ShowSeparator { get; private set; }

        public bool DisableJoin;

        private bool EnabledJoin
        {
            get
            {
                if (DisableJoin) return false;
                var t = CoreContext.TenantManager.GetCurrentTenant();
                return (t.TrustedDomainsType == TenantTrustedDomainsType.Custom && t.TrustedDomains.Count > 0) ||
                       t.TrustedDomainsType == TenantTrustedDomainsType.All;
            }
        }

        private static bool EnableAdmMess
        {
            get
            {
                var setting = SettingsManager.Instance.LoadSettings<StudioAdminMessageSettings>(TenantProvider.CurrentTenantID);
                return setting.Enable || TenantStatisticsProvider.IsNotPaid();
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            _joinBlock.Visible = EnabledJoin;

            _sendAdmin.Visible = EnableAdmMess;

            if (EnabledJoin || EnableAdmMess)
                AjaxPro.Utility.RegisterTypeForAjax(GetType());

            ShowSeparator = _joinBlock.Visible && _sendAdmin.Visible;

            Page.RegisterBodyScripts("~/usercontrols/users/AuthCommunications/js/authcommunications.js");
        }

        [SecurityPassthrough]
        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public object SendAdmMail(string email, string message)
        {
            try
            {
                if (!EnableAdmMess)
                {
                    throw new Exception("The method is turned off");
                }
                if (email == null || email.Trim().Length == 0)
                {
                    throw new ArgumentException("Email is empty.", "email");
                }
                if (message == null || message.Trim().Length == 0)
                {
                    throw new ArgumentException("Message is empty.", "message");
                }

                var key = HttpContext.Current.Request.UserHostAddress + "adminmessages";
                var count = Convert.ToInt32(HttpContext.Current.Cache[key]);
                if (10 < count)
                {
                    throw new ArgumentOutOfRangeException("Messages count", "Rate limit exceeded.");
                }
                HttpContext.Current.Cache.Insert(key, ++count, null, System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(2));

                StudioNotifyService.Instance.SendMsgToAdminFromNotAuthUser(email, message);
                MessageService.Send(HttpContext.Current.Request, MessageAction.ContactAdminMailSent);

                return new {Status = 1, Message = Resource.AdminMessageSent};
            }
            catch(Exception ex)
            {
                return new {Status = 0, Message = ex.Message.HtmlEncode()};
            }
        }

        [SecurityPassthrough]
        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse SendJoinInviteMail(string email)
        {
            email = (email ?? "").Trim();
            var resp = new AjaxResponse {rs1 = "0"};

            try
            {
                if (String.IsNullOrEmpty(email))
                {
                    resp.rs2 = Resource.ErrorNotCorrectEmail;
                    return resp;
                }

                if (!email.TestEmailRegex())
                {
                    resp.rs2 = Resource.ErrorNotCorrectEmail;
                }

                var user = CoreContext.UserManager.GetUserByEmail(email);
                if (!user.ID.Equals(ASC.Core.Users.Constants.LostUser.ID))
                {
                    resp.rs1 = "0";
                    resp.rs2 = CustomNamingPeople.Substitute<Resource>("ErrorEmailAlreadyExists").HtmlEncode();
                    return resp;
                }

                var tenant = CoreContext.TenantManager.GetCurrentTenant();
                if (tenant != null)
                {
                    var settings = SettingsManager.Instance.LoadSettings<IPRestrictionsSettings>(tenant.TenantId);
                    if (settings.Enable && !IPSecurity.IPSecurity.Verify(tenant))
                    {
                        resp.rs2 = Resource.ErrorAccessRestricted;
                        return resp;
                    }
                }


                var trustedDomainSettings = SettingsManager.Instance.LoadSettings<StudioTrustedDomainSettings>(TenantProvider.CurrentTenantID);
                var emplType = trustedDomainSettings.InviteUsersAsVisitors ? EmployeeType.Visitor : EmployeeType.User;
                var enableInviteUsers = TenantStatisticsProvider.GetUsersCount() < TenantExtra.GetTenantQuota().ActiveUsers;

                if (!enableInviteUsers)
                {
                    emplType = EmployeeType.Visitor;
                }

                switch (tenant.TrustedDomainsType)
                {
                    case TenantTrustedDomainsType.Custom:
                        {
                            var address = new MailAddress(email);
                            if (tenant.TrustedDomains.Any(d => address.Address.EndsWith("@" + d, StringComparison.InvariantCultureIgnoreCase)))
                            {
                                StudioNotifyService.Instance.InviteUsers(email, "", true, emplType);
                                MessageService.Send(HttpContext.Current.Request, MessageInitiator.System, MessageAction.SentInviteInstructions, email);
                                resp.rs1 = "1";
                                resp.rs2 = Resource.FinishInviteJoinEmailMessage;
                                return resp;
                            }
                            else {
                                resp.rs2 = Resource.ErrorEmailDomainNotAllowed;
                            }
                        }
                        break;
                    case TenantTrustedDomainsType.All:
                        StudioNotifyService.Instance.InviteUsers(email, "", true, emplType);
                        MessageService.Send(HttpContext.Current.Request, MessageInitiator.System, MessageAction.SentInviteInstructions, email);
                        resp.rs1 = "1";
                        resp.rs2 = Resource.FinishInviteJoinEmailMessage;
                        return resp;
                    default:
                        resp.rs2 = Resource.ErrorNotCorrectEmail;
                        break;
                }
            }
            catch(FormatException)
            {
                resp.rs2 = Resource.ErrorNotCorrectEmail;
            }
            catch(Exception e)
            {
                resp.rs2 = HttpUtility.HtmlEncode(e.Message);
            }

            return resp;
        }

        public static string RenderTrustedDominTitle()
        {
            var tenant = CoreContext.TenantManager.GetCurrentTenant();
            switch (tenant.TrustedDomainsType)
            {
                case TenantTrustedDomainsType.Custom:
                    {
                        var domains = "<strong>";
                        var i = 0;
                        foreach (var d in tenant.TrustedDomains)
                        {
                            if (i != 0)
                                domains += "</strong>, <strong>";

                            domains += d;
                            i++;
                        }
                        domains += "</strong>";
                        return String.Format(Resource.TrustedDomainsInviteTitle.HtmlEncode(), domains);
                    }
                case TenantTrustedDomainsType.All:
                    return Resource.SignInFromAnyDomainInviteTitle.HtmlEncode();
            }

            return "";
        }
    }
}