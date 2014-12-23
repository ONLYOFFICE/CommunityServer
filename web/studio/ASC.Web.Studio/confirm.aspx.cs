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
using System.IO;
using System.Web;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.MessagingSystem;
using ASC.Security.Cryptography;
using ASC.Web.Core;
using ASC.Web.Core.Files;
using ASC.Web.Core.Users;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.SMS;
using ASC.Web.Studio.UserControls.FirstTime;
using ASC.Web.Studio.UserControls.Management;
using ASC.Web.Studio.Utility;
using Resources;

namespace ASC.Web.Studio
{
    public partial class Confirm : MainPage
    {
        protected override bool MayNotAuth
        {
            get { return true; }
        }

        protected override bool MayNotPaid
        {
            get { return true; }
        }

        protected override bool MayPhoneNotActivate
        {
            get { return true; }
        }

        protected TenantInfoSettings _tenantInfoSettings;

        public string ErrorMessage { get; set; }

        private string _email;
        private ConfirmType _type;

        protected bool isPersonal
        {
            get { return CoreContext.Configuration.Personal; }
        }

        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);
            _type = typeof(ConfirmType).TryParseEnum(Request["type"] ?? "", ConfirmType.EmpInvite);

            if (!SecurityContext.IsAuthenticated && CoreContext.Configuration.Personal)
            {
                if (Request["campaign"] == "personal")
                {
                    Session["campaign"] = "personal";
                }
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

            _tenantInfoSettings = SettingsManager.Instance.LoadSettings<TenantInfoSettings>(TenantProvider.CurrentTenantID);

            _email = Request["email"] ?? "";

            var tenant = CoreContext.TenantManager.GetCurrentTenant();
            if (tenant.Status != TenantStatus.Active && _type != ConfirmType.PortalContinue)
            {
                Response.Redirect(SetupInfo.NoTenantRedirectURL, true);
            }

            if (_type == ConfirmType.PhoneActivation && SecurityContext.IsAuthenticated)
            {
                Master.TopStudioPanel.DisableUserInfo = false;
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

            var validInterval = SetupInfo.ValidEamilKeyInterval;
            var authInterval = TimeSpan.FromHours(1);

            EmailValidationKeyProvider.ValidationResult checkKeyResult;
            switch (_type)
            {
                case ConfirmType.PortalContinue:
                    checkKeyResult = EmailValidationKeyProvider.ValidateEmailKey(_email + _type, key);
                    break;

                case ConfirmType.PhoneActivation:
                case ConfirmType.PhoneAuth:
                    checkKeyResult = EmailValidationKeyProvider.ValidateEmailKey(_email + _type, key, authInterval);
                    break;

                case ConfirmType.Auth:
                    {
                        var first = Request["first"] ?? "";
                        var module = Request["module"];

                        checkKeyResult = EmailValidationKeyProvider.ValidateEmailKey(_email + _type + first + module, key, authInterval);

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
                                if (StudioSmsNotificationSettings.IsVisibleSettings && StudioSmsNotificationSettings.Enable)
                                {
                                    Response.Redirect(SmsConfirmUrl(user), true);
                                }

                                var authCookie = SecurityContext.AuthenticateMe(user.ID);
                                CookiesManager.SetCookies(CookiesType.AuthKey, authCookie);

                                var messageAction = social == "true" ? MessageAction.LoginSuccessViaSocialAccount : MessageAction.LoginSuccess;
                                MessageService.Send(HttpContext.Current.Request, messageAction);
                            }

                            AuthRedirect(user, first.ToLower() == "true", module, Request[FilesLinkUtility.FileUri]);
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

                case ConfirmType.PasswordChange:

                    var userHash = !String.IsNullOrEmpty(Request["p"]) && Request["p"] == "1";

                    String hash = String.Empty;

                    if (userHash)
                       hash = CoreContext.Authentication.GetUserPasswordHash(CoreContext.UserManager.GetUserByEmail(_email).ID);
                    
                    checkKeyResult = EmailValidationKeyProvider.ValidateEmailKey(_email + _type + (string.IsNullOrEmpty(hash) ? string.Empty : Hasher.Base64Hash(hash)), key, validInterval);
                    break;

                default:
                    checkKeyResult = EmailValidationKeyProvider.ValidateEmailKey(_email + _type, key, validInterval);
                    break;
            }

            if (checkKeyResult == EmailValidationKeyProvider.ValidationResult.Expired)
            {
                ShowError(Resource.ErrorExpiredActivationLink);
                return false;
            }

            if (checkKeyResult == EmailValidationKeyProvider.ValidationResult.Invalid)
            {
                ShowError(_type == ConfirmType.LinkInvite
                              ? Resource.ErrorInvalidActivationLink
                              : Resource.ErrorConfirmURLError);
                return false;
            }

            if (!string.IsNullOrEmpty(_email) && !_email.TestEmailRegex())
            {
                ShowError(Resource.ErrorNotCorrectEmail);
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
                        ShowError(Resource.ErrorUserNotFound);
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
            }
        }

        private void ProcessEmailActivation(string email)
        {
            var user = CoreContext.UserManager.GetUserByEmail(email);

            if (user.ID.Equals(Constants.LostUser.ID))
            {
                ShowError(Resource.ErrorConfirmURLError);
            }
            else if (user.ActivationStatus == EmployeeActivationStatus.Activated)
            {
                Response.Redirect(CommonLinkUtility.GetDefault());
            }
            else
            {
                try
                {
                    SecurityContext.AuthenticateMe(ASC.Core.Configuration.Constants.CoreSystem);
                    user.ActivationStatus = EmployeeActivationStatus.Activated;
                    CoreContext.UserManager.SaveUserInfo(user);
                    MessageService.Send(HttpContext.Current.Request, MessageInitiator.System, MessageAction.UserActivated, user.DisplayUserName(false));
                }
                finally
                {
                    Auth.ProcessLogout();
                }

                var redirectUrl = String.Format("~/auth.aspx?confirmed-email={0}", email);
                Response.Redirect(redirectUrl, true);
            }
        }

        private void ShowError(string error)
        {
            if (SecurityContext.IsAuthenticated)
            {
                ErrorMessage = error;
                _confirmHolder.Visible = false;
            }
            else
            {
                Response.Redirect(string.Format("~/auth.aspx?m={0}", HttpUtility.UrlEncode(error)));
            }
        }

        private void AuthRedirect(UserInfo user, bool first, string module, string fileUri)
        {
            var wizardSettings = SettingsManager.Instance.LoadSettings<WizardSettings>(TenantProvider.CurrentTenantID);
            if (first && wizardSettings.Completed)
            {
                // wizardSettings.Completed - open source, Request["first"] - cloud
                wizardSettings.Completed = false;
                SettingsManager.Instance.SaveSettings(wizardSettings, TenantProvider.CurrentTenantID);
            }

            if (wizardSettings.Completed)
            {
                if (string.IsNullOrEmpty(module))
                {
                    Response.Redirect(CommonLinkUtility.GetDefault(), true);
                }
                else
                {
                    SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

                    FirstTimeTenantSettings.SetDefaultTenantSettings();
                    FirstTimeTenantSettings.SendInstallInfo(user);

                    if (!string.IsNullOrEmpty(fileUri))
                    {
                        UserHelpTourHelper.IsNewUser = true;

                        var fileExt = FileUtility.GetInternalExtension(Path.GetFileName(HttpUtility.UrlDecode(fileUri)));
                        var createUrl = FilesLinkUtility.GetFileWebEditorExternalUrl(fileUri, "Demo" + fileExt, true);
                        Response.Redirect(createUrl, true);
                    }

                    Response.Redirect(CommonLinkUtility.GetDefault(), true);

                }
            }
            else
            {
                Response.Redirect(SecurityContext.IsAuthenticated ? "~/wizard.aspx" : "~/auth.aspx", true);
            }
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
    }
}