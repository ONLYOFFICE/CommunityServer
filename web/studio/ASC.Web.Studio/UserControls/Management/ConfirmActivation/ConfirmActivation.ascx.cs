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
using System.Collections.Specialized;
using System.Threading;
using System.Web;
using System.Web.UI;
using ASC.Core;
using ASC.Core.Users;
using ASC.MessagingSystem;
using ASC.Security.Cryptography;
using ASC.Web.Core;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.Core.SMS;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.Utility;
using AjaxPro;
using Resources;
using Constants = ASC.Core.Users.Constants;

namespace ASC.Web.Studio.UserControls.Management
{
    [AjaxNamespace("ConfirmActivation")]
    public partial class ConfirmActivation : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Management/ConfirmActivation/ConfirmActivation.ascx"; }
        }

        protected UserInfo User { get; set; }
        protected ConfirmType Type { get; set; }

        protected bool isPersonal
        {
            get { return CoreContext.Configuration.Personal; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(typeof(ConfirmActivation));
            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/management/confirmactivation/js/confirmactivation.js"));
            Page.RegisterStyleControl(VirtualPathUtility.ToAbsolute("~/usercontrols/management/ConfirmActivation/css/confirmactivation.less"));
            Page.Title = HeaderStringHelper.GetPageTitle(Resource.Authorization);
            ButtonEmailAndPasswordOK.Text = Resource.EmailAndPasswordOK;
            btChangeEmail.Text = Resource.ChangeEmail;

            var email = (Request["email"] ?? "").Trim();
            if (string.IsNullOrEmpty(email))
            {
                ShowError(Resource.ErrorNotCorrectEmail);
                return;
            }

            Type = typeof(ConfirmType).TryParseEnum(Request["type"] ?? "", ConfirmType.EmpInvite);

            User = CoreContext.UserManager.GetUserByEmail(email);
            if (User.ID.Equals(Constants.LostUser.ID))
            {
                ShowError(string.Format(Resource.ErrorUserNotFoundByEmail, email));
                return;
            }

            try
            {
                UserAuth(User, HttpContext.Current);
                ActivateMail(User);

                if (Type == ConfirmType.EmailChange && !IsPostBack)
                {
                    emailChange.Visible = true;
                    AjaxPro.Utility.RegisterTypeForAjax(typeof(EmailOperationService));
                }
                else if (Type == ConfirmType.PasswordChange)
                {
                    passwordSetter.Visible = true;
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private static void UserAuth(UserInfo user, HttpContext context)
        {
            if (SecurityContext.IsAuthenticated) return;

            if (StudioSmsNotificationSettings.IsVisibleSettings && StudioSmsNotificationSettings.Enable)
            {
                context.Session["refererURL"] = context.Request.Url.AbsoluteUri;
                context.Response.Redirect(Confirm.SmsConfirmUrl(user), true);
                return;
            }

            var cookiesKey = SecurityContext.AuthenticateMe(user.ID);
            CookiesManager.SetCookies(CookiesType.AuthKey, cookiesKey);
            MessageService.Send(HttpContext.Current.Request, MessageAction.LoginSuccess);
        }

        private static void ActivateMail(UserInfo user)
        {
            if (user.ActivationStatus == EmployeeActivationStatus.Activated) return;

            user.ActivationStatus = EmployeeActivationStatus.Activated;
            CoreContext.UserManager.SaveUserInfo(user);

            MessageService.Send(HttpContext.Current.Request,
                                user.IsVisitor() ? MessageAction.GuestActivated : MessageAction.UserActivated,
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

        protected void LoginToPortal(object sender, EventArgs e)
        {
            try
            {
                var pwd = Request.Form["pwdInput"];

                UserManagerWrapper.CheckPasswordPolicy(pwd);
                if (string.IsNullOrEmpty(pwd))
                {
                    throw new ArgumentException(Resource.ErrorMissMatchPwd);
                }

                SecurityContext.SetUserPassword(User.ID, pwd);
                MessageService.Send(HttpContext.Current.Request, MessageAction.UserUpdatedPassword);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message, false);
            }

            Response.Redirect(CommonLinkUtility.GetDefault());
        }

        [AjaxMethod]
        public AjaxResponse SendEmailActivationInstructionsOnChange(string newEmail, string queryString)
        {
            const string StatusSuccess = "success";
            const string StatusError = "error";
            const string StatusFatalError = "fatalerror";

            var response = new AjaxResponse { status = StatusSuccess };

            if (String.IsNullOrEmpty(queryString))
            {
                response.status = StatusFatalError;
                response.message = Resource.ErrorConfirmURLError;
                return response;
            }

            if (String.IsNullOrEmpty(newEmail))
            {
                response.status = StatusError;
                response.message = Resource.ErrorEmailEmpty;
                return response;
            }

            try
            {
                var result = CheckValidationKey(queryString.Substring(1));
                if (result != EmailValidationKeyProvider.ValidationResult.Ok)
                {
                    response.status = StatusFatalError;
                    switch (result)
                    {
                        case EmailValidationKeyProvider.ValidationResult.Invalid:
                            response.message = Resource.ErrorInvalidActivationLink;
                            break;
                        case EmailValidationKeyProvider.ValidationResult.Expired:
                            response.message = Resource.ErrorExpiredActivationLink;
                            break;
                        default:
                            response.message = Resource.ErrorConfirmURLError;
                            break;
                    }
                    return response;
                }

                var user = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
                if (user == null)
                {
                    response.status = StatusFatalError;
                    response.message = Resource.ErrorUserNotFound;
                    return response;
                }

                var existentUser = CoreContext.UserManager.GetUserByEmail(newEmail);
                if (existentUser != null && existentUser.ID == user.ID)
                {
                    response.status = StatusError;
                    response.message = Resource.ErrorEmailsAreTheSame;
                    return response;
                }

                if (existentUser != null && existentUser.ID != Constants.LostUser.ID)
                {
                    response.status = StatusError;
                    response.message = CustomNamingPeople.Substitute<Resource>("ErrorEmailAlreadyExists");
                    return response;
                }

                user.Email = newEmail;
                user.ActivationStatus = EmployeeActivationStatus.NotActivated;
                CoreContext.UserManager.SaveUserInfo(user);

                StudioNotifyService.Instance.SendEmailActivationInstructions(user, newEmail);
                MessageService.Send(HttpContext.Current.Request, MessageAction.UserSentActivationInstructions, user.DisplayUserName(false));

                response.message = String.Format(Resource.MessageEmailActivationInstuctionsSentOnEmail, "<b>" + newEmail + "</b>");
                return response;
            }
            catch (Exception)
            {
                response.status = StatusFatalError;
                response.message = Resource.UnknownError;
                return response;
            }
        }

        private static EmailValidationKeyProvider.ValidationResult CheckValidationKey(string queryString)
        {
            var request = BuildRequestFromQueryString(queryString);

            var type = request["type"];
            if (String.IsNullOrEmpty(type) || type.ToLowerInvariant() != ConfirmType.EmailChange.ToString().ToLowerInvariant())
            {
                return EmailValidationKeyProvider.ValidationResult.Invalid;
            }

            var email = request["email"];
            if (String.IsNullOrEmpty(email) || !email.TestEmailRegex())
            {
                return EmailValidationKeyProvider.ValidationResult.Invalid;
            }

            var key = request["key"];
            if (String.IsNullOrEmpty(key))
            {
                return EmailValidationKeyProvider.ValidationResult.Invalid;
            }

            return EmailValidationKeyProvider.ValidateEmailKey(email + type, key, SetupInfo.ValidEamilKeyInterval);
        }

        private static NameValueCollection BuildRequestFromQueryString(string queryString)
        {
            return HttpUtility.ParseQueryString(queryString);
        }
    }
}