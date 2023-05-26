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
using System.Security;
using System.Security.Authentication;
using System.Threading;
using System.Web;

using ASC.ActiveDirectory;
using ASC.ActiveDirectory.Base.Settings;
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
using ASC.Web.Core;
using ASC.Web.Core.Sms;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.Core.SMS;
using ASC.Web.Studio.Core.TFA;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.PublicResources;
using ASC.Web.Studio.UserControls.Common;
using ASC.Web.Studio.Utility;

using Constants = ASC.Core.Configuration.Constants;
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Specific.AuthorizationApi
{
    /// <summary>
    /// Authorization API.
    /// </summary>
    /// <name>authentication</name>
    public class AuthenticationEntryPoint : IApiEntryPoint
    {
        private static readonly ICache Cache = AscCache.Memory;
        private ILog Log = LogManager.GetLogger("ASC.Web");


        /// <summary>
        /// Entry point name.
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
        /// Authenticates the current user by SMS, authenticator app, or without two-factor authentication.
        /// </summary>
        /// <short>Authenticate a user</short>
        /// <param type="System.String, System" name="userName">User name or email</param>
        /// <param type="System.String, System" name="password">Password</param>
        /// <param type="System.String, System" name="provider">Social media provider type</param>
        /// <param type="System.String, System" name="accessToken">Provider token</param>
        /// <param type="System.String, System" name="codeOAuth">Code for getting a token</param>
        /// <httpMethod>POST</httpMethod>
        /// <path>api/2.0/authentication</path>
        /// <returns type="ASC.Specific.AuthorizationApi.AuthenticationTokenData, ASC.Specific">Authentication token to use in the 'Authorization' header when calling API methods</returns>
        /// <exception cref="AuthenticationException">Thrown when not authenticated</exception>
        /// <requiresAuthorization>false</requiresAuthorization>
        [Create(@"", false, false)] //NOTE: this method doesn't require auth!!!  //NOTE: this method doesn't check payment!!!
        public AuthenticationTokenData AuthenticateMe(string userName, string password, string provider, string accessToken, string codeOAuth)
        {
            bool viaEmail;
            var user = GetUser(userName, password, provider, accessToken, out viaEmail, codeOAuth);

            if (StudioSmsNotificationSettings.IsVisibleAndAvailableSettings && StudioSmsNotificationSettings.TfaEnabledForUser(user.ID))
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

            if (TfaAppAuthSettings.IsVisibleSettings && TfaAppAuthSettings.TfaEnabledForUser(user.ID))
            {
                if (!TfaAppUserSettings.EnableForUser(user.ID))
                    return new AuthenticationTokenData
                    {
                        Tfa = true,
                        TfaKey = user.GenerateSetupCode().ManualEntryKey
                    };

                return new AuthenticationTokenData
                {
                    Tfa = true
                };
            }

            try
            {
                var action = viaEmail ? MessageAction.LoginSuccessViaApi : MessageAction.LoginSuccessViaApiSocialAccount;
                var token = CookiesManager.AuthenticateMeAndSetCookies(user.Tenant, user.ID, action);

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
        /// Sets a mobile phone for the user with the name specified in the request.
        /// </summary>
        /// <short>Set a mobile phone</short>
        /// <param type="System.String, System" name="userName">User name or email</param>
        /// <param type="System.String, System" name="password">Password</param>
        /// <param type="System.String, System" name="provider">Social media provider type</param>
        /// <param type="System.String, System" name="accessToken">Provider token</param>
        /// <param type="System.String, System" name="mobilePhone">New mobile phone</param>
        /// <httpMethod>POST</httpMethod>
        /// <path>api/2.0/authentication/setphone</path>
        /// <returns type="ASC.Specific.AuthorizationApi.AuthenticationTokenData, ASC.Specific">Authentication data: authentication by SMS or not, phone number, SMS expiration time</returns>
        /// <requiresAuthorization>false</requiresAuthorization>
        [Create(@"setphone", false, false)] //NOTE: This method doesn't require auth!!!  //NOTE: This method doesn't check payment!!!
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
        /// Sends SMS with an authentication code.
        /// </summary>
        /// <short>Send SMS code</short>
        /// <param type="System.String, System" name="userName">User name or email</param>
        /// <param type="System.String, System" name="password">Password</param>
        /// <param type="System.String, System" name="provider">Social media provider type</param>
        /// <param type="System.String, System" name="accessToken">Provider token</param>
        /// <httpMethod>POST</httpMethod>
        /// <path>api/2.0/authentication/sendsms</path>
        /// <returns type="ASC.Specific.AuthorizationApi.AuthenticationTokenData, ASC.Specific">Authentication data: authentication by SMS or not, phone number, SMS expiration time</returns>
        /// <requiresAuthorization>false</requiresAuthorization>
        [Create(@"sendsms", false, false)] //NOTE: This method doesn't require auth!!!  //NOTE: This method doesn't check payment!!!
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
        /// Authenticates the current user by SMS or two-factor authentication code.
        /// </summary>
        /// <short>
        /// Authenticate a user by code
        /// </short>
        /// <param type="System.String, System" name="userName">User name or email</param>
        /// <param type="System.String, System" name="password">Password</param>
        /// <param type="System.String, System" name="provider">Social media provider type</param>
        /// <param type="System.String, System" name="accessToken">Provider token</param>
        /// <param type="System.String, System" name="code" method="url">Two-factor authentication code</param>
        /// <param type="System.String, System" name="codeOAuth">Code for getting a token</param>
        /// <httpMethod>POST</httpMethod>
        /// <path>api/2.0/authentication/{code}</path>
        /// <requiresAuthorization>false</requiresAuthorization>
        /// <returns type="ASC.Specific.AuthorizationApi.AuthenticationTokenData, ASC.Specific">Two-factor authentication token to use in the 'Authorization' header when calling API methods</returns>
        [Create(@"{code}", false, false)] //NOTE: This method doesn't require auth!!!  //NOTE: This method doesn't check payment!!!
        public AuthenticationTokenData AuthenticateMe(string userName, string password, string provider, string accessToken, string code, string codeOAuth)
        {
            bool viaEmail;
            var user = GetUser(userName, password, provider, accessToken, out viaEmail, codeOAuth);

            var sms = false;
            try
            {
                if (StudioSmsNotificationSettings.IsVisibleAndAvailableSettings && StudioSmsNotificationSettings.TfaEnabledForUser(user.ID))
                {
                    sms = true;
                    SmsManager.ValidateSmsCode(user, code, true);
                }
                else if (TfaAppAuthSettings.IsVisibleSettings && TfaAppAuthSettings.TfaEnabledForUser(user.ID))
                {
                    if (user.ValidateAuthCode(code, true, true))
                    {
                        MessageService.Send(Request, MessageAction.UserConnectedTfaApp, MessageTarget.Create(user.ID));
                    }
                }
                else
                {
                    throw new SecurityException("Auth code is not available");
                }

                var token = CookiesManager.AuthenticateMeAndSetCookies(user.Tenant, user.ID, MessageAction.LoginSuccess);
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
        /// Requests an email invitation from personal.onlyoffice.com.
        /// </summary>
        /// <short>Register a user on the Personal portal</short>
        /// <param type="System.String, System" name="email">Email address</param>
        /// <param type="System.String, System" name="lang">Culture</param>
        /// <param type="System.Boolean, System" name="spam">User consent to subscribe to the ONLYOFFICE newsletter</param>
        /// <param type="System.String, System" name="recaptchaResponse">ReCAPTCHA token</param>
        /// <httpMethod>POST</httpMethod>
        /// <path>api/2.0/authentication/register</path>
        /// <requiresAuthorization>false</requiresAuthorization>
        /// <visible>false</visible>
        [Create(@"register", false)] //NOTE: This method doesn't require auth!!!
        public string RegisterUserOnPersonal(string email, string lang, bool spam, string recaptchaResponse)
        {
            if (!CoreContext.Configuration.Personal) throw new MethodAccessException("Method is only available on personal.onlyoffice.com");

            try
            {
                if (CoreContext.Configuration.CustomMode) lang = "ru-RU";

                var cultureInfo = SetupInfo.GetPersonalCulture(lang).Value;

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
                        SecurityContext.CurrentAccount = Constants.CoreSystem;
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
                        using (var db = new DbManager(_databaseID))
                        {
                            db.ExecuteNonQuery(new SqlInsert("template_unsubscribe", false)
                                                   .InColumnValue("email", email.ToLowerInvariant())
                                                   .InColumnValue("reason", "personal")
                                );
                            Log.Debug(String.Format("Write to template_unsubscribe {0}", email.ToLowerInvariant()));
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Debug(String.Format("ERROR write to template_unsubscribe {0}, email:{1}", ex.Message, email.ToLowerInvariant()));
                    }
                }
                StudioNotifyService.Instance.SendInvitePersonal(email);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return string.Empty;
        }

        /// <summary>
        /// Checks the username and password when logging in.
        /// </summary>
        /// <short>Log in</short>
        /// <param type="System.String, System" name="userName">Username or email</param>
        /// <param type="System.String, System" name="password">Password</param>
        /// <param type="System.String, System" name="key">Email key</param>
        /// <exception cref="AuthenticationException">Thrown when not authenticated.</exception>
        /// <httpMethod>POST</httpMethod>
        /// <path>api/2.0/authentication/login</path>
        /// <requiresAuthorization>false</requiresAuthorization>
        /// <visible>false</visible>
        [Create(@"login", false, false)] //NOTE: This method doesn't require auth!!!  //NOTE: This method doesn't check payment!!!
        public bool AuthenticateMe(string userName, string password, string key)
        {
            var authInterval = TimeSpan.FromMinutes(5);
            var checkKeyResult = EmailValidationKeyProvider.ValidateEmailKey(userName + password + ConfirmType.Auth, key, authInterval);
            if (checkKeyResult != EmailValidationKeyProvider.ValidationResult.Ok) throw new SecurityException("Access Denied.");

            bool viaEmail;
            var user = GetUser(userName, password, null, null, out viaEmail);
            return user != null;
        }

        private static UserInfo GetUser(string userName, string password, string provider, string accessToken, out bool viaEmail, string codeOAuth = null)
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

                    var secretEmail = SetupInfo.IsSecretEmail(userName);
                    var requestIp = MessageSettings.GetFullIPAddress(Request);
                    var bruteForceLoginManager = new BruteForceLoginManager(Cache, userName, requestIp);
                    var bruteForceSuccessAttempt = bruteForceLoginManager.Increment(out bool _);

                    if (!secretEmail && !bruteForceSuccessAttempt)
                    {
                        throw new Authorize.BruteForceCredentialException();
                    }

                    if (EnableLdap)
                    {
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

                    if (!secretEmail)
                    {
                        bruteForceLoginManager.Decrement();
                    }
                }
                else
                {
                    if (!(CoreContext.Configuration.Standalone || CoreContext.TenantManager.GetTenantQuota(TenantProvider.CurrentTenantID).Oauth))
                    {
                        throw new Exception(Resource.ErrorNotAllowedOption);
                    }
                    viaEmail = false;

                    action = MessageAction.LoginFailViaApiSocialAccount;

                    var thirdPartyProfile = ProviderManager.GetLoginProfile(provider, accessToken, codeOAuth);
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
            if (settings.Enable && user.ID != tenant.OwnerId && !IPSecurity.IPSecurity.Verify(tenant, user.Email))
            {
                throw new IPSecurityException();
            }

            return user;
        }

        protected static bool EnableLdap
        {
            get
            {
                if (!CoreContext.Configuration.Standalone
                    && (!SetupInfo.IsVisibleSettings(ManagementType.LdapSettings.ToString())
                        || !CoreContext.TenantManager.GetTenantQuota(TenantProvider.CurrentTenantID).Ldap))
                {
                    return false;
                }

                var enabled = LdapSettings.Load().EnableLdapAuthentication;

                return enabled;
            }
        }
    }
}