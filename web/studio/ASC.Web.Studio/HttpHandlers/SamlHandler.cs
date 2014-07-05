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

using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.MessagingSystem;
using ASC.SingleSignOn.Common;
using ASC.SingleSignOn.Saml;
using ASC.Web.Core;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.SMS;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.UserControls.Statistics;
using ASC.Web.Studio.Utility;
using log4net;
using Resources;
using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Net.Mail;
using System.Web;

namespace ASC.Web.Studio.HttpHandlers
{
    public class SamlHandler : IHttpHandler
    {
        private readonly static ILog _log = LogManager.GetLogger(typeof(SamlHandler));
        private const string AUTH_PAGE = "~/auth.aspx";
        private const string SAML_RESPONSE = "SAMLResponse";
        private const string SAML = "saml";
        private const string PASSWORD = "password";

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
                    context.Response.Redirect(AUTH_PAGE + "?m=" + HttpUtility.UrlEncode(Resource.SsoSettingsDisabled), false);
                    context.ApplicationInstance.CompleteRequest();
                    return;
                }
                var settings = SettingsManager.Instance.LoadSettings<SsoSettings>(TenantProvider.CurrentTenantID);
                if (!settings.EnableSso)
                {
                    _log.DebugFormat("Single sign-on is disabled");
                    context.Response.Redirect(AUTH_PAGE + "?m=" + HttpUtility.UrlEncode(Resource.SsoSettingsDisabled), false);
                    context.ApplicationInstance.CompleteRequest();
                    return;
                }

                if (context.User.Identity.IsAuthenticated)
                {
                    _log.DebugFormat("User {0} already authenticated");
                    context.Response.Redirect(CommonLinkUtility.GetDefault(), false);
                    context.ApplicationInstance.CompleteRequest();
                    return;
                }

                UserInfo userInfo;
                if (settings.TokenType != TokenTypes.SAML)
                {
                    _log.Error("Settings TokenType  is not SAML");
                    context.Response.Redirect(AUTH_PAGE + "?m=" + HttpUtility.UrlEncode(Resource.SsoSettingsEnexpectedTokenType), false);
                    context.ApplicationInstance.CompleteRequest();
                    return;
                }

                if (context.Request["auth"] == "true")
                {
                    SamlRequest req = new SamlRequest(settings);
                    string assertionConsumerServiceUrl = context.Request.Url.AbsoluteUri.Substring(0,
                        context.Request.Url.AbsoluteUri.IndexOf("?"));
                    context.Response.Redirect(settings.SsoEndPoint + "?" +
                        req.GetRequest(SamlRequestFormat.Base64,
                            assertionConsumerServiceUrl,
                            Path.Combine(context.Request.PhysicalApplicationPath, "App_Data\\certificates\\sp.pfx"),
                            ConfigurationManager.AppSettings["saml.request.certificate.password"] ?? PASSWORD),
                            false);
                    context.ApplicationInstance.CompleteRequest();
                    return;
                }

                var samlEncodedString = context.Request.Form[SAML_RESPONSE];
                if (string.IsNullOrWhiteSpace(samlEncodedString))
                {
                    _log.Error("SAML response is null or empty");
                    context.Response.Redirect(AUTH_PAGE + "?m=" + HttpUtility.UrlEncode(Resource.SsoSettingsEmptyToken), false);
                    context.ApplicationInstance.CompleteRequest();
                    return;
                }
                _log.Debug("Trying to authenticate using SAML");
                SamlResponse samlResponse = new SamlResponse(settings);
                samlResponse.LoadXmlFromBase64(samlEncodedString);
                if (!samlResponse.IsValid())
                {
                    _log.Error("SAML response is not valid");
                    context.Response.Redirect(AUTH_PAGE + "?m=" + HttpUtility.UrlEncode(Resource.SsoSettingsNotValidToken), false);
                    context.ApplicationInstance.CompleteRequest();
                    return;
                }
                SamlUserCreator userCreator = new SamlUserCreator();
                userInfo = userCreator.CreateUserInfo(samlResponse);
                if (userInfo == Constants.LostUser)
                {
                    _log.Error("Can't create userInfo using current SAML response");
                    context.Response.Redirect(AUTH_PAGE + "?m=" + HttpUtility.UrlEncode(Resource.SsoSettingsCantCreateUser), false);
                    context.ApplicationInstance.CompleteRequest();
                    return;
                }
                if (userInfo.Status == EmployeeStatus.Terminated)
                {
                    _log.Error("Current user is terminated");
                    context.Response.Redirect(AUTH_PAGE + "?m=" + HttpUtility.UrlEncode(Resource.SsoSettingsUserTerminated), false);
                    context.ApplicationInstance.CompleteRequest();
                    return;
                }
                AddUser(samlResponse, userInfo);
                MessageService.Send(context.Request, MessageAction.LoginSuccessViaSSO);
                context.Response.Redirect(CommonLinkUtility.GetDefault(), false);
                context.ApplicationInstance.CompleteRequest();
            }
            catch (Exception e)
            {
                _log.ErrorFormat("Unexpected error. {0}", e);
                context.Response.Redirect(AUTH_PAGE + "?m=" + HttpUtility.UrlEncode(e.Message), false);
                context.ApplicationInstance.CompleteRequest();
            }
        }

        #endregion

        private void AddUser(SamlResponse samlResponse, UserInfo userInfo)
        {
            try
            {
                _log.DebugFormat("Adding or updating user in database, userId={0}", userInfo.ID);
                SecurityContext.AuthenticateMe(ASC.Core.Configuration.Constants.CoreSystem);
                if (!string.IsNullOrEmpty(userInfo.MobilePhone))
                {
                    userInfo.MobilePhone = SmsManager.GetPhoneValueDigits(userInfo.MobilePhone);
                }
                if (string.IsNullOrEmpty(userInfo.UserName))
                {
                    if (string.IsNullOrWhiteSpace(userInfo.FirstName))
                    {
                        userInfo.FirstName = Resource.FirstName;
                    }
                    if (string.IsNullOrWhiteSpace(userInfo.LastName))
                    {
                        userInfo.LastName = Resource.LastName;
                    }
                    if (TenantStatisticsProvider.GetUsersCount() < TenantExtra.GetTenantQuota().ActiveUsers)
                    {
                        userInfo = UserManagerWrapper.AddUser(userInfo, UserManagerWrapper.GeneratePassword(), true, false);
                    }
                    else
                    {
                        userInfo = UserManagerWrapper.AddUser(userInfo, UserManagerWrapper.GeneratePassword(), true, false, true);
                    }
                }
                else
                {
                    CoreContext.UserManager.SaveUserInfo(userInfo);
                }
                var photoUrl = samlResponse.GetRemotePhotoUrl();
                if (!string.IsNullOrEmpty(photoUrl))
                {
                    var photoLoader = new UserPhotoLoader();
                    photoLoader.SaveOrUpdatePhoto(photoUrl, userInfo.ID);
                }
            }
            finally
            {
                SecurityContext.Logout();
            }
            var cookiesKey = SecurityContext.AuthenticateMe(userInfo.ID);
            CookiesManager.SetCookies(CookiesType.AuthKey, cookiesKey);
        }
    }
}
