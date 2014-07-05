/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using ASC.Core;
using ASC.Core.Users;
using ASC.FederatedLogin;
using ASC.FederatedLogin.Profile;
using ASC.Web.Core;
using ASC.Web.Core.Users;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.UserControls.Users.UserProfile;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.UserControls.Common
{
    public partial class LoginWithThirdParty : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Common/LoginWithThirdParty.ascx"; }
        }

        public bool FromEditor;

        private string _loginMessage;

        protected string LoginMessage
        {
            get
            {
                return string.IsNullOrEmpty(_loginMessage)
                           ? string.Empty
                           : "<div class=\"errorBox\">" + _loginMessage.HtmlEncode() + "</div>";
            }
            set { _loginMessage = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            var accountLink = (AccountLinkControl)LoadControl(AccountLinkControl.Location);
            accountLink.ClientCallback = "loginJoinCallback";
            accountLink.SettingsView = false;
            ThirdPartyList.Controls.Add(accountLink);

            _loginMessage = Request["m"];

            var thirdPartyProfile = Request.Url.GetProfile();

            if (thirdPartyProfile == null && !IsPostBack || SecurityContext.IsAuthenticated) return;

            try
            {
                if (thirdPartyProfile == null)
                {
                    if (string.IsNullOrEmpty(Request["__EVENTARGUMENT"]) || Request["__EVENTTARGET"] != "thirdPartyLogin")
                    {
                        LoginMessage = "<div class=\"errorBox\">" + HttpUtility.HtmlEncode(Resources.Resource.InvalidUsernameOrPassword) + "</div>";
                        return;
                    }

                    thirdPartyProfile = new LoginProfile(Request["__EVENTARGUMENT"]);
                }

                if (!string.IsNullOrEmpty(thirdPartyProfile.AuthorizationError))
                {
                    // ignore cancellation
                    if (thirdPartyProfile.AuthorizationError != "Canceled at provider")
                    {
                        _loginMessage = thirdPartyProfile.AuthorizationError;
                    }
                    return;
                }

                if (string.IsNullOrEmpty(thirdPartyProfile.EMail))
                {
                    _loginMessage = Resources.Resource.ErrorNotCorrectEmail;
                    return;
                }

                var cookiesKey = string.Empty;
                var accounts = accountLink.GetLinker().GetLinkedObjectsByHashId(thirdPartyProfile.HashId);

                foreach (var account in accounts.Select(x =>
                                                            {
                                                                try
                                                                {
                                                                    return new Guid(x);
                                                                }
                                                                catch
                                                                {
                                                                    return Guid.Empty;
                                                                }
                                                            }))
                {
                    if (account == Guid.Empty || !CoreContext.UserManager.UserExists(account)) continue;

                    var coreAcc = CoreContext.UserManager.GetUsers(account);
                    cookiesKey = SecurityContext.AuthenticateMe(coreAcc.ID);
                }

                if (string.IsNullOrEmpty(cookiesKey))
                {
                    var emailAcc = CoreContext.UserManager.GetUserByEmail(thirdPartyProfile.EMail);
                    if (CoreContext.UserManager.UserExists(emailAcc.ID))
                    {
                        cookiesKey = SecurityContext.AuthenticateMe(emailAcc.ID);
                    }
                }

                if (CoreContext.Configuration.Personal && string.IsNullOrEmpty(cookiesKey))
                {
                    cookiesKey = JoinByThirdPartyAccount(thirdPartyProfile);

                    UserHelpTourHelper.IsNewUser = true;
                    PersonalSettings.IsNewUser = true;
                }

                CookiesManager.SetCookies(CookiesType.AuthKey, cookiesKey);
            }
            catch (System.Security.SecurityException)
            {
                Auth.ProcessLogout();
                _loginMessage = Resources.Resource.InvalidUsernameOrPassword;
                return;
            }
            catch (Exception exception)
            {
                Auth.ProcessLogout();
                _loginMessage = exception.Message;
                return;
            }

            var refererURL = (string)Session["refererURL"];

            var addMember = Request.Form["additionalMember"];
            if (!string.IsNullOrEmpty(addMember))
            {
                refererURL = addMember;
            }

            if (String.IsNullOrEmpty(refererURL))
                Response.Redirect(CommonLinkUtility.GetDefault());
            else
            {
                Session["refererURL"] = null;
                Response.Redirect(refererURL);
            }
        }

        private static string JoinByThirdPartyAccount(LoginProfile thirdPartyProfile)
        {
            var userInfo = new UserInfo
                {
                    Status = EmployeeStatus.Active,
                    FirstName = string.IsNullOrEmpty(thirdPartyProfile.FirstName) ? Resources.UserControlsCommonResource.UnknownFirstName : thirdPartyProfile.FirstName,
                    LastName = string.IsNullOrEmpty(thirdPartyProfile.LastName) ? Resources.UserControlsCommonResource.UnknownLastName : thirdPartyProfile.LastName,
                    Email = thirdPartyProfile.EMail,
                    Title = string.Empty,
                    Location = string.Empty,
                    WorkFromDate = ASC.Core.Tenants.TenantUtil.DateTimeNow(),
                };

            var cultureInfo = SetupInfo.EnabledCultures.Find(c => String.Equals(c.TwoLetterISOLanguageName, CultureInfo.CurrentUICulture.Name, StringComparison.InvariantCultureIgnoreCase));
            if (cultureInfo != null)
            {
                userInfo.CultureName = cultureInfo.Name;
            }

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

            return SecurityContext.AuthenticateMe(newUserInfo.ID);
        }
    }
}