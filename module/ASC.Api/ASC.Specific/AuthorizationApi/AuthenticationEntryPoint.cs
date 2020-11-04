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
using System.Globalization;
using System.Security;
using System.Security.Authentication;
using System.Threading;
using System.Web;
using ASC.ActiveDirectory;
using ASC.ActiveDirectory.ComplexOperations;
using ASC.Api.Attributes;
using ASC.Api.Interfaces;
using ASC.Api.Utils;
using ASC.Common.Caching;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.FederatedLogin.LoginProviders;
using ASC.IPSecurity;
using ASC.MessagingSystem;
using ASC.Security.Cryptography;
using ASC.Web.Core.Sms;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.Core.SMS;
using ASC.Web.Studio.Core.TFA;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.UserControls.Common;
using ASC.Web.Studio.Utility;
using Resources;
using Constants = ASC.Core.Configuration.Constants;
using SecurityContext = ASC.Core.SecurityContext;
using ASC.ActiveDirectory.Base.Settings;

namespace ASC.Specific.AuthorizationApi
{
    /// <summary>
    /// Authorization for api
    /// </summary>
    public class AuthenticationEntryPoint : IApiEntryPoint
    {
        private static readonly ICache Cache = AscCache.Memory;


        /// <summary>
        /// Entry point name
        /// </summary>
        public string Name
        {
            get { return "authentication"; }
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
        /// <param name="provider">social media provider type</param>
        /// <param name="accessToken">provider token</param>
        /// <returns>tokent to use in 'Authorization' header when calling API methods</returns>
        /// <exception cref="AuthenticationException">Thrown when not authenticated</exception>
        [Create(@"", false, false)] //NOTE: this method doesn't requires auth!!!  //NOTE: this method doesn't check payment!!!
        public AuthenticationTokenData AuthenticateMe(string userName, string password, string provider, string accessToken)
        {
            bool viaEmail;
            var user = GetUser(userName, password, provider, accessToken, out viaEmail);

            if (StudioSmsNotificationSettings.IsVisibleSettings && StudioSmsNotificationSettings.Enable)
            {
                if (string.IsNullOrEmpty(user.MobilePhone) || user.MobilePhoneActivationStatus == MobilePhoneActivationStatus.NotActivated)
                    return new AuthenticationTokenData
                        {
                            Sms = true
                        };

                SmsManager.PutAuthCode(user, false);

                return new AuthenticationTokenData
                    {
                        Sms = true,
                        PhoneNoise = SmsSender.BuildPhoneNoise(user.MobilePhone),
                        Expires = new ApiDateTime(DateTime.UtcNow.Add(SmsKeyStorage.StoreInterval))
                    };
            }

            if (TfaAppAuthSettings.IsVisibleSettings && TfaAppAuthSettings.Enable)
            {
                if (!TfaAppUserSettings.EnableForUser(user.ID))
                    return new AuthenticationTokenData
                        {
                            Tfa = true,
                            TfaKey = user.GenerateSetupCode(300).ManualEntryKey
                        };

                return new AuthenticationTokenData
                    {
                        Tfa = true
                    };
            }

            try
            {
                var token = SecurityContext.AuthenticateMe(user.ID);

                MessageService.Send(Request, viaEmail ? MessageAction.LoginSuccessViaApi : MessageAction.LoginSuccessViaApiSocialAccount);

                var tenant = CoreContext.TenantManager.GetCurrentTenant().TenantId;
                var expires = TenantCookieSettings.GetExpiresTime(tenant);

                return new AuthenticationTokenData
                    {
                        Token = token,
                        Expires = new ApiDateTime(expires)
                    };
            }
            catch
            {
                MessageService.Send(Request, user.DisplayUserName(false), viaEmail ? MessageAction.LoginFailViaApi : MessageAction.LoginFailViaApiSocialAccount);
                throw new AuthenticationException("User authentication failed");
            }
            finally
            {
                SecurityContext.Logout();
            }
        }

        /// <summary>
        /// Set mobile phone for user
        /// </summary>
        /// <param name="userName">user name or email</param>
        /// <param name="password">password</param>
        /// <param name="provider">social media provider type</param>
        /// <param name="accessToken">provider token</param>
        /// <param name="mobilePhone">new mobile phone</param>
        /// <returns>mobile phone</returns>
        [Create(@"setphone", false, false)] //NOTE: this method doesn't requires auth!!!  //NOTE: this method doesn't check payment!!!
        public AuthenticationTokenData SaveMobilePhone(string userName, string password, string provider, string accessToken, string mobilePhone)
        {
            bool viaEmail;
            var user = GetUser(userName, password, provider, accessToken, out viaEmail);
            mobilePhone = SmsManager.SaveMobilePhone(user, mobilePhone);
            MessageService.Send(HttpContext.Current.Request, MessageAction.UserUpdatedMobileNumber, MessageTarget.Create(user.ID), user.DisplayUserName(false), mobilePhone);

            return new AuthenticationTokenData
                {
                    Sms = true,
                    PhoneNoise = SmsSender.BuildPhoneNoise(mobilePhone),
                    Expires = new ApiDateTime(DateTime.UtcNow.Add(SmsKeyStorage.StoreInterval))
                };
        }

        /// <summary>
        /// Send sms code again
        /// </summary>
        /// <param name="userName">user name or email</param>
        /// <param name="password">password</param>
        /// <param name="provider">social media provider type</param>
        /// <param name="accessToken">provider token</param>
        /// <returns>mobile phone</returns>
        [Create(@"sendsms", false, false)] //NOTE: this method doesn't requires auth!!!  //NOTE: this method doesn't check payment!!!
        public AuthenticationTokenData SendSmsCode(string userName, string password, string provider, string accessToken)
        {
            bool viaEmail;
            var user = GetUser(userName, password, provider, accessToken, out viaEmail);
            SmsManager.PutAuthCode(user, true);

            return new AuthenticationTokenData
                {
                    Sms = true,
                    PhoneNoise = SmsSender.BuildPhoneNoise(user.MobilePhone),
                    Expires = new ApiDateTime(DateTime.UtcNow.Add(SmsKeyStorage.StoreInterval))
                };
        }

        /// <summary>
        /// Gets two-factor authentication token for use in api authorization
        /// </summary>
        /// <short>
        /// Get token
        /// </short>
        /// <param name="userName">user name or email</param>
        /// <param name="password">password</param>
        /// <param name="provider">social media provider type</param>
        /// <param name="accessToken">provider token</param>
        /// <param name="code">two-factor authentication code</param>
        /// <returns>tokent to use in 'Authorization' header when calling API methods</returns>
        [Create(@"{code}", false, false)] //NOTE: this method doesn't requires auth!!!  //NOTE: this method doesn't check payment!!!
        public AuthenticationTokenData AuthenticateMe(string userName, string password, string provider, string accessToken, string code)
        {
            bool viaEmail;
            var user = GetUser(userName, password, provider, accessToken, out viaEmail);

            var sms = false;
            try
            {
                if (StudioSmsNotificationSettings.IsVisibleSettings && StudioSmsNotificationSettings.Enable)
                {
                    sms = true;
                    SmsManager.ValidateSmsCode(user, code);
                }
                else if (TfaAppAuthSettings.IsVisibleSettings && TfaAppAuthSettings.Enable)
                {
                    if (user.ValidateAuthCode(code))
                    {
                        MessageService.Send(HttpContext.Current.Request, MessageAction.UserConnectedTfaApp, MessageTarget.Create(user.ID));
                    }
                }
                else
                {
                    throw new SecurityException("Auth code is not available");
                }

                var token = SecurityContext.AuthenticateMe(user.ID);

                MessageService.Send(Request, sms ? MessageAction.LoginSuccessViaApiSms : MessageAction.LoginSuccessViaApiTfa);

                var tenant = CoreContext.TenantManager.GetCurrentTenant().TenantId;
                var expires = TenantCookieSettings.GetExpiresTime(tenant);

                var result = new AuthenticationTokenData
                    {
                        Token = token,
                        Expires = new ApiDateTime(expires)
                    };

                if (sms)
                {
                    result.Sms = true;
                    result.PhoneNoise = SmsSender.BuildPhoneNoise(user.MobilePhone);
                }
                else
                {
                    result.Tfa = true;
                }

                return result;
            }
            catch
            {
                MessageService.Send(Request, user.DisplayUserName(false), sms
                                                                              ? MessageAction.LoginFailViaApiSms
                                                                              : MessageAction.LoginFailViaApiTfa,
                                    MessageTarget.Create(user.ID));
                throw new AuthenticationException("User authentication failed");
            }
            finally
            {
                SecurityContext.Logout();
            }
        }

        /// <summary>
        /// Request of invitation by email on personal.onlyoffice.com
        /// </summary>
        /// <param name="email">Email address</param>
        /// <param name="lang">Culture</param>
        /// <param name="spam">User consent to subscribe to the ONLYOFFICE newsletter</param>
        /// <param name="analytics">Track analytics</param>
        /// <param name="recaptchaResponse">recaptcha token</param>
        /// <visible>false</visible>
        [Create(@"register", false)] //NOTE: this method doesn't requires auth!!!
        public string RegisterUserOnPersonal(string email, string lang, bool spam, bool analytics, string recaptchaResponse)
        {
            if (!CoreContext.Configuration.Personal) throw new MethodAccessException("Method is only available on personal.onlyoffice.com");

            try
            {
                if (CoreContext.Configuration.CustomMode) lang = "ru-RU";

                var cultureInfo = SetupInfo.EnabledCultures.Find(c => String.Equals(c.TwoLetterISOLanguageName, lang, StringComparison.InvariantCultureIgnoreCase));
                if (cultureInfo != null)
                {
                    Thread.CurrentThread.CurrentUICulture = cultureInfo;
                }

                email.ThrowIfNull(new ArgumentException(Resource.ErrorEmailEmpty, "email"));

                if (!email.TestEmailRegex()) throw new ArgumentException(Resource.ErrorNotCorrectEmail, "email");

                if (!SetupInfo.IsSecretEmail(email)
                    && !string.IsNullOrEmpty(SetupInfo.RecaptchaPublicKey) && !string.IsNullOrEmpty(SetupInfo.RecaptchaPrivateKey))
                {
                    var ip = Request.Headers["X-Forwarded-For"] ?? Request.UserHostAddress;

                    if (String.IsNullOrEmpty(recaptchaResponse)
                        || !Authorize.ValidateRecaptcha(recaptchaResponse, ip))
                    {
                        throw new Authorize.RecaptchaException(Resource.RecaptchaInvalid);
                    }
                }

                var newUserInfo = CoreContext.UserManager.GetUserByEmail(email);

                if (CoreContext.UserManager.UserExists(newUserInfo.ID))
                {
                    if (!SetupInfo.IsSecretEmail(email) || SecurityContext.IsAuthenticated)
                    {
                        throw new Exception(CustomNamingPeople.Substitute<Resource>("ErrorEmailAlreadyExists"));
                    }

                    try
                    {
                        SecurityContext.AuthenticateMe(Constants.CoreSystem);
                        CoreContext.UserManager.DeleteUser(newUserInfo.ID);
                    }
                    finally
                    {
                        SecurityContext.Logout();
                    }
                }
                if (!spam)
                {
                    try
                    {
                        const string _databaseID = "com";
                        using (var db = DbManager.FromHttpContext(_databaseID))
                        {
                            db.ExecuteNonQuery(new SqlInsert("template_unsubscribe", false)
                                                   .InColumnValue("email", email.ToLowerInvariant())
                                                   .InColumnValue("reason", "personal")
                                );
                            LogManager.GetLogger("ASC.Web").Debug(String.Format("Write to template_unsubscribe {0}", email.ToLowerInvariant()));
                        }
                    }
                    catch (Exception ex)
                    {
                        LogManager.GetLogger("ASC.Web").Debug(String.Format("ERROR write to template_unsubscribe {0}, email:{1}", ex.Message, email.ToLowerInvariant()));
                    }
                }
                StudioNotifyService.Instance.SendInvitePersonal(email, String.Empty, analytics);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return string.Empty;
        }

        /// <summary>
        /// Check userName and password
        /// </summary>
        /// <param name="userName">user name or email</param>
        /// <param name="password">password</param>
        /// <param name="key"></param>
        /// <exception cref="AuthenticationException">Thrown when not authenticated</exception>
        /// <visible>false</visible>
        [Create(@"login", false, false)] //NOTE: this method doesn't requires auth!!!  //NOTE: this method doesn't check payment!!!
        public bool AuthenticateMe(string userName, string password, string key)
        {
            var authInterval = TimeSpan.FromMinutes(5);
            var checkKeyResult = EmailValidationKeyProvider.ValidateEmailKey(userName + password + ConfirmType.Auth, key, authInterval);
            if (checkKeyResult != EmailValidationKeyProvider.ValidationResult.Ok) throw new SecurityException("Access Denied.");

            bool viaEmail;
            var user = GetUser(userName, password, null, null, out viaEmail);
            return user != null;
        }

        private static UserInfo GetUser(string userName, string password, string provider, string accessToken, out bool viaEmail)
        {
            viaEmail = true;
            var action = MessageAction.LoginFailViaApi;
            UserInfo user = null;
            try
            {
                if (string.IsNullOrEmpty(provider) || provider == "email")
                {
                    userName.ThrowIfNull(new ArgumentException(@"userName empty", "userName"));
                    password.ThrowIfNull(new ArgumentException(@"password empty", "password"));

                    int counter;
                    int.TryParse(Cache.Get<String>("loginsec/" + userName), out counter);
                    if (++counter > SetupInfo.LoginThreshold && !SetupInfo.IsSecretEmail(userName))
                    {
                        throw new Authorize.BruteForceCredentialException();
                    }
                    Cache.Insert("loginsec/" + userName, counter.ToString(CultureInfo.InvariantCulture), DateTime.UtcNow.Add(TimeSpan.FromMinutes(1)));

                    if (EnableLdap) {
                        var localization = new LdapLocalization(Resource.ResourceManager);
                        var ldapUserManager = new LdapUserManager(localization);

                        ldapUserManager.TryGetAndSyncLdapUserInfo(userName, password, out user);
                    }

                    if (user == null || !CoreContext.UserManager.UserExists(user.ID))
                    {
                        var passwordHash = PasswordHasher.GetClientPassword(password);
                        user = CoreContext.UserManager.GetUsersByPasswordHash(
                            CoreContext.TenantManager.GetCurrentTenant().TenantId,
                            userName,
                            passwordHash);
                    }

                    if (user == null || !CoreContext.UserManager.UserExists(user.ID))
                    {
                        throw new Exception("user not found");
                    }

                    Cache.Insert("loginsec/" + userName, (--counter).ToString(CultureInfo.InvariantCulture), DateTime.UtcNow.Add(TimeSpan.FromMinutes(1)));
                }
                else
                {
                    viaEmail = false;

                    action = MessageAction.LoginFailViaApiSocialAccount;
                    var thirdPartyProfile = ProviderManager.GetLoginProfile(provider, accessToken);
                    userName = thirdPartyProfile.EMail;

                    user = LoginWithThirdParty.GetUserByThirdParty(thirdPartyProfile);
                }
            }
            catch (Authorize.BruteForceCredentialException)
            {
                MessageService.Send(Request, !string.IsNullOrEmpty(userName) ? userName : AuditResource.EmailNotSpecified, MessageAction.LoginFailBruteForce);
                throw new AuthenticationException("Login Fail. Too many attempts");
            }
            catch
            {
                MessageService.Send(Request, !string.IsNullOrEmpty(userName) ? userName : AuditResource.EmailNotSpecified, action);
                throw new AuthenticationException("User authentication failed");
            }

            var tenant = CoreContext.TenantManager.GetCurrentTenant();
            var settings = IPRestrictionsSettings.Load();
            if (settings.Enable && user.ID != tenant.OwnerId && !IPSecurity.IPSecurity.Verify(tenant))
            {
                throw new IPSecurityException();
            }

            return user;
        }

        protected static bool EnableLdap
        {
            get
            {
                if (!SetupInfo.IsVisibleSettings(ManagementType.LdapSettings.ToString()) ||
                    (CoreContext.Configuration.Standalone &&
                        !CoreContext.TenantManager.GetTenantQuota(TenantProvider.CurrentTenantID).Ldap))
                {
                    return false;
                }

                var enabled = LdapSettings.Load().EnableLdapAuthentication;

                return enabled;
            }
        }
    }
}