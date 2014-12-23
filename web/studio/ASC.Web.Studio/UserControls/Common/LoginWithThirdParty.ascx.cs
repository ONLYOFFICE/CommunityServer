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
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI;
using ASC.Core;
using ASC.Core.Users;
using ASC.FederatedLogin;
using ASC.FederatedLogin.Profile;
using ASC.MessagingSystem;
using ASC.Web.Core;
using ASC.Web.Core.Users;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.UserControls.Users.UserProfile;
using ASC.Web.Studio.Utility;
using Resources;

namespace ASC.Web.Studio.UserControls.Common
{
    public partial class LoginWithThirdParty : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Common/LoginWithThirdParty.ascx"; }
        }

        protected string LoginMessage;

        protected void Page_Load(object sender, EventArgs e)
        {
            var accountLink = (AccountLinkControl) LoadControl(AccountLinkControl.Location);
            accountLink.ClientCallback = "loginJoinCallback";
            accountLink.SettingsView = false;
            ThirdPartyList.Controls.Add(accountLink);

            var thirdPartyProfile = Request.Url.GetProfile();

            if (thirdPartyProfile == null && !IsPostBack || SecurityContext.IsAuthenticated) return;

            try
            {
                if (thirdPartyProfile == null)
                {
                    if (string.IsNullOrEmpty(Request["__EVENTARGUMENT"]) || Request["__EVENTTARGET"] != "thirdPartyLogin")
                    {
                        return;
                    }

                    thirdPartyProfile = new LoginProfile(Request["__EVENTARGUMENT"]);
                }

                if (!string.IsNullOrEmpty(thirdPartyProfile.AuthorizationError))
                {
                    // ignore cancellation
                    if (thirdPartyProfile.AuthorizationError != "Canceled at provider")
                    {
                        LoginMessage = thirdPartyProfile.AuthorizationError;
                    }
                    return;
                }

                if (string.IsNullOrEmpty(thirdPartyProfile.EMail))
                {
                    LoginMessage = Resource.ErrorNotCorrectEmail;
                    return;
                }

                var userId = Guid.Empty;
                var linkedProfiles = accountLink.GetLinker().GetLinkedObjectsByHashId(thirdPartyProfile.HashId);
                linkedProfiles.Any(profileId => Guid.TryParse(profileId, out userId) && CoreContext.UserManager.UserExists(userId));

                var acc = CoreContext.UserManager.GetUsers(userId);

                if (!CoreContext.UserManager.UserExists(acc.ID))
                {
                    acc = CoreContext.UserManager.GetUserByEmail(thirdPartyProfile.EMail);
                }

                var isNew = false;
                if (CoreContext.Configuration.Personal)
                {
                    if (CoreContext.UserManager.UserExists(acc.ID) && SetupInfo.IsSecretEmail(acc.Email))
                    {
                        try
                        {
                            SecurityContext.AuthenticateMe(ASC.Core.Configuration.Constants.CoreSystem);
                            CoreContext.UserManager.DeleteUser(acc.ID);
                            acc = ASC.Core.Users.Constants.LostUser;
                        }
                        finally
                        {
                            SecurityContext.Logout();
                        }
                    }

                    if (!CoreContext.UserManager.UserExists(acc.ID))
                    {
                        acc = JoinByThirdPartyAccount(thirdPartyProfile);

                        isNew = true;
                    }
                }

                var cookiesKey = SecurityContext.AuthenticateMe(acc.ID);
                CookiesManager.SetCookies(CookiesType.AuthKey, cookiesKey);
                MessageService.Send(HttpContext.Current.Request, MessageAction.LoginSuccessViaSocialAccount);

                if (isNew)
                {
                    StudioNotifyService.Instance.UserHasJoin();
                    UserHelpTourHelper.IsNewUser = true;
                    PersonalSettings.IsNewUser = true;
                }
            }
            catch (System.Security.SecurityException)
            {
                Auth.ProcessLogout();
                LoginMessage = Resource.InvalidUsernameOrPassword;
                MessageService.Send(HttpContext.Current.Request, thirdPartyProfile != null ? thirdPartyProfile.HashId : AuditResource.EmailNotSpecified, MessageAction.LoginFailDisabledProfile);
                return;
            }
            catch (Exception exception)
            {
                Auth.ProcessLogout();
                LoginMessage = exception.Message;
                MessageService.Send(HttpContext.Current.Request, AuditResource.EmailNotSpecified, MessageAction.LoginFail);
                return;
            }

            var refererURL = (string) Session["refererURL"];

            if (String.IsNullOrEmpty(refererURL))
                Response.Redirect(CommonLinkUtility.GetDefault());
            else
            {
                Session["refererURL"] = null;
                Response.Redirect(refererURL);
            }
        }

        private static UserInfo JoinByThirdPartyAccount(LoginProfile thirdPartyProfile)
        {
            if (string.IsNullOrEmpty(thirdPartyProfile.EMail)) throw new Exception(Resource.ErrorNotCorrectEmail);

            var firstName = thirdPartyProfile.FirstName;
            if (string.IsNullOrEmpty(firstName)) firstName = thirdPartyProfile.DisplayName;

            var userInfo = new UserInfo
                {
                    FirstName = string.IsNullOrEmpty(firstName) ? UserControlsCommonResource.UnknownFirstName : firstName,
                    LastName = string.IsNullOrEmpty(thirdPartyProfile.LastName) ? UserControlsCommonResource.UnknownLastName : thirdPartyProfile.LastName,
                    Email = thirdPartyProfile.EMail,
                    Title = string.Empty,
                    Location = string.Empty,
                    CultureName = Thread.CurrentThread.CurrentUICulture.Name,
                    ActivationStatus = EmployeeActivationStatus.Activated,
                };

            var pwd = UserManagerWrapper.GeneratePassword();

            UserInfo newUserInfo;
            try
            {
                SecurityContext.AuthenticateMe(ASC.Core.Configuration.Constants.CoreSystem);
                newUserInfo = UserManagerWrapper.AddUser(userInfo, pwd);
            }
            finally
            {
                SecurityContext.Logout();
            }

            var linker = new AccountLinker("webstudio");
            linker.AddLink(newUserInfo.ID.ToString(), thirdPartyProfile);

            return newUserInfo;
        }
    }
}