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
        public const string GoogleOauthTokenUrl = GoogleOauthUrl + "token";


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
                return RequestProfile(token);
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

        private static LoginProfile RequestProfile(OAuth20Token token)
        {
            var googleProfile = RequestHelper.PerformRequest(GoogleProfileUrl + "me", headers: new Dictionary<string, string> { { "Authorization", "Bearer " + token.AccessToken } });
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