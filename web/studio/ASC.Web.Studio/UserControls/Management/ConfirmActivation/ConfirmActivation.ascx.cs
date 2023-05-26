/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.Threading;
using System.Web;
using System.Web.UI;

using ASC.Core;
using ASC.Core.Users;
using ASC.MessagingSystem;
using ASC.Web.Core;
using ASC.Web.Core.Utility;
using ASC.Web.Studio.Core.SMS;
using ASC.Web.Studio.Core.TFA;
using ASC.Web.Studio.PublicResources;
using ASC.Web.Studio.Utility;

using Constants = ASC.Core.Users.Constants;

namespace ASC.Web.Studio.UserControls.Management
{
    public partial class ConfirmActivation : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Management/ConfirmActivation/ConfirmActivation.ascx"; }
        }

        private UserInfo User { get; set; }
        protected ConfirmType Type { get; set; }

        protected Web.Core.Utility.PasswordSettings TenantPasswordSettings;

        protected bool isPersonal
        {
            get { return CoreContext.Configuration.Personal; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.RegisterBodyScripts("~/UserControls/Management/ConfirmActivation/js/confirmactivation.js");

            if(ModeThemeSettings.GetModeThemesSettings().ModeThemeName == ModeTheme.dark)
            {
                Page.RegisterStyle("~/UserControls/Management/ConfirmActivation/css/dark-confirmactivation.less");
            }
            else
            {
                Page.RegisterStyle("~/UserControls/Management/ConfirmActivation/css/confirmactivation.less");
            }
            Page.Title = HeaderStringHelper.GetPageTitle(Resource.Authorization);

            var email = (Request["email"] ?? "").Trim();
            if (string.IsNullOrEmpty(email))
            {
                ShowError(Resource.ErrorNotCorrectEmail);
                return;
            }

            Type = typeof(ConfirmType).TryParseEnum(Request["type"] ?? "", ConfirmType.EmpInvite);

            TenantPasswordSettings = Web.Core.Utility.PasswordSettings.Load();

            try
            {
                if (Type == ConfirmType.EmailChange)
                {
                    User = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
                    User.Email = email;
                    CoreContext.UserManager.SaveUserInfo(User, syncCardDav: true);
                    MessageService.Send(Request, MessageAction.UserUpdatedEmail);

                    ActivateMail(User);
                    const string successParam = "email_change=success";
                    CookiesManager.ResetUserCookie();
                    if (User.IsVisitor())
                    {
                        Response.Redirect(CommonLinkUtility.ToAbsolute("~/My.aspx?" + successParam), true);
                    }
                    Response.Redirect(string.Format("{0}&{1}", User.GetUserProfilePageURL(), successParam), true);
                    return;
                }

                User = CoreContext.UserManager.GetUserByEmail(email);
                if (User.ID.Equals(Constants.LostUser.ID))
                {
                    ShowError(string.Format(Resource.ErrorUserNotFoundByEmail, email));
                    return;
                }

                if (!User.ID.Equals(SecurityContext.CurrentAccount.ID))
                {
                    Auth.ProcessLogout();
                }

                UserAuth(User);
                ActivateMail(User);

                passwordSetter.Visible = true;
                ButtonEmailAndPasswordOK.Text = Resource.EmailAndPasswordOK;
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }

            if (IsPostBack)
            {
                LoginToPortal();
            }
        }

        private void UserAuth(UserInfo user)
        {
            if (SecurityContext.IsAuthenticated) return;

            if (StudioSmsNotificationSettings.IsVisibleAndAvailableSettings && StudioSmsNotificationSettings.TfaEnabledForUser(user.ID))
            {
                Response.Redirect(Request.AppendRefererURL(Confirm.SmsConfirmUrl(user)), true);
                return;
            }
            if (TfaAppAuthSettings.IsVisibleSettings && TfaAppAuthSettings.TfaEnabledForUser(user.ID))
            {
                Response.Redirect(Request.AppendRefererURL(Confirm.TfaConfirmUrl(user)), true);
                return;
            }
            try
            {
                CookiesManager.AuthenticateMeAndSetCookies(user.Tenant, user.ID, MessageAction.LoginSuccess);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void ActivateMail(UserInfo user)
        {
            if (user.ActivationStatus.HasFlag(EmployeeActivationStatus.Activated)) return;

            user.ActivationStatus = EmployeeActivationStatus.Activated;
            CoreContext.UserManager.SaveUserInfo(user);

            MessageService.Send(Request,
                                user.IsVisitor() ? MessageAction.GuestActivated : MessageAction.UserActivated,
                                MessageTarget.Create(user.ID),
                                user.DisplayUserName(false));
        }

        private void ShowError(string message, bool redirect = true)
        {
            var confirm = Page as Confirm;
            if (confirm != null)
                confirm.ErrorMessage = HttpUtility.HtmlEncode(message);

            Auth.ProcessLogout();

            if (redirect)
                RegisterRedirect();
        }

        private void RegisterRedirect()
        {
            Page.ClientScript.RegisterStartupScript(GetType(), "redirect",
                                                    string.Format("setTimeout('location.href = \"{0}\";',10000);", CommonLinkUtility.GetDefault()),
                                                    true);
        }

        protected void LoginToPortal()
        {
            try
            {
                var passwordHash = Request["passwordHash"];
                if (string.IsNullOrEmpty(passwordHash)) throw new Exception(Resource.ErrorPasswordEmpty);

                SecurityContext.SetUserPasswordHash(User.ID, passwordHash);
                MessageService.Send(Request, MessageAction.UserUpdatedPassword);

                CookiesManager.ResetUserCookie();
                MessageService.Send(Request, MessageAction.CookieSettingsUpdated);
            }
            catch (SecurityContext.PasswordException)
            {
                ShowError(Resource.ErrorPasswordRechange, false);
                return;
            }
            catch (Exception ex)
            {
                ShowError(ex.Message, false);
                return;
            }

            Response.Redirect(CommonLinkUtility.GetDefault());
        }
    }
}