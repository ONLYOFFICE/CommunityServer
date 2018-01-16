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
using System.Collections.Generic;
using System.Threading;
using System.Web;
using ASC.FederatedLogin.Helpers;
using ASC.FederatedLogin.Profile;
using ASC.Thrdparty.Configuration;
using Newtonsoft.Json.Linq;

namespace ASC.FederatedLogin.LoginProviders
{
    public class LinkedInLoginProvider : ILoginProvider
    {
        private const string LinkedInProfileUrl = "https://api.linkedin.com/v1/people/~:(id,first-name,last-name,formatted-name,email-address)?format=json";
        private const string LinkedInProfileScope = "r_basicprofile r_emailaddress";


        public const string LinkedInOauthCodeUrl = "https://www.linkedin.com/uas/oauth2/authorization";
        public const string LinkedInOauthTokenUrl = "https://www.linkedin.com/uas/oauth2/accessToken";

        public static string LinkedInOAuth20ClientId
        {
            get { return KeyStorage.Get("linkedInKey"); }
        }

        public static string LinkedInOAuth20ClientSecret
        {
            get { return KeyStorage.Get("linkedInSecret"); }
        }

        public static string LinkedInOAuth20RedirectUrl
        {
            get { return KeyStorage.Get("linkedInRedirectUrl"); }
        }

        public LoginProfile ProcessAuthoriztion(HttpContext context, IDictionary<string, string> @params)
        {
            try
            {
                var token = Auth(context, LinkedInProfileScope);

                return GetLoginProfile(token == null ? null : token.AccessToken);
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
            if (string.IsNullOrEmpty(accessToken))
                throw new Exception("Login failed");

            return RequestProfile(accessToken);
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
                OAuth20TokenHelper.RequestCode(HttpContext.Current,
                                               LinkedInOauthCodeUrl,
                                               LinkedInOAuth20ClientId,
                                               LinkedInOAuth20RedirectUrl,
                                               scopes);
                return null;
            }

            var token = OAuth20TokenHelper.GetAccessToken(LinkedInOauthTokenUrl,
                                                          LinkedInOAuth20ClientId,
                                                          LinkedInOAuth20ClientSecret,
                                                          LinkedInOAuth20RedirectUrl,
                                                          code);
            return token;
        }

        private static LoginProfile RequestProfile(string accessToken)
        {
            var linkedInProfile = RequestHelper.PerformRequest(LinkedInProfileUrl, headers: new Dictionary<string, string> {{"Authorization", "Bearer " + accessToken}});
            var loginProfile = ProfileFromLinkedIn(linkedInProfile);
            return loginProfile;
        }

        internal static LoginProfile ProfileFromLinkedIn(string linkedInProfile)
        {
            var jProfile = JObject.Parse(linkedInProfile);
            if (jProfile == null) throw new Exception("Failed to correctly process the response");

            var profile = new LoginProfile
                {
                    Id = jProfile.Value<string>("id"),
                    FirstName = jProfile.Value<string>("firstName"),
                    LastName = jProfile.Value<string>("lastName"),
                    DisplayName = jProfile.Value<string>("formattedName"),
                    EMail = jProfile.Value<string>("emailAddress"),
                    
                    Provider = ProviderConstants.LinkedIn,
                };

            return profile;
        }
    }
}