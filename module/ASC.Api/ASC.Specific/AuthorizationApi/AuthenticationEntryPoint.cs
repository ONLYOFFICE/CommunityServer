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
using System.Security.Authentication;
using System.Threading;
using System.Web;
using ASC.Api.Attributes;
using ASC.Api.Impl;
using ASC.Api.Interfaces;
using ASC.Api.Utils;
using ASC.Core;
using ASC.Core.Users;
using ASC.MessagingSystem;
using ASC.Security.Cryptography;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.Core.SMS;
using ASC.Web.Studio.Core.Users;
using Resources;

namespace ASC.Specific.AuthorizationApi
{
    /// <summary>
    /// Authorization for api
    /// </summary>
    public class AuthenticationEntryPoint : IApiEntryPoint
    {
        private readonly ApiContext _context;

        /// <summary>
        /// Entry point name
        /// </summary>
        public string Name
        {
            get { return "authentication"; }
        }

        ///<summary>
        /// Constructor
        ///</summary>
        ///<param name="context"></param>
        public AuthenticationEntryPoint(ApiContext context)
        {
            _context = context;
        }

        private static HttpRequest Request
        {
            get { return HttpContext.Current.Request; }
        }

        /// <summary>
        /// Gets authentication token for use in api authorization
        /// </summary>
        /// <short>
        /// Get token
        /// </short>
        /// <param name="userName">user name or email</param>
        /// <param name="password">password</param>
        /// <returns>tokent to use in 'Authorization' header when calling API methods</returns>
        /// <exception cref="AuthenticationException">Thrown when not authenticated</exception>
        [Create(@"", false)] //NOTE: this method doesn't requires auth!!!
        public AuthenticationTokenData AuthenticateMe(string userName, string password)
        {
            userName.ThrowIfNull(new ArgumentException("userName empty", "userName"));
            password.ThrowIfNull(new ArgumentException("password empty", "password"));

            if (!StudioSmsNotificationSettings.IsVisibleSettings || !StudioSmsNotificationSettings.Enable)
            {
                try
                {
                    var token = SecurityContext.AuthenticateMe(userName, password);
                    if (string.IsNullOrEmpty(token))
                    {
                        throw new AuthenticationException("User authentication failed");
                    }

                    MessageService.Send(Request, MessageAction.LoginSuccessViaApi);

                    return new AuthenticationTokenData
                    {
                        Token = token,
                        Expires = new ApiDateTime(DateTime.UtcNow.AddYears(1))
                    };
                }
                catch
                {
                    MessageService.Send(Request, userName, MessageAction.LoginFailViaApi);
                    throw;
                }
            }

            var user = GetUser(userName, password);

            if (string.IsNullOrEmpty(user.MobilePhone) || user.MobilePhoneActivationStatus == MobilePhoneActivationStatus.NotActivated)
                return new AuthenticationTokenData
                    {
                        Sms = true
                    };

            SmsManager.PutAuthCode(user, false);

            return new AuthenticationTokenData
                {
                    Sms = true,
                    PhoneNoise = SmsManager.BuildPhoneNoise(user.MobilePhone),
                    Expires = new ApiDateTime(DateTime.UtcNow.AddMinutes(10))
                };
        }

        /// <summary>
        /// Set mobile phone for user
        /// </summary>
        /// <param name="userName">user name or email</param>
        /// <param name="password">password</param>
        /// <param name="mobilePhone">new mobile phone</param>
        /// <returns>mobile phone</returns>
        [Create(@"setphone", false)] //NOTE: this method doesn't requires auth!!!
        public AuthenticationTokenData SaveMobilePhone(string userName, string password, string mobilePhone)
        {
            var user = GetUser(userName, password);
            mobilePhone = SmsManager.SaveMobilePhone(user, mobilePhone);

            return new AuthenticationTokenData
                {
                    Sms = true,
                    PhoneNoise = SmsManager.BuildPhoneNoise(mobilePhone)
                };
        }

        /// <summary>
        /// Send sms code again
        /// </summary>
        /// <param name="userName">user name or email</param>
        /// <param name="password">password</param>
        /// <returns>mobile phone</returns>
        [Create(@"sendsms", false)] //NOTE: this method doesn't requires auth!!!
        public AuthenticationTokenData SendSmsCode(string userName, string password)
        {
            var user = GetUser(userName, password);
            SmsManager.PutAuthCode(user, true);

            return new AuthenticationTokenData
                {
                    Sms = true,
                    PhoneNoise = SmsManager.BuildPhoneNoise(user.MobilePhone)
                };
        }

        /// <summary>
        /// Gets authentication token for use in api authorization
        /// </summary>
        /// <short>
        /// Get token
        /// </short>
        /// <param name="userName">user name or email</param>
        /// <param name="password">password</param>
        /// <param name="code">sms code</param>
        /// <returns>tokent to use in 'Authorization' header when calling API methods</returns>
        [Create(@"{code}", false)] //NOTE: this method doesn't requires auth!!!
        public AuthenticationTokenData AuthenticateMe(string userName, string password, string code)
        {
            var user = GetUser(userName, password);

            SmsManager.ValidateSmsCode(user, code);

            try
            {
                var token = SecurityContext.AuthenticateMe(user.ID);
                if (string.IsNullOrEmpty(token))
                {
                    throw new AuthenticationException("User authentication failed");
                }

                MessageService.Send(Request, MessageAction.LoginSuccessViaApiSms);

                return new AuthenticationTokenData
                    {
                        Token = token,
                        Expires = new ApiDateTime(DateTime.UtcNow.AddYears(1)),
                        Sms = true,
                        PhoneNoise = SmsManager.BuildPhoneNoise(user.MobilePhone)
                    };
            }
            catch
            {
                MessageService.Send(Request, userName, MessageAction.LoginFailViaApi);
                throw;
            }
        }

        /// <summary>
        /// Request of invitation by email on personal.onlyoffice.com
        /// </summary>
        /// <param name="email">Email address</param>
        /// <param name="lang">Culture</param>
        /// <param name="campaign"></param>
        /// <visible>false</visible>
        [Create(@"register", false)] //NOTE: this method doesn't requires auth!!!
        public string RegisterUserOnPersonal(string email, string lang, bool campaign = false)
        {
            if (!CoreContext.Configuration.Personal) throw new MethodAccessException("Method is only available on personal.onlyoffice.com");

            try
            {
                var cultureInfo = SetupInfo.EnabledCultures.Find(c => String.Equals(c.TwoLetterISOLanguageName, lang, StringComparison.InvariantCultureIgnoreCase));
                if (cultureInfo != null)
                {
                    Thread.CurrentThread.CurrentUICulture = cultureInfo;
                }

                email.ThrowIfNull(new ArgumentException(Resource.ErrorEmailEmpty, "email"));

                if (!email.TestEmailRegex()) throw new ArgumentException(Resource.ErrorNotCorrectEmail, "email");

                var newUserInfo = CoreContext.UserManager.GetUserByEmail(email);

                if (CoreContext.UserManager.UserExists(newUserInfo.ID))
                {
                    if (!SetupInfo.IsSecretEmail(email) || SecurityContext.IsAuthenticated)
                    {
                        throw new Exception(CustomNamingPeople.Substitute<Resource>("ErrorEmailAlreadyExists"));
                    }

                    try
                    {
                        SecurityContext.AuthenticateMe(Core.Configuration.Constants.CoreSystem);
                        CoreContext.UserManager.DeleteUser(newUserInfo.ID);
                    }
                    finally
                    {
                        SecurityContext.Logout();
                    }
                }

                StudioNotifyService.Instance.SendInvitePersonal(email, campaign ? "&campaign=personal" : "");
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return string.Empty;
        }

        private static UserInfo GetUser(string userName, string password)
        {
            var user = CoreContext.UserManager.GetUsers(
                CoreContext.TenantManager.GetCurrentTenant().TenantId,
                userName,
                Hasher.Base64Hash(password, HashAlg.SHA256));

            if (user == null || Equals(user, Constants.LostUser))
                throw new AuthenticationException("User authentication failed");

            return user;
        }
    }
}