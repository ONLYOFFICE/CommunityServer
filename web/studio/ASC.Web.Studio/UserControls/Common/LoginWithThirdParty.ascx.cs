/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using System.Threading;
using System.Web;
using System.Web.UI;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
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
using log4net;

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

            var loginProfile = Request.Url.GetProfile();

            if (loginProfile == null && !IsPostBack || SecurityContext.IsAuthenticated) return;

            try
            {
                if (loginProfile == null)
                {
                    if (string.IsNullOrEmpty(Request["__EVENTARGUMENT"]) || Request["__EVENTTARGET"] != "thirdPartyLogin")
                    {
                        return;
                    }

                    loginProfile = new LoginProfile(Request["__EVENTARGUMENT"]);
                }

                var userInfo = GetUserByThirdParty(loginProfile);
                if (!CoreContext.UserManager.UserExists(userInfo.ID)) return;

                var cookiesKey = SecurityContext.AuthenticateMe(userInfo.ID);
                CookiesManager.SetCookies(CookiesType.AuthKey, cookiesKey);
                MessageService.Send(HttpContext.Current.Request, MessageAction.LoginSuccessViaSocialAccount);
            }
            catch (System.Security.SecurityException)
            {
                LoginMessage = Resource.InvalidUsernameOrPassword;
                MessageService.Send(HttpContext.Current.Request, loginProfile != null ? loginProfile.EMail : AuditResource.EmailNotSpecified, MessageAction.LoginFailDisabledProfile);
                return;
            }
            catch (Exception exception)
            {
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

        public static UserInfo GetUserByThirdParty(LoginProfile loginProfile)
        {
            try
            {
                if (!string.IsNullOrEmpty(loginProfile.AuthorizationError))
                {
                    // ignore cancellation
                    if (loginProfile.AuthorizationError != "Canceled at provider")
                    {
                        throw new Exception(loginProfile.AuthorizationError);
                    }
                    return ASC.Core.Users.Constants.LostUser;
                }

                if (string.IsNullOrEmpty(loginProfile.EMail))
                {
                    throw new Exception(Resource.ErrorNotCorrectEmail);
                }

                var userInfo = new UserInfo();

                Guid userId;
                if (TryGetUserByHash(loginProfile.HashId, out userId))
                {
                    userInfo = CoreContext.UserManager.GetUsers(userId);
                }
                if (!CoreContext.UserManager.UserExists(userInfo.ID))
                {
                    userInfo = CoreContext.UserManager.GetUserByEmail(loginProfile.EMail);
                }

                var isNew = false;
                if (CoreContext.Configuration.Personal)
                {
                    if (CoreContext.UserManager.UserExists(userInfo.ID) && SetupInfo.IsSecretEmail(userInfo.Email))
                    {
                        try
                        {
                            SecurityContext.AuthenticateMe(ASC.Core.Configuration.Constants.CoreSystem);
                            CoreContext.UserManager.DeleteUser(userInfo.ID);
                            userInfo = ASC.Core.Users.Constants.LostUser;
                        }
                        finally
                        {
                            SecurityContext.Logout();
                        }
                    }

                    if (!CoreContext.UserManager.UserExists(userInfo.ID))
                    {
                        userInfo = JoinByThirdPartyAccount(loginProfile);

                        isNew = true;
                    }
                }

                if (isNew)
                {
                    var spam = HttpContext.Current.Request["spam"];
                    if (spam != "on")
                    {
                        try
                        {
                            const string _databaseID = "com";
                            using (var db = DbManager.FromHttpContext(_databaseID))
                            {
                                db.ExecuteNonQuery(new SqlInsert("template_unsubscribe", false)
                                        .InColumnValue("email",userInfo.Email.ToLowerInvariant())
                                        .InColumnValue("reason", "personal")
                                    );
                                LogManager.GetLogger("ASC.Web").Debug(String.Format("Write to template_unsubscribe {0}", userInfo.Email.ToLowerInvariant()));
                            }
                        }
                        catch (Exception ex)
                        {
                            LogManager.GetLogger("ASC.Web").Debug(String.Format("ERROR write to template_unsubscribe {0}, email:{1}", ex.Message, userInfo.Email.ToLowerInvariant()));
                        }
                    }
                    StudioNotifyService.Instance.UserHasJoin();
                    UserHelpTourHelper.IsNewUser = true;
                    PersonalSettings.IsNewUser = true;
                }

                return userInfo;
            }
            catch (Exception)
            {
                Auth.ProcessLogout();
                throw;
            }
        }

        public static bool TryGetUserByHash(string hashId, out Guid userId)
        {
            userId = Guid.Empty;
            if (string.IsNullOrEmpty(hashId)) return false;

            var linkedProfiles = new AccountLinker("webstudio").GetLinkedObjectsByHashId(hashId);
            var tmp = Guid.Empty;
            if (linkedProfiles.Any(profileId => Guid.TryParse(profileId, out tmp) && CoreContext.UserManager.UserExists(tmp)))
                userId = tmp;
            return true;
        }

        public static UserInfo ProfileToUserInfo(LoginProfile loginProfile)
        {
            if (string.IsNullOrEmpty(loginProfile.EMail)) throw new Exception(Resource.ErrorNotCorrectEmail);

            var firstName = loginProfile.FirstName;
            if (string.IsNullOrEmpty(firstName)) firstName = loginProfile.DisplayName;

            var userInfo = new UserInfo
                {
                    FirstName = string.IsNullOrEmpty(firstName) ? UserControlsCommonResource.UnknownFirstName : firstName,
                    LastName = string.IsNullOrEmpty(loginProfile.LastName) ? UserControlsCommonResource.UnknownLastName : loginProfile.LastName,
                    Email = loginProfile.EMail,
                    Title = string.Empty,
                    Location = string.Empty,
                    CultureName = Thread.CurrentThread.CurrentUICulture.Name,
                    ActivationStatus = EmployeeActivationStatus.Activated,
                };

            var gender = loginProfile.Gender;
            if (!string.IsNullOrEmpty(gender))
            {
                userInfo.Sex = gender == "male";
            }

            return userInfo;
        }

        private static UserInfo JoinByThirdPartyAccount(LoginProfile loginProfile)
        {
            var userInfo = ProfileToUserInfo(loginProfile);

            UserInfo newUserInfo;
            try
            {
                SecurityContext.AuthenticateMe(ASC.Core.Configuration.Constants.CoreSystem);
                newUserInfo = UserManagerWrapper.AddUser(userInfo, UserManagerWrapper.GeneratePassword());
            }
            finally
            {
                SecurityContext.Logout();
            }

            var linker = new AccountLinker("webstudio");
            linker.AddLink(newUserInfo.ID.ToString(), loginProfile);

            return newUserInfo;
        }
    }
}