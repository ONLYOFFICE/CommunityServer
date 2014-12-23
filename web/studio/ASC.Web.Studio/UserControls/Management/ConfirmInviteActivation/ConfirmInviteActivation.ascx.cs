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
using System.Net;
using System.Threading;
using System.Web;
using System.Web.UI;
using ASC.Common.Utils;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.FederatedLogin;
using ASC.FederatedLogin.Profile;
using ASC.MessagingSystem;
using ASC.Web.Core;
using ASC.Web.Core.Users;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.UserControls.Statistics;
using ASC.Web.Studio.UserControls.Users.UserProfile;
using ASC.Web.Studio.Utility;
using Resources;
using System.Text.RegularExpressions;
using System.Configuration;

namespace ASC.Web.Studio.UserControls.Management
{
    public partial class ConfirmInviteActivation : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Management/ConfirmInviteActivation/ConfirmInviteActivation.ascx"; }
        }

        protected string _errorMessage;

        protected TenantInfoSettings _tenantInfoSettings;
        protected string _userName;
        protected string _userPost;
        protected string _userAvatar;

        protected string _email
        {
            get { return (Request["email"] ?? String.Empty).Trim(); }
        }

        protected string _firstName
        {
            get { return (Request["firstname"] ?? String.Empty).Trim(); }
        }

        protected string _lastName
        {
            get { return (Request["lastname"] ?? String.Empty).Trim(); }
        }

        protected string _pwd
        {
            get { return (Request["pwd"] ?? "").Trim(); }
        }

        protected string _rePwd
        {
            get { return (Request["repwd"] ?? "").Trim(); }
        }

        protected ConfirmType _type
        {
            get { return typeof (ConfirmType).TryParseEnum(Request["type"] ?? "", ConfirmType.EmpInvite); }
        }

        protected EmployeeType _employeeType
        {
            get { return typeof(EmployeeType).TryParseEnum(Request["emplType"] ?? "", EmployeeType.User); }
        }

        protected string GetEmailAddress()
        {
            if (!String.IsNullOrEmpty(Request["emailInput"]))
                return Request["emailInput"].Trim();

            if (!String.IsNullOrEmpty(Request["email"]))
                return Request["email"].Trim();

            return String.Empty;
        }

        private string GetEmailAddress(LoginProfile account)
        {
            var value = GetEmailAddress();
            return String.IsNullOrEmpty(value) ? account.EMail : value;
        }

        protected string GetFirstName()
        {
            var value = string.Empty;
            if (!string.IsNullOrEmpty(Request["firstname"])) value = Request["firstname"].Trim();
            if (!string.IsNullOrEmpty(Request["firstnameInput"])) value = Request["firstnameInput"].Trim();
            return HtmlUtil.GetText(value);
        }

        private string GetFirstName(LoginProfile account)
        {
            var value = GetFirstName();
            return String.IsNullOrEmpty(value) ? account.FirstName : value;
        }

        protected string GetLastName()
        {
            var value = string.Empty;
            if (!string.IsNullOrEmpty(Request["lastname"])) value = Request["lastname"].Trim();
            if (!string.IsNullOrEmpty(Request["lastnameInput"])) value = Request["lastnameInput"].Trim();
            return HtmlUtil.GetText(value);
        }

        private string GetLastName(LoginProfile account)
        {
            var value = GetLastName();
            return String.IsNullOrEmpty(value) ? account.LastName : value;
        }

        protected bool isPersonal {
            get { return CoreContext.Configuration.Personal; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/management/confirminviteactivation/js/confirm_invite_activation.js"));

            Page.RegisterStyleControl(VirtualPathUtility.ToAbsolute("~/usercontrols/management/confirminviteactivation/css/confirm_invite_activation.less"));

            _tenantInfoSettings = SettingsManager.Instance.LoadSettings<TenantInfoSettings>(TenantProvider.CurrentTenantID);

            var uid = Guid.Empty;
            try
            {
                uid = new Guid(Request["uid"]);
            }
            catch
            {
            }

            var email = GetEmailAddress();

            if (_type != ConfirmType.Activation && AccountLinkControl.IsNotEmpty && !CoreContext.Configuration.Personal)
            {
                var thrd = (AccountLinkControl) LoadControl(AccountLinkControl.Location);
                thrd.InviteView = true;
                thrd.ClientCallback = "loginJoinCallback";
                thrdParty.Visible = true;
                thrdParty.Controls.Add(thrd);
            }

            Page.Title = HeaderStringHelper.GetPageTitle(Resource.Authorization);

            UserInfo user;
            try
            {
                SecurityContext.AuthenticateMe(ASC.Core.Configuration.Constants.CoreSystem);

                user = CoreContext.UserManager.GetUserByEmail(email);
                var usr = CoreContext.UserManager.GetUsers(uid);
                if (usr.ID.Equals(ASC.Core.Users.Constants.LostUser.ID) || usr.ID.Equals(ASC.Core.Configuration.Constants.Guest.ID))
                    usr = CoreContext.UserManager.GetUsers(CoreContext.TenantManager.GetCurrentTenant().OwnerId);

                _userAvatar = usr.GetMediumPhotoURL();
                _userName = usr.DisplayUserName(true);
                _userPost = (usr.Title ?? "").HtmlEncode();
            }
            finally
            {
                SecurityContext.Logout();
            }

            if (_type == ConfirmType.LinkInvite || _type == ConfirmType.EmpInvite)
            {
                if (TenantStatisticsProvider.GetUsersCount() >= TenantExtra.GetTenantQuota().ActiveUsers && _employeeType == EmployeeType.User)
                {
                    ShowError(UserControlsCommonResource.TariffUserLimitReason);
                    return;
                }

                if (!user.ID.Equals(ASC.Core.Users.Constants.LostUser.ID))
                {
                    ShowError(CustomNamingPeople.Substitute<Resource>("ErrorEmailAlreadyExists"));
                    return;
                }
            }

            else if (_type == ConfirmType.Activation)
            {
                if (user.IsActive)
                {
                    ShowError(Resource.ErrorConfirmURLError);
                    return;
                }

                if (user.ID.Equals(ASC.Core.Users.Constants.LostUser.ID) || user.Status == EmployeeStatus.Terminated)
                {
                    ShowError(string.Format(Resource.ErrorUserNotFoundByEmail, email));
                    return;
                }
            }

            if (!IsPostBack)
                return;

            var firstName = GetFirstName();
            var lastName = GetLastName();
            var pwd = (Request["pwdInput"] ?? "").Trim();
            var mustChangePassword = false;
            LoginProfile thirdPartyProfile;

            //thirdPartyLogin confirmInvite
            if (Request["__EVENTTARGET"] == "thirdPartyLogin")
            {
                var valueRequest = Request["__EVENTARGUMENT"];
                thirdPartyProfile = new LoginProfile(valueRequest);

                if (!string.IsNullOrEmpty(thirdPartyProfile.AuthorizationError))
                {
                    // ignore cancellation
                    if (thirdPartyProfile.AuthorizationError != "Canceled at provider")
                        ShowError(HttpUtility.HtmlEncode(thirdPartyProfile.AuthorizationError));
                    return;
                }

                if (string.IsNullOrEmpty(thirdPartyProfile.EMail))
                {
                    ShowError(HttpUtility.HtmlEncode(Resource.ErrorNotCorrectEmail));
                    return;
                }
            }

            if (Request["__EVENTTARGET"] == "confirmInvite")
            {
                if (String.IsNullOrEmpty(email))
                {
                    _errorMessage = Resource.ErrorEmptyUserEmail;
                    return;
                }

                if (!email.TestEmailRegex())
                {
                    _errorMessage = Resource.ErrorNotCorrectEmail;
                    return;
                }

                if (String.IsNullOrEmpty(firstName))
                {
                    _errorMessage = Resource.ErrorEmptyUserFirstName;
                    return;
                }

                if (String.IsNullOrEmpty(lastName))
                {
                    _errorMessage = Resource.ErrorEmptyUserLastName;
                    return;
                }

                var checkPassResult = CheckPassword(pwd);
                if (!String.IsNullOrEmpty(checkPassResult))
                {
                    _errorMessage = checkPassResult;
                    return;
                }
            }
            var userID = Guid.Empty;
            try
            {
                SecurityContext.AuthenticateMe(ASC.Core.Configuration.Constants.CoreSystem);
                if (_type == ConfirmType.EmpInvite || _type == ConfirmType.LinkInvite)
                {
                    if (TenantStatisticsProvider.GetUsersCount() >= TenantExtra.GetTenantQuota().ActiveUsers && _employeeType == EmployeeType.User)
                    {
                        ShowError(UserControlsCommonResource.TariffUserLimitReason);
                        return;
                    }

                    UserInfo newUser;
                    if (Request["__EVENTTARGET"] == "confirmInvite")
                    {
                        var fromInviteLink = _type == ConfirmType.LinkInvite;
                        newUser = CreateNewUser(firstName, lastName, email, pwd, _employeeType, fromInviteLink);

                        var messageAction = _employeeType == EmployeeType.User ? MessageAction.UserCreatedViaInvite : MessageAction.GuestCreatedViaInvite;
                        MessageService.Send(HttpContext.Current.Request, MessageInitiator.System, messageAction, newUser.DisplayUserName(false));
                        
                        userID = newUser.ID;
                    }

                    if (Request["__EVENTTARGET"] == "thirdPartyLogin")
                    {
                        if (!String.IsNullOrEmpty(CheckPassword(pwd)))
                        {
                            pwd = UserManagerWrapper.GeneratePassword();
                            mustChangePassword = true;
                        }
                        var valueRequest = Request["__EVENTARGUMENT"];
                        thirdPartyProfile = new LoginProfile(valueRequest);
                        newUser = CreateNewUser(GetFirstName(thirdPartyProfile), GetLastName(thirdPartyProfile), GetEmailAddress(thirdPartyProfile), pwd, _employeeType, false);

                        var messageAction = _employeeType == EmployeeType.User ? MessageAction.UserCreatedViaInvite : MessageAction.GuestCreatedViaInvite;
                        MessageService.Send(HttpContext.Current.Request, MessageInitiator.System, messageAction, newUser.DisplayUserName(false));
                        
                        userID = newUser.ID;
                        if (!String.IsNullOrEmpty(thirdPartyProfile.Avatar))
                        {
                            SaveContactImage(userID, thirdPartyProfile.Avatar);
                        }

                        var linker = new AccountLinker("webstudio");
                        linker.AddLink(userID.ToString(), thirdPartyProfile);
                    }
                }
                else if (_type == ConfirmType.Activation)
                {
                    user.ActivationStatus = EmployeeActivationStatus.Activated;
                    user.FirstName = firstName;
                    user.LastName = lastName;
                    CoreContext.UserManager.SaveUserInfo(user);
                    SecurityContext.SetUserPassword(user.ID, pwd);

                    userID = user.ID;

                    //notify
                    if (user.IsVisitor()) { 
                        StudioNotifyService.Instance.GuestInfoAddedAfterInvite(user, pwd);
                        MessageService.Send(HttpContext.Current.Request, MessageInitiator.System, MessageAction.GuestActivated, user.DisplayUserName(false));
                    }
                    else
                    {
                        StudioNotifyService.Instance.UserInfoAddedAfterInvite(user, pwd);
                        MessageService.Send(HttpContext.Current.Request, MessageInitiator.System, MessageAction.UserActivated, user.DisplayUserName(false));
                    }
                }
            }
            catch (Exception exception)
            {
                _errorMessage = HttpUtility.HtmlEncode(exception.Message);
                return;
            }
            finally
            {
                SecurityContext.Logout();
            }

            user = CoreContext.UserManager.GetUsers(userID);
            try
            {
                var cookiesKey = SecurityContext.AuthenticateMe(user.Email, pwd);
                CookiesManager.SetCookies(CookiesType.AuthKey, cookiesKey);
                MessageService.Send(HttpContext.Current.Request, MessageAction.LoginSuccess);
                StudioNotifyService.Instance.UserHasJoin();

                if (mustChangePassword)
                {
                    StudioNotifyService.Instance.UserPasswordChange(user);
                }
            }
            catch (Exception exception)
            {
                (Page as Confirm).ErrorMessage = HttpUtility.HtmlEncode(exception.Message);
                return;
            }

            UserHelpTourHelper.IsNewUser = true;
            if (CoreContext.Configuration.Personal)
                PersonalSettings.IsNewUser = true;
            Response.Redirect("~/");
        }

        private static void SaveContactImage(Guid userID, string url)
        {
            using (var memstream = new MemoryStream())
            {
                var req = WebRequest.Create(url);
                var response = req.GetResponse();
                var stream = response.GetResponseStream();

                var buffer = new byte[512];
                int bytesRead;
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                    memstream.Write(buffer, 0, bytesRead);
                var bytes = memstream.ToArray();

                UserPhotoManager.SaveOrUpdatePhoto(userID, bytes);
            }
        }

        private void ShowError(string message)
        {
            (Page as Confirm).ErrorMessage = message.Trim();
            (Page as Confirm)._confirmHolder2.Visible = false;
            (Page as Confirm)._confirmHolder.Visible = false;
            (Page as Confirm)._contentWithControl.Visible = true;

            if (SecurityContext.IsAuthenticated == false)
                (Page as Confirm).ErrorMessage +=
                    (message.EndsWith(".") ? "" : ".")
                    + " "
                    + String.Format(Resource.ForSignInFollowMessage,
                                    string.Format("<a href=\"{0}\">",
                                                  VirtualPathUtility.ToAbsolute("~/auth.aspx")),
                                    "</a>");

            _confirmHolder.Visible = false;
        }

        private static string CheckPassword(string pwd)
        {
            if (String.IsNullOrEmpty(pwd))
                return Resource.ErrorPasswordEmpty;

            try
            {
                UserManagerWrapper.CheckPasswordPolicy(pwd);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return String.Empty;
        }

        private static UserInfo CreateNewUser(string firstName, string lastName, string email, string pwd, EmployeeType employeeType, bool fromInviteLink)
        {
            var isVisitor = employeeType == EmployeeType.Visitor;

            if (SetupInfo.IsSecretEmail(email))
            {
                fromInviteLink = false;
            }

            var userInfo = new UserInfo
            {
                FirstName = string.IsNullOrEmpty(firstName) ? UserControlsCommonResource.UnknownFirstName : firstName,
                LastName = string.IsNullOrEmpty(lastName) ? UserControlsCommonResource.UnknownLastName : lastName,
                Email = email,
            };

            if (CoreContext.Configuration.Personal)
            {
                userInfo.ActivationStatus = EmployeeActivationStatus.Activated;
                userInfo.CultureName = Thread.CurrentThread.CurrentUICulture.Name;
            }

            return UserManagerWrapper.AddUser(userInfo, pwd, true, true, isVisitor, fromInviteLink);
        }
    }
}