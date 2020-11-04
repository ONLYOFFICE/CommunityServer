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
using System.Threading;
using System.Web;
using ASC.FederatedLogin.Helpers;
using ASC.FederatedLogin.Profile;
using Newtonsoft.Json.Linq;

namespace ASC.FederatedLogin.LoginProviders
{
    public class YandexLoginProvider : BaseLoginProvider<YandexLoginProvider>
    {
        public override string CodeUrl
        {
            get { return "https://oauth.yandex.ru/authorize"; }
        }

        public override string AccessTokenUrl
        {
            get { return "https://oauth.yandex.ru/token"; }
        }

        public override string ClientID
        {
            get { return this["yandexClientId"]; }
        }

        public override string ClientSecret
        {
            get { return this["yandexClientSecret"]; }
        }

        public override string RedirectUri
        {
            get { return this["yandexRedirectUrl"]; }
        }

        private const string YandexProfileUrl = "https://login.yandex.ru/info";


        public YandexLoginProvider()
        {
        }

        public YandexLoginProvider(string name, int order, Dictionary<string, string> props, Dictionary<string, string> additional = null)
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
                                                              { "force_confirm", "true" }
                                                          }
                                                      : null);

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

        public override LoginProfile GetLoginProfile(string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
                throw new Exception("Login failed");

            return RequestProfile(accessToken);
        }

        private static LoginProfile RequestProfile(string accessToken)
        {
            var yandexProfile = RequestHelper.PerformRequest(YandexProfileUrl + "?format=json&oauth_token=" + accessToken);
            var loginProfile = ProfileFromYandex(yandexProfile);

            return loginProfile;
        }

        private static LoginProfile ProfileFromYandex(string strProfile)
        {
            var jProfile = JObject.Parse(strProfile);
            if (jProfile == null) throw new Exception("Failed to correctly process the response");

            var profile = new LoginProfile
                {
                    EMail = jProfile.Value<string>("default_email"),
                    Id = jProfile.Value<string>("id"),
                    FirstName = jProfile.Value<string>("first_name"),
                    LastName = jProfile.Value<string>("last_name"),
                    DisplayName = jProfile.Value<string>("display_name"),
                    Gender = jProfile.Value<string>("sex"),

                    Provider = ProviderConstants.Yandex,
                };

            return profile;
        }
    }
}