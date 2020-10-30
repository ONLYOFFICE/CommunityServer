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
using AjaxPro;
using ASC.Common.Logging;
using ASC.Common.Security.Authentication;
using ASC.Common.Utils;
using ASC.Core;
using ASC.Core.Users;
using ASC.MessagingSystem;
using ASC.Web.Core;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.UserControls.Statistics;
using ASC.Web.Studio.Utility;
using Resources;
using LogoutSsoUserData = ASC.Web.Studio.UserControls.Management.SingleSignOnSettings.LogoutSsoUserData;
using SsoSettingsV2 = ASC.Web.Studio.UserControls.Management.SingleSignOnSettings.SsoSettingsV2;
using SsoUserData = ASC.Web.Studio.UserControls.Management.SingleSignOnSettings.SsoUserData;

namespace ASC.Web.Studio.HttpHandlers
{
    public class SsoHandler : IHttpHandler
    {
        private readonly ILog _log = LogManager.GetLogger("ASC");
        private const string AUTH_PAGE = "~/Auth.aspx";

        #region IHttpHandler Members

        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            try
            {
                if (!SetupInfo.IsVisibleSettings(ManagementType.SingleSignOnSettings.ToString()))
                {
                    _log.DebugFormat("Single sign-on settings are disabled");
                    context.Response.Redirect(AUTH_PAGE + "?am=" + (int)Auth.MessageKey.SsoSettingsDisabled,
                        false);
                    return;
                }
                if (CoreContext.Configuration.Standalone &&
                    !CoreContext.TenantManager.GetTenantQuota(TenantProvider.CurrentTenantID).Sso)
                {
                    _log.DebugFormat("Single sign-on settings are not paid");
                    context.Response.Redirect(
                        AUTH_PAGE + "?am=" + (int)Auth.MessageKey.ErrorNotAllowedOption, false);
                    return;
                }
                var settings = SsoSettingsV2.Load();

                if (context.Request["config"] == "saml")
                {
                    context.Response.StatusCode = 200;
                    var signedSettings = Signature.Create(settings);
                    var ssoConfig = JavaScriptSerializer.Serialize(signedSettings);
                    context.Response.Write(ssoConfig.Replace("\"", ""));
                    return;
                }

                if (!settings.EnableSso)
                {
                    _log.DebugFormat("Single sign-on is disabled");
                    context.Response.Redirect(AUTH_PAGE + "?am=" + (int)Auth.MessageKey.SsoSettingsDisabled,
                        false);
                    return;
                }

                var data = context.Request["data"];

                if (string.IsNullOrEmpty(data))
                {
                    _log.Error("SAML response is null or empty");
                    context.Response.Redirect(
                        AUTH_PAGE + "?am=" + (int)Auth.MessageKey.SsoSettingsEmptyToken, false);
                    return;
                }

                if (context.Request["auth"] == "true")
                {
                    var userData = Signature.Read<SsoUserData>(data);

                    if (userData == null)
                    {
                        _log.Error("SAML response is not valid");
                        MessageService.Send(context.Request, MessageAction.LoginFailViaSSO);
                        context.Response.Redirect(
                            AUTH_PAGE + "?am=" + (int)Auth.MessageKey.SsoSettingsNotValidToken, false);
                        return;
                    }

                    var userInfo = userData.ToUserInfo(true);

                    if (Equals(userInfo, Constants.LostUser))
                    {
                        _log.Error("Can't create userInfo using current SAML response (fields Email, FirstName, LastName are required)");
                        context.Response.Redirect(
                            AUTH_PAGE + "?am=" + (int)Auth.MessageKey.SsoSettingsCantCreateUser, false);
                        return;
                    }

                    if (userInfo.Status == EmployeeStatus.Terminated)
                    {
                        _log.Error("Current user is terminated");
                        context.Response.Redirect(
                            AUTH_PAGE + "?am=" + (int)Auth.MessageKey.SsoSettingsUserTerminated, false);
                        return;
                    }

                    if (context.User != null && context.User.Identity != null && context.User.Identity.IsAuthenticated)
                    {
                       var authenticatedUserInfo = CoreContext.UserManager.GetUsers(((IUserAccount)context.User.Identity).ID);

                        if (!Equals(userInfo, authenticatedUserInfo))
                        {
                            var loginName = authenticatedUserInfo.DisplayUserName(false);
                            CookiesManager.ResetUserCookie();
                            SecurityContext.Logout();
                            MessageService.Send(HttpContext.Current.Request, loginName, MessageAction.Logout);
                        }
                        else
                        {
                            _log.DebugFormat("User {0} already authenticated", context.User.Identity);
                        }
                    }

                    userInfo = AddUser(userInfo);

                    var authKey = SecurityContext.AuthenticateMe(userInfo.ID);

                    CookiesManager.SetCookies(CookiesType.AuthKey, authKey);

                    MessageService.Send(context.Request, MessageAction.LoginSuccessViaSSO);

                    context.Response.Redirect(CommonLinkUtility.GetDefault() + "?token=" + HttpUtility.UrlEncode(authKey), false);
                }
                else if (context.Request["logout"] == "true")
                {
                    var logoutSsoUserData = Signature.Read<LogoutSsoUserData>(data);

                    if (logoutSsoUserData == null)
                    {
                        _log.Error("SAML Logout response is not valid");
                        MessageService.Send(context.Request, MessageAction.LoginFailViaSSO);
                        context.Response.Redirect(
                            AUTH_PAGE + "?am=" + (int)Auth.MessageKey.SsoSettingsNotValidToken, false);
                        return;
                    }

                    var userInfo = CoreContext.UserManager.GetSsoUserByNameId(logoutSsoUserData.NameId);

                    if (Equals(userInfo, Constants.LostUser))
                    {
                        _log.Error("Can't logout userInfo using current SAML response");
                        context.Response.Redirect(
                            AUTH_PAGE + "?am=" + (int)Auth.MessageKey.SsoSettingsCantCreateUser, false);
                        return;
                    }

                    if (userInfo.Status == EmployeeStatus.Terminated)
                    {
                        _log.Error("Current user is terminated");
                        context.Response.Redirect(
                            AUTH_PAGE + "?am=" + (int)Auth.MessageKey.SsoSettingsUserTerminated, false);
                        return;
                    }

                    SecurityContext.AuthenticateMe(userInfo.ID);

                    var loginName = userInfo.DisplayUserName(false);

                    CookiesManager.ResetUserCookie();
                    SecurityContext.Logout();

                    MessageService.Send(HttpContext.Current.Request, loginName, MessageAction.Logout);
                    context.Response.Redirect(AUTH_PAGE, false);
                }
            }
            catch (Exception e)
            {
                _log.ErrorFormat("Unexpected error. {0}", e);
                context.Response.Redirect(AUTH_PAGE + "?am=" + (int)Auth.MessageKey.Error, false);
            }
            finally
            {
                context.ApplicationInstance.CompleteRequest();
            }
        }

        #endregion

        private UserInfo AddUser(UserInfo userInfo)
        {
            UserInfo newUserInfo;

            try
            {
                newUserInfo = userInfo.Clone() as UserInfo;

                if(newUserInfo == null)
                    return Constants.LostUser;

                _log.DebugFormat("Adding or updating user in database, userId={0}", userInfo.ID);

                SecurityContext.AuthenticateMe(ASC.Core.Configuration.Constants.CoreSystem);

                if (string.IsNullOrEmpty(newUserInfo.UserName))
                {
                    var limitExceeded = TenantStatisticsProvider.GetUsersCount() >= TenantExtra.GetTenantQuota().ActiveUsers;

                    newUserInfo = UserManagerWrapper.AddUser(newUserInfo, UserManagerWrapper.GeneratePassword(), true,
                        false, isVisitor: limitExceeded);
                }
                else
                {
                    if (!UserFormatter.IsValidUserName(userInfo.FirstName, userInfo.LastName))
                        throw new Exception(Resource.ErrorIncorrectUserName);

                    CoreContext.UserManager.SaveUserInfo(newUserInfo);
                }

                /*var photoUrl = samlResponse.GetRemotePhotoUrl();
                if (!string.IsNullOrEmpty(photoUrl))
                {
                    var photoLoader = new UserPhotoLoader();
                    photoLoader.SaveOrUpdatePhoto(photoUrl, userInfo.ID);
                }*/
            }
            finally
            {
                SecurityContext.Logout();
            }

            return newUserInfo;
        }
    }
}
