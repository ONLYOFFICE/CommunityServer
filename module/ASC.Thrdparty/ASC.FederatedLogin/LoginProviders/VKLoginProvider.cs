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
    public class VKLoginProvider : ILoginProvider
    {
        private const string VKProfileUrl = "https://api.vk.com/method/users.get";
        private const string VKProfileScope = "";

        private const string VKOauthUrl = "https://oauth.vk.com/";
        public const string VKOauthCodeUrl = VKOauthUrl + "authorize";
        public const string VKOauthTokenUrl = VKOauthUrl + "access_token";


        public static string VKOAuth20ClientId
        {
            get { return KeyStorage.Get("vkClientId"); }
        }

        public static string VKOAuth20ClientSecret
        {
            get { return KeyStorage.Get("vkClientSecret"); }
        }

        public static string VKOAuth20RedirectUrl
        {
            get { return KeyStorage.Get("vkRedirectUrl"); }
        }

        public LoginProfile ProcessAuthoriztion(HttpContext context, IDictionary<string, string> @params)
        {
            try
            {
                var token = Auth(context, VKProfileScope);
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
                OAuth20TokenHelper.RequestCode(HttpContext.Current,
                                               VKOauthCodeUrl,
                                               VKOAuth20ClientId,
                                               VKOAuth20RedirectUrl,
                                               scopes);
                return null;
            }

            var token = OAuth20TokenHelper.GetAccessToken(VKOauthTokenUrl,
                                                          VKOAuth20ClientId,
                                                          VKOAuth20ClientSecret,
                                                          VKOAuth20RedirectUrl,
                                                          code);
            return token;
        }

        private static LoginProfile RequestProfile(OAuth20Token token)
        {
            var vkProfile = RequestHelper.PerformRequest(VKProfileUrl + "?access_token=" + token.AccessToken);
            var loginProfile = ProfileFromVK(vkProfile);

            return loginProfile;
        }

        private static LoginProfile ProfileFromVK(string strProfile)
        {
            var jProfile = JObject.Parse(strProfile);
            if (jProfile == null) throw new Exception("Failed to correctly process the response");

            var profileJson = jProfile.Value<JArray>("response");
            if (profileJson == null) throw new Exception("Failed to correctly process the response");

            var vkProfiles = profileJson.ToObject<List<VKProfile>>();
            if (vkProfiles.Count == 0) throw new Exception("Failed to correctly process the response");

            var profile = new LoginProfile
                {
                    //EMail = email,
                    Id = vkProfiles[0].uid,
                    FirstName = vkProfiles[0].first_name,
                    LastName = vkProfiles[0].last_name,
                    BirthDay = vkProfiles[0].bdate,

                    Provider = ProviderConstants.VK,
                };

            return profile;
        }

        private class VKProfile
        {
            public string uid = null;
            public string first_name = null;
            public string last_name = null;
            public string bdate = null;
        }
    }
}