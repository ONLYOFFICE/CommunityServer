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
using System.Web;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.MessagingSystem;
using ASC.Security.Cryptography;
using ASC.Web.Core;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.Core.SMS;
using ASC.Web.Studio.Core.TFA;
using ASC.Web.Studio.UserControls.Management;
using ASC.Web.Studio.Utility;

using Resources;
using Constants = ASC.Core.Users.Constants;

namespace ASC.Web.Studio
{
    public partial class Confirm : MainPage
    {
        protected override bool MayNotAuth
        {
            get { return _type != ConfirmType.EmailChange; }
        }

        protected override bool MayNotPaid
        {
            get { return true; }
        }

        protected override bool MayPhoneNotActivate
        {
            get { return true; }
        }

        public string ErrorMessage { get; set; }

        private string _email;
        private ConfirmType _type;

        protected override void OnPreInit(EventArgs e)
        {
            _type = typeof(ConfirmType).TryParseEnum(Request["type"] ?? "", ConfirmType.EmpInvite);

            base.OnPreInit(e);

            if (!SecurityContext.IsAuthenticated && CoreContext.Configuration.Personal)
            {
                SetLanguage();
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.Title = HeaderStringHelper.GetPageTitle(Resource.AccountControlPageTitle);

            Master.DisabledSidePanel = true;
            Master.TopStudioPanel.DisableProductNavigation = true;
            Master.TopStudioPanel.DisableUserInfo = true;
            Master.TopStudioPanel.DisableSearch = true;
            Master.TopStudioPanel.DisableSettings = true;
            Master.TopStudioPanel.DisableTariff = true;
            Master.TopStudioPanel.DisableLoginPersonal = true;
            Master.TopStudioPanel.DisableGift = true;

            _email = (Request["email"] ?? "").Trim();

            var tenant = CoreContext.TenantManager.GetCurrentTenant();
            if (tenant.Status != TenantStatus.Active && _type != ConfirmType.PortalContinue)
            {
                Response.Redirect(CommonLinkUtility.GetDefault(), true);
                return;
            }

            if (_type == ConfirmType.PhoneActivation && SecurityContext.IsAuthenticated)
            {
                Master.TopStudioPanel.DisableUserInfo = false;
            }
            else
            {
                Master.DisabledTopStudioPanel = true;
            }

            if (!CheckValidationKey())
            {
                return;
            }

            LoadControls();
        }

        private bool CheckValidationKey()
        {
            var key = Request["key"] ?? "";
            var emplType = Request["emplType"] ?? "";
            var social = Request["social"] ?? "";

            var validInterval = SetupInfo.ValidEmailKeyInterval;
            var authInterval = SetupInfo.ValidAuthKeyInterval;

            EmailValidationKeyProvider.ValidationResult checkKeyResult;
            switch (_type)
            {
                case ConfirmType.PortalContinue:
                    checkKeyResult = EmailValidationKeyProvider.ValidateEmailKey(_email + _type, key);
                    break;

                case ConfirmType.PhoneActivation:
                case ConfirmType.PhoneAuth:
                case ConfirmType.TfaActivation:
                case ConfirmType.TfaAuth:
                    checkKeyResult = EmailValidationKeyProvider.ValidateEmailKey(_email + _type, key, authInterval);
                    break;

                case ConfirmType.Auth:
                    {
                        var first = Request["first"] ?? "";
                        var module = Request["module"] ?? "";
                        var smsConfirm = Request["sms"] ?? "";

                        checkKeyResult = EmailValidationKeyProvider.ValidateEmailKey(_email + _type + first + module + smsConfirm, key, authInterval);

                        if (checkKeyResult == EmailValidationKeyProvider.ValidationResult.Ok)
                        {
                            var user = _email.Contains("@")
                                           ? CoreContext.UserManager.GetUserByEmail(_email)
                                           : CoreContext.UserManager.GetUsers(new Guid(_email));

                            if (SecurityContext.IsAuthenticated && SecurityContext.CurrentAccount.ID != user.ID)
                            {
                                Auth.ProcessLogout();
                            }

                            if (!SecurityContext.IsAuthenticated)
                            {
                                if (!CoreContext.UserManager.UserExists(user.ID) || user.Status != EmployeeStatus.Active)
                                {
                                    ShowError(Auth.MessageKey.ErrorUserNotFound);
                                    return false;
                                }

                                if (StudioSmsNotificationSettings.IsVisibleSettings && StudioSmsNotificationSettings.Enable && smsConfirm.ToLower() != "true")
                                {
                                    //todo: think about 'first' & 'module'
                                    Response.Redirect(SmsConfirmUrl(user), true);
                                }

                                if (TfaAppAuthSettings.IsVisibleSettings && TfaAppAuthSettings.Enable)
                                {
                                    //todo: think about 'first' & 'module'
                                    Response.Redirect(TfaConfirmUrl(user), true);
                                }

                                var authCookie = SecurityContext.AuthenticateMe(user.ID);
                                CookiesManager.SetCookies(CookiesType.AuthKey, authCookie);

                                var messageAction = social == "true" ? MessageAction.LoginSuccessViaSocialAccount : MessageAction.LoginSuccess;
                                MessageService.Send(HttpContext.Current.Request, messageAction);
                            }

                            SetDefaultModule(module);

                            AuthRedirect(first.ToLower() == "true");
                        }
                    }
                    break;

                case ConfirmType.DnsChange:
                    {
                        var dnsChangeKey = string.Join(string.Empty, new[] { _email, _type.ToString(), Request["dns"], Request["alias"] });
                        checkKeyResult = EmailValidationKeyProvider.ValidateEmailKey(dnsChangeKey, key, validInterval);
                    }
                    break;

                case ConfirmType.PortalOwnerChange:
                    {
                        Guid uid;
                        try
                        {
                            uid = new Guid(Request["uid"]);
                        }
                        catch
                        {
                            uid = Guid.Empty;
                        }
                        checkKeyResult = EmailValidationKeyProvider.ValidateEmailKey(_email + _type + uid, key, validInterval);
                    }
                    break;

                case ConfirmType.EmpInvite:
                    checkKeyResult = EmailValidationKeyProvider.ValidateEmailKey(_email + _type + emplType, key, validInterval);
                    break;

                case ConfirmType.LinkInvite:
                    checkKeyResult = EmailValidationKeyProvider.ValidateEmailKey(_type + emplType, key, validInterval);
                    break;

                case ConfirmType.EmailChange:
                    checkKeyResult = EmailValidationKeyProvider.ValidateEmailKey(_email + _type + SecurityContext.CurrentAccount.ID, key, validInterval);
                    break;

                case ConfirmType.PasswordChange:
                    var hash = CoreContext.Authentication.GetUserPasswordStamp(CoreContext.UserManager.GetUserByEmail(_email).ID).ToString("s");

                    checkKeyResult = EmailValidationKeyProvider.ValidateEmailKey(_email + _type + hash, key, validInterval);
                    break;

                default:
                    checkKeyResult = EmailValidationKeyProvider.ValidateEmailKey(_email + _type, key, validInterval);
                    break;
            }

            if (checkKeyResult == EmailValidationKeyProvider.ValidationResult.Expired)
            {
                ShowError(Auth.MessageKey.ErrorExpiredActivationLink);
                return false;
            }

            if (checkKeyResult == EmailValidationKeyProvider.ValidationResult.Invalid)
            {
                ShowError(_type == ConfirmType.LinkInvite
                              ? Auth.MessageKey.ErrorInvalidActivationLink
                              : Auth.MessageKey.ErrorConfirmURLError);
                return false;
            }

            if (!string.IsNullOrEmpty(_email) && !_email.TestEmailRegex())
            {
                ShowError(Auth.MessageKey.ErrorNotCorrectEmail);
                return false;
            }

            return true;
        }

        private void LoadControls()
        {
            switch (_type)
            {
                case ConfirmType.EmpInvite:
                case ConfirmType.LinkInvite:
                case ConfirmType.Activation:
                    _confirmHolder2.Controls.Add(LoadControl(ConfirmInviteActivation.Location));
                    _contentWithControl.Visible = false;
                    break;

                case ConfirmType.EmailChange:
                case ConfirmType.PasswordChange:
                    _confirmHolder.Controls.Add(LoadControl(ConfirmActivation.Location));
                    break;

                case ConfirmType.EmailActivation:
                    ProcessEmailActivation(_email);
                    break;

                case ConfirmType.PortalRemove:
                case ConfirmType.PortalSuspend:
                case ConfirmType.PortalContinue:
                case ConfirmType.DnsChange:
                    _confirmHolder.Controls.Add(LoadControl(ConfirmPortalActivity.Location));
                    break;

                case ConfirmType.PortalOwnerChange:
                    _confirmHolder.Controls.Add(LoadControl(ConfirmPortalOwner.Location));
                    break;

                case ConfirmType.ProfileRemove:
                    var user = CoreContext.UserManager.GetUserByEmail(_email);
                    if (user.ID.Equals(Constants.LostUser.ID))
                    {
                        ShowError(Auth.MessageKey.ErrorUserNotFound);
                        return;
                    }

                    var control = (ProfileOperation)LoadControl(ProfileOperation.Location);
                    control.UserId = user.ID;
                    _confirmHolder.Controls.Add(control);
                    break;

                case ConfirmType.PhoneActivation:
                case ConfirmType.PhoneAuth:
                    var confirmMobileActivation = (ConfirmMobileActivation)LoadControl(ConfirmMobileActivation.Location);
                    confirmMobileActivation.Activation = _type == ConfirmType.PhoneActivation;
                    confirmMobileActivation.User = CoreContext.UserManager.GetUserByEmail(_email);
                    _confirmHolder.Controls.Add(confirmMobileActivation);
                    break;
                case ConfirmType.TfaActivation:
                case ConfirmType.TfaAuth:
                    var confirmTfaActivation = (TfaActivation)LoadControl(TfaActivation.Location);
                    confirmTfaActivation.Activation = _type == ConfirmType.TfaActivation;
                    confirmTfaActivation.User = CoreContext.UserManager.GetUserByEmail(_email);
                    _confirmHolder.Controls.Add(confirmTfaActivation);
                    break;
            }
        }

        private void ProcessEmailActivation(string email)
        {
            var user = CoreContext.UserManager.GetUserByEmail(email);

            if (user.ID.Equals(Constants.LostUser.ID))
            {
                ShowError(Auth.MessageKey.ErrorConfirmURLError);
            }
            else if (user.ActivationStatus.HasFlag(EmployeeActivationStatus.Activated))
            {
                Response.Redirect(CommonLinkUtility.GetDefault());
            }
            else
            {
                try
                {
                    SecurityContext.AuthenticateMe(ASC.Core.Configuration.Constants.CoreSystem);

                    if (user.ActivationStatus.HasFlag(EmployeeActivationStatus.AutoGenerated))
                    {
                        user.ActivationStatus |= EmployeeActivationStatus.Activated;
                    }
                    else
                    {
                        user.ActivationStatus = EmployeeActivationStatus.Activated;
                    }

                    user = CoreContext.UserManager.SaveUserInfo(user);

                    var first = Request["first"] ?? "";
                    if (first.ToLower() == "true" && !CoreContext.Configuration.Personal && user.IsAdmin()) {
                        StudioNotifyService.Instance.SendAdminWelcome(user);
                    }

                    MessageService.Send(HttpContext.Current.Request, MessageInitiator.System, MessageAction.UserActivated, user.DisplayUserName(false));
                }
                finally
                {
                    Auth.ProcessLogout();
                }

                string redirectUrl;

                if (user.IsLDAP())
                {
                    redirectUrl = String.Format("~/Auth.aspx?ldap-login={0}", user.UserName);
                }
                else
                {
                    redirectUrl= String.Format("~/Auth.aspx?confirmed-email={0}", email);
                }

                Response.Redirect(redirectUrl, true);
            }
        }

        private void ShowError(Auth.MessageKey messageKey = Auth.MessageKey.None)
        {
            if (SecurityContext.IsAuthenticated)
            {
                ErrorMessage = Auth.GetAuthMessage(messageKey);
                _confirmHolder.Visible = false;
            }
            else
            {
                Response.Redirect(string.Format("~/Auth.aspx?am={0}", (int)messageKey));
            }
        }

        private void AuthRedirect(bool first)
        {
            var wizardSettings = WizardSettings.Load();
            if (first && wizardSettings.Completed)
            {
                wizardSettings.Completed = false;
                wizardSettings.Save();
            }

            string url;
            if (wizardSettings.Completed)
            {
                url = CommonLinkUtility.GetDefault();
            }
            else
            {
                url = SecurityContext.IsAuthenticated ? "~/Wizard.aspx" : "~/Auth.aspx";
            }

            if (Request.DesktopApp())
            {
                if (wizardSettings.Completed)
                {
                    url = WebItemManager.Instance[WebItemManager.DocumentsProductID].StartURL;
                }
                url += "?desktop=true&first=true";
            }

            Response.Redirect(url, true);
        }

        public static string SmsConfirmUrl(UserInfo user)
        {
            if (user == null)
                return string.Empty;

            var confirmType =
                string.IsNullOrEmpty(user.MobilePhone) ||
                user.MobilePhoneActivationStatus == MobilePhoneActivationStatus.NotActivated
                    ? ConfirmType.PhoneActivation
                    : ConfirmType.PhoneAuth;

            return CommonLinkUtility.GetConfirmationUrl(user.Email, confirmType);
        }

        public static string TfaConfirmUrl(UserInfo user)
        {
            if (user == null)
                return string.Empty;
            var confirmType = TfaAppUserSettings.EnableForUser(user.ID)
                ? ConfirmType.TfaAuth
                : ConfirmType.TfaActivation;
            return CommonLinkUtility.GetConfirmationUrl(user.Email, confirmType);
        }

        private static void SetDefaultModule(string module)
        {
            if (string.IsNullOrEmpty(module)) return;
            try
            {
                LogManager.GetLogger("ASC.Web").Debug("SetDefaultModule " + module);
                new StudioDefaultPageSettings { DefaultProductID = new Guid(module) }.Save();
                MessageService.Send(HttpContext.Current.Request, MessageAction.DefaultStartPageSettingsUpdated);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("ASC.Web").Error("SetDefaultModule", ex);
            }
        }
    }
}