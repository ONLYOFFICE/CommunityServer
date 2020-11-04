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

        private const string VKProfileUrl = "https://api.vk.com/method/users.get?v=5.103";


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