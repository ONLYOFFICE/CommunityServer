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
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI;

using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Logging;
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
using ASC.Web.Studio.PublicResources;
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

        private static ILog Log = LogManager.GetLogger("ASC.Web");

        protected string LoginMessage;
        public bool RenderDisabled { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            var accountLink = (AccountLinkControl)LoadControl(AccountLinkControl.Location);
            accountLink.ClientCallback = "loginJoinCallback";
            accountLink.SettingsView = false;
            accountLink.RenderDisabled = RenderDisabled;
            ThirdPartyList.Controls.Add(accountLink);

            var loginProfile = Request.Url.GetProfile();

            if (loginProfile == null && !IsPostBack || SecurityContext.IsAuthenticated) return;

            string cookiesKey = string.Empty;
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

                cookiesKey = CookiesManager.AuthenticateMeAndSetCookies(userInfo.Tenant, userInfo.ID, MessageAction.LoginSuccessViaSocialAccount);
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

            var refererURL = Context.GetRefererURL();
            Response.Redirect(
                              Request.DesktopApp()
                              ? refererURL + ((refererURL.Contains("?") ? "&" : "?") + "token=" + HttpUtility.HtmlEncode(cookiesKey))
                              : refererURL);
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
                    return Constants.LostUser;
                }

                var userInfo = Constants.LostUser;

                Guid userId;
                if (TryGetUserByHash(loginProfile.HashId, out userId))
                {
                    userInfo = CoreContext.UserManager.GetUsers(userId);
                }

                var isNew = false;
                if (CoreContext.Configuration.Personal)
                {
                    if (CoreContext.UserManager.UserExists(userInfo.ID) && SetupInfo.IsSecretEmail(userInfo.Email))
                    {
                        try
                        {
                            SecurityContext.CurrentAccount = ASC.Core.Configuration.Constants.CoreSystem;
                            CoreContext.UserManager.DeleteUser(userInfo.ID);
                            userInfo = Constants.LostUser;
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
                            using (var db = new DbManager(_databaseID))
                            {
                                db.ExecuteNonQuery(new SqlInsert("template_unsubscribe", false)
                                                       .InColumnValue("email", userInfo.Email.ToLowerInvariant())
                                                       .InColumnValue("reason", "personal")
                                    );
                                Log.Debug(String.Format("Write to template_unsubscribe {0}", userInfo.Email.ToLowerInvariant()));
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Debug(String.Format("ERROR write to template_unsubscribe {0}, email:{1}", ex.Message, userInfo.Email.ToLowerInvariant()));
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
                CultureName = CoreContext.Configuration.CustomMode ? "ru-RU" : Thread.CurrentThread.CurrentUICulture.Name,
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
            if (string.IsNullOrEmpty(loginProfile.EMail))
            {
                throw new Exception(Resource.ErrorNotCorrectEmail);
            }

            var userInfo = CoreContext.UserManager.GetUserByEmail(loginProfile.EMail);
            if (!CoreContext.UserManager.UserExists(userInfo.ID))
            {
                var newUserInfo = ProfileToUserInfo(loginProfile);

                try
                {
                    SecurityContext.CurrentAccount = ASC.Core.Configuration.Constants.CoreSystem;
                    userInfo = UserManagerWrapper.AddUser(newUserInfo, UserManagerWrapper.GeneratePassword());
                }
                finally
                {
                    SecurityContext.Logout();
                }
            }

            var linker = new AccountLinker("webstudio");
            linker.AddLink(userInfo.ID.ToString(), loginProfile);

            return userInfo;
        }
    }
}