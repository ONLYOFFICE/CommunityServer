/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using System.Linq;
using System.Threading;
using System.Web;
using ASC.FederatedLogin.Helpers;
using ASC.FederatedLogin.Profile;
using Newtonsoft.Json.Linq;

namespace ASC.FederatedLogin.LoginProviders
{
    public class VKLoginProvider : BaseLoginProvider<VKLoginProvider>
    {
        public override string CodeUrl
        {
            get { return "https://oauth.vk.com/authorize"; }
        }

        public override string AccessTokenUrl
        {
            get { return "https://oauth.vk.com/access_token"; }
        }

        public override string ClientID
        {
            get { return this["vkClientId"]; }
        }

        public override string ClientSecret
        {
            get { return this["vkClientSecret"]; }
        }

        public override string RedirectUri
        {
            get { return this["vkRedirectUrl"]; }
        }

        public override string Scopes
        {
            get { return (new[] { 4194304 }).Sum().ToString(); }
        }

        private const string VKProfileUrl = "https://api.vk.com/method/users.get?v=5.80";


        public VKLoginProvider()
        {
        }

        public VKLoginProvider(string name, int order, Dictionary<string, string> props, Dictionary<string, string> additional = null)
            : base(name, order, props, additional)
        {
        }


        public override LoginProfile ProcessAuthoriztion(HttpContext context, IDictionary<string, string> @params)
        {
            try
            {
                var token = Auth(context, Scopes, (context.Request["access_type"] ?? "") == "offline"
                                                      ? new Dictionary<string, string>
                                                          {
                                                              { "revoke", "1" }
                                                          }
                                                      : null);

                if (token == null)
                {
                    throw new Exception("Login failed");
                }

                var loginProfile = GetLoginProfile(token.AccessToken);

                loginProfile.EMail = GetMail(token);

                return loginProfile;
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

        public override LoginProfile GetLoginProfile(string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
                throw new Exception("Login failed");

            return RequestProfile(accessToken);
        }

        private static LoginProfile RequestProfile(string accessToken)
        {
            var fields = new[] { "sex" };
            var vkProfile = RequestHelper.PerformRequest(VKProfileUrl + "&fields=" + HttpUtility.UrlEncode(string.Join(",", fields)) + "&access_token=" + accessToken);
            var loginProfile = ProfileFromVK(vkProfile);

            return loginProfile;
        }

        private static LoginProfile ProfileFromVK(string strProfile)
        {
            var jProfile = JObject.Parse(strProfile);
            if (jProfile == null) throw new Exception("Failed to correctly process the response");

            var error = jProfile.Value<JObject>("error");
            if (error != null) throw new Exception(error.Value<string>("error_msg"));

            var profileJson = jProfile.Value<JArray>("response");
            if (profileJson == null) throw new Exception("Failed to correctly process the response");

            var vkProfiles = profileJson.ToObject<List<VKProfile>>();
            if (vkProfiles.Count == 0) throw new Exception("Failed to correctly process the response");

            var profile = new LoginProfile
                {
                    Id = vkProfiles[0].id,
                    FirstName = vkProfiles[0].first_name,
                    LastName = vkProfiles[0].last_name,

                    Provider = ProviderConstants.VK,
                };

            return profile;
        }

        private class VKProfile
        {
            public string id = null;
            public string first_name = null;
            public string last_name = null;
        }

        private static string GetMail(OAuth20Token token)
        {
            if (string.IsNullOrEmpty(token.OriginJson)) return null;
            var parser = JObject.Parse(token.OriginJson);

            return
                parser == null
                    ? null
                    : parser.Value<string>("email");
        }
    }
}