/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
using System.Web;
using AjaxPro;
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
using log4net;
using Resources;
using LogoutSsoUserData = ASC.Web.Studio.UserControls.Management.SingleSignOnSettings.LogoutSsoUserData;
using SsoSettingsV2 = ASC.Web.Studio.UserControls.Management.SingleSignOnSettings.SsoSettingsV2;
using SsoUserData = ASC.Web.Studio.UserControls.Management.SingleSignOnSettings.SsoUserData;

namespace ASC.Web.Studio.HttpHandlers
{
    public class SsoHandler : IHttpHandler
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(SsoHandler));
        private const string AUTH_PAGE = "~/auth.aspx";

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
                    context.Response.Redirect(AUTH_PAGE + "?m=" + HttpUtility.UrlEncode(Resource.SsoSettingsDisabled),
                        false);
                    return;
                }
                if (CoreContext.Configuration.Standalone &&
                    !CoreContext.TenantManager.GetTenantQuota(TenantProvider.CurrentTenantID).Sso)
                {
                    _log.DebugFormat("Single sign-on settings are not paid");
                    context.Response.Redirect(
                        AUTH_PAGE + "?m=" + HttpUtility.UrlEncode(Resource.ErrorNotAllowedOption), false);
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
                    context.Response.Redirect(AUTH_PAGE + "?m=" + HttpUtility.UrlEncode(Resource.SsoSettingsDisabled),
                        false);
                    return;
                }

                var data = context.Request["data"];

                if (string.IsNullOrEmpty(data))
                {
                    _log.Error("SAML response is null or empty");
                    context.Response.Redirect(
                        AUTH_PAGE + "?m=" + HttpUtility.UrlEncode(Resource.SsoSettingsEmptyToken), false);
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
                            AUTH_PAGE + "?m=" + HttpUtility.UrlEncode(Resource.SsoSettingsNotValidToken), false);
                        return;
                    }

                    var userInfo = userData.ToUserInfo(true);

                    if (Equals(userInfo, Constants.LostUser))
                    {
                        _log.Error("Can't create userInfo using current SAML response");
                        context.Response.Redirect(
                            AUTH_PAGE + "?m=" + HttpUtility.UrlEncode(Resource.SsoSettingsCantCreateUser), false);
                        return;
                    }

                    if (userInfo.Status == EmployeeStatus.Terminated)
                    {
                        _log.Error("Current user is terminated");
                        context.Response.Redirect(
                            AUTH_PAGE + "?m=" + HttpUtility.UrlEncode(Resource.SsoSettingsUserTerminated), false);
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
                            AUTH_PAGE + "?m=" + HttpUtility.UrlEncode(Resource.SsoSettingsNotValidToken), false);
                        return;
                    }

                    var userInfo = CoreContext.UserManager.GetSsoUserByNameId(logoutSsoUserData.NameId);

                    if (Equals(userInfo, Constants.LostUser))
                    {
                        _log.Error("Can't logout userInfo using current SAML response");
                        context.Response.Redirect(
                            AUTH_PAGE + "?m=" + HttpUtility.UrlEncode(Resource.SsoSettingsCantCreateUser), false);
                        return;
                    }

                    if (userInfo.Status == EmployeeStatus.Terminated)
                    {
                        _log.Error("Current user is terminated");
                        context.Response.Redirect(
                            AUTH_PAGE + "?m=" + HttpUtility.UrlEncode(Resource.SsoSettingsUserTerminated), false);
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
                context.Response.Redirect(AUTH_PAGE + "?m=" + HttpUtility.UrlEncode(e.Message), false);
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
                    if (string.IsNullOrWhiteSpace(newUserInfo.FirstName))
                    {
                        newUserInfo.FirstName = Resource.FirstName;
                    }

                    if (string.IsNullOrWhiteSpace(newUserInfo.LastName))
                    {
                        newUserInfo.LastName = Resource.LastName;
                    }

                    if (TenantStatisticsProvider.GetUsersCount() < TenantExtra.GetTenantQuota().ActiveUsers)
                    {
                        newUserInfo = UserManagerWrapper.AddUser(newUserInfo, UserManagerWrapper.GeneratePassword(), true,
                            false);
                    }
                    else
                    {
                        newUserInfo = UserManagerWrapper.AddUser(newUserInfo, UserManagerWrapper.GeneratePassword(), true,
                            false, true);
                    }
                }
                else
                {
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
