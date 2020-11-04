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
using System.Net.Mail;
using System.Web;
using System.Web.UI;
using AjaxPro;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.MessagingSystem;
using ASC.Web.Core.Security;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.UserControls.Statistics;
using ASC.Web.Studio.Utility;
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
                var setting = StudioAdminMessageSettings.Load();
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

            Page.RegisterBodyScripts("~/UserControls/Users/AuthCommunications/js/authcommunications.js");
        }

        [SecurityPassthrough]
        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public object SendAdmMail(string email, string message)
        {
            try
            {
                if (!EnableAdmMess)
                    throw new MethodAccessException("Method not available");

                if (!email.TestEmailRegex())
                    throw new Exception(Resource.ErrorNotCorrectEmail);

                if (string.IsNullOrEmpty(message))
                    throw new Exception(Resource.ErrorEmptyMessage);

                var key = HttpContext.Current.Request.UserHostAddress + "adminmessages";
                var count = Convert.ToInt32(HttpContext.Current.Cache[key]);
                if (10 < count)
                    throw new Exception(Resource.ErrorRequestLimitExceeded);

                HttpContext.Current.Cache.Insert(key, ++count, null, System.Web.Caching.Cache.NoAbsoluteExpiration,
                                                 TimeSpan.FromMinutes(2));

                StudioNotifyService.Instance.SendMsgToAdminFromNotAuthUser(email, message);
                MessageService.Send(HttpContext.Current.Request, MessageAction.ContactAdminMailSent);

                return new {Status = 1, Message = Resource.AdminMessageSent};
            }
            catch (Exception ex)
            {
                return new {Status = 0, Message = ex.Message.HtmlEncode()};
            }
        }

        [SecurityPassthrough]
        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public object SendJoinInviteMail(string email)
        {
            try
            {
                if (!EnabledJoin)
                    throw new MethodAccessException("Method not available");

                if (!email.TestEmailRegex())
                    throw new Exception(Resource.ErrorNotCorrectEmail);

                var user = CoreContext.UserManager.GetUserByEmail(email);
                if (!user.ID.Equals(Constants.LostUser.ID))
                    throw new Exception(CustomNamingPeople.Substitute<Resource>("ErrorEmailAlreadyExists"));

                var tenant = CoreContext.TenantManager.GetCurrentTenant();
                var settings = IPRestrictionsSettings.Load();

                if (settings.Enable && !IPSecurity.IPSecurity.Verify(tenant))
                    throw new Exception(Resource.ErrorAccessRestricted);

                var trustedDomainSettings = StudioTrustedDomainSettings.Load();
                var emplType = trustedDomainSettings.InviteUsersAsVisitors ? EmployeeType.Visitor : EmployeeType.User;
                var enableInviteUsers = TenantStatisticsProvider.GetUsersCount() <
                                        TenantExtra.GetTenantQuota().ActiveUsers;

                if (!enableInviteUsers)
                    emplType = EmployeeType.Visitor;

                switch (tenant.TrustedDomainsType)
                {
                    case TenantTrustedDomainsType.Custom:
                        {
                            var address = new MailAddress(email);
                            if (
                                tenant.TrustedDomains.Any(
                                    d => address.Address.EndsWith("@" + d, StringComparison.InvariantCultureIgnoreCase)))
                            {
                                StudioNotifyService.Instance.SendJoinMsg(email, emplType);
                                MessageService.Send(HttpContext.Current.Request, MessageInitiator.System,
                                                    MessageAction.SentInviteInstructions, email);
                                return new {Status = 1, Message = Resource.FinishInviteJoinEmailMessage};
                            }

                            throw new Exception(Resource.ErrorEmailDomainNotAllowed);
                        }
                    case TenantTrustedDomainsType.All:
                        {
                            StudioNotifyService.Instance.SendJoinMsg(email, emplType);
                            MessageService.Send(HttpContext.Current.Request, MessageInitiator.System,
                                                MessageAction.SentInviteInstructions, email);
                            return new {Status = 1, Message = Resource.FinishInviteJoinEmailMessage};
                        }
                    default:
                        throw new Exception(Resource.ErrorNotCorrectEmail);
                }
            }
            catch (FormatException)
            {
                return new {Status = 0, Message = Resource.ErrorNotCorrectEmail};
            }
            catch (Exception e)
            {
                return new {Status = 0, Message = e.Message.HtmlEncode()};
            }
        }

        public static string RenderTrustedDominTitle()
        {
            var tenant = CoreContext.TenantManager.GetCurrentTenant();

            switch (tenant.TrustedDomainsType)
            {
                case TenantTrustedDomainsType.Custom:
                    {
                        var domains = "<strong>" +
                                      string.Join("</strong>, <strong>", tenant.TrustedDomains) +
                                      "</strong>";
                        return String.Format(Resource.TrustedDomainsInviteTitle.HtmlEncode(), domains);
                    }
                case TenantTrustedDomainsType.All:
                    return Resource.SignInFromAnyDomainInviteTitle.HtmlEncode();
            }

            return string.Empty;
        }
    }
}