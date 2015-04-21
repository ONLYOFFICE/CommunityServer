/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using System.Collections.Generic;
using System.Threading;
using System.Web;
using ASC.FederatedLogin.Helpers;
using ASC.FederatedLogin.Profile;
using ASC.Thrdparty;
using ASC.Thrdparty.Configuration;
using Newtonsoft.Json.Linq;

namespace ASC.FederatedLogin.LoginProviders
{
    public class GoogleLoginProvider : ILoginProvider
    {
        private const string GoogleProfileUrl = "https://www.googleapis.com/plus/v1/people/";
        private const string GoogleProfileScope = "https://www.googleapis.com/auth/userinfo.email";

        private const string GoogleOauthUrl = "https://accounts.google.com/o/oauth2/";
        public const string GoogleOauthCodeUrl = GoogleOauthUrl + "auth";
        public const string GoogleOauthTokenUrl = "https://www.googleapis.com/oauth2/v3/token";


        public static string GoogleOAuth20ClientId
        {
            get { return KeyStorage.Get("googleClientId"); }
        }

        public static string GoogleOAuth20ClientSecret
        {
            get { return KeyStorage.Get("googleClientSecret"); }
        }

        public static string GoogleOAuth20RedirectUrl
        {
            get { return KeyStorage.Get("googleRedirectUrl"); }
        }

        public LoginProfile ProcessAuthoriztion(HttpContext context, IDictionary<string, string> @params)
        {
            try
            {
                var token = Auth(context, GoogleProfileScope);
                return token == null
                           ? LoginProfile.FromError(new Exception("Login failed"))
                           : RequestProfile(token.AccessToken);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return LoginProfile.FromError(ex);
            }
        }

        public LoginProfile GetLoginProfile(string accessToken)
        {
            try
            {
                return RequestProfile(accessToken);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return LoginProfile.FromError(ex);
            }
        }

        public static OAuth20Token Auth(HttpContext context, string scopes)
        {
            var error = context.Request["error"];
            if (!string.IsNullOrEmpty(error))
            {
                if (error == "access_denied")
                {
                    error = "Canceled at provider";
                }
                throw new Exception(error);
            }

            var code = context.Request["code"];
            if (string.IsNullOrEmpty(code))
            {
                var additionalArgs =
                    (context.Request["access_type"] ?? "") == "offline"
                        ? new Dictionary<string, string>
                        {
                            { "access_type", "offline" },
                            { "approval_prompt", "force" }
                        }
                        : null;

                OAuth20TokenHelper.RequestCode(HttpContext.Current,
                                               GoogleOauthCodeUrl,
                                               GoogleOAuth20ClientId,
                                               GoogleOAuth20RedirectUrl,
                                               scopes,
                                               additionalArgs);
                return null;
            }

            var token = OAuth20TokenHelper.GetAccessToken(GoogleOauthTokenUrl,
                                                          GoogleOAuth20ClientId,
                                                          GoogleOAuth20ClientSecret,
                                                          GoogleOAuth20RedirectUrl,
                                                          code);
            return token;
        }

        private static LoginProfile RequestProfile(string accessToken)
        {
            var googleProfile = RequestHelper.PerformRequest(GoogleProfileUrl + "me", headers: new Dictionary<string, string> { { "Authorization", "Bearer " + accessToken } });
            var loginProfile = ProfileFromGoogle(googleProfile);
            return loginProfile;
        }

        private static LoginProfile ProfileFromGoogle(string googleProfile)
        {
            var jProfile = JObject.Parse(googleProfile);
            if (jProfile == null) throw new Exception("Failed to correctly process the response");

            var email = string.Empty;
            var emailsArr = jProfile.Value<JArray>("emails");
            if (emailsArr != null)
            {
                var emailsList = emailsArr.ToObject<List<GoogleEmail>>();
                if (emailsList.Count == 0) return null;

                var ind = emailsList.FindIndex(gEmail => gEmail.primary);
                email = emailsList[ind > -1 ? ind : 0].value;
            }

            var profile = new LoginProfile
                {
                    EMail = email,
                    Id = jProfile.Value<string>("id"),
                    DisplayName = jProfile.Value<string>("displayName"),
                    FirstName = (string)jProfile.SelectToken("name.givenName"),
                    LastName = (string)jProfile.SelectToken("name.familyName"),
                    MiddleName = (string)jProfile.SelectToken("name.middleName"),
                    Link = jProfile.Value<string>("url"),
                    BirthDay = jProfile.Value<string>("birthday"),
                    Gender = jProfile.Value<string>("gender"),
                    Locale = jProfile.Value<string>("language"),
                    TimeZone = jProfile.Value<string>("currentLocation"),
                    Avatar = (string)jProfile.SelectToken("image.url"),

                    Provider = ProviderConstants.Google,
                };

            return profile;
        }

        private class GoogleEmail
        {
            public string value = null;
            public string type = null;
            public bool primary = false;
        }
    }
}