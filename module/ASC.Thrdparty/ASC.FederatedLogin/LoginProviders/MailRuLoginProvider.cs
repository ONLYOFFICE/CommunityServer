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
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;
using ASC.FederatedLogin.Helpers;
using ASC.FederatedLogin.Profile;
using Newtonsoft.Json.Linq;

namespace ASC.FederatedLogin.LoginProviders
{
    public class MailRuLoginProvider : BaseLoginProvider<MailRuLoginProvider>
    {
        public override string CodeUrl
        {
            get { return "https://connect.mail.ru/oauth/authorize"; }
        }

        public override string AccessTokenUrl
        {
            get { return "https://connect.mail.ru/oauth/token"; }
        }

        public override string ClientID
        {
            get { return this["mailRuClientId"]; }
        }

        public override string ClientSecret
        {
            get { return this["mailRuClientSecret"]; }
        }

        public override string RedirectUri
        {
            get { return this["mailRuRedirectUrl"]; }
        }

        private const string MailRuApiUrl = "http://www.appsmail.ru/platform/api";

        public MailRuLoginProvider()
        {
        }

        public MailRuLoginProvider(string name, int order, Dictionary<string, string> props, Dictionary<string, string> additional = null)
            : base(name, order, props, additional)
        {
        }

        public override LoginProfile ProcessAuthoriztion(HttpContext context, IDictionary<string, string> @params)
        {
            try
            {
                var token = Auth(context, Scopes);

                if (token == null)
                {
                    throw new Exception("Login failed");
                }

                var uid = GetUid(token);

                return RequestProfile(token.AccessToken, uid);
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
            throw new NotImplementedException();
        }

        private LoginProfile RequestProfile(string accessToken, string uid)
        {
            var queryDictionary = new Dictionary<string, string>
                {
                    { "app_id", ClientID },
                    { "method", "users.getInfo" },
                    { "secure", "1" },
                    { "session_key", accessToken },
                    { "uids", uid },
                };

            var sortedKeys = queryDictionary.Keys.ToList();
            sortedKeys.Sort();

            var mailruParams = string.Join("", sortedKeys.Select(key => key + "=" + queryDictionary[key]).ToList());
            var sig = string.Join("", MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(mailruParams + ClientSecret)).Select(b => b.ToString("x2")));

            var mailRuProfile = RequestHelper.PerformRequest(
                MailRuApiUrl
                + "?" + string.Join("&", queryDictionary.Select(pair => pair.Key + "=" + HttpUtility.UrlEncode(pair.Value)))
                + "&sig=" + HttpUtility.UrlEncode(sig));
            var loginProfile = ProfileFromMailRu(mailRuProfile);

            return loginProfile;
        }

        private static LoginProfile ProfileFromMailRu(string strProfile)
        {
            var jProfile = JArray.Parse(strProfile);
            if (jProfile == null) throw new Exception("Failed to correctly process the response");

            var mailRuProfiles = jProfile.ToObject<List<MailRuProfile>>();
            if (mailRuProfiles.Count == 0) throw new Exception("Failed to correctly process the response");

            var profile = new LoginProfile
                {
                    EMail = mailRuProfiles[0].email,
                    Id = mailRuProfiles[0].uid,
                    FirstName = mailRuProfiles[0].first_name,
                    LastName = mailRuProfiles[0].last_name,

                    Provider = ProviderConstants.MailRu,
                };

            return profile;
        }

        private class MailRuProfile
        {
            public string uid = null;
            public string first_name = null;
            public string last_name = null;
            public string email = null;
        }

        private static string GetUid(OAuth20Token token)
        {
            if (string.IsNullOrEmpty(token.OriginJson)) return null;
            var parser = JObject.Parse(token.OriginJson);

            return
                parser == null
                    ? null
                    : parser.Value<string>("x_mailru_vid");
        }
    }
}