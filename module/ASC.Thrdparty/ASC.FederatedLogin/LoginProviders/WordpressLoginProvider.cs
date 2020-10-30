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
using System.Web;
using ASC.FederatedLogin.Helpers;
using ASC.FederatedLogin.Profile;

namespace ASC.FederatedLogin.LoginProviders
{
    public class WordpressLoginProvider : BaseLoginProvider<WordpressLoginProvider>
    {
        public const string WordpressMeInfoUrl      = "https://public-api.wordpress.com/rest/v1/me";
        public const string WordpressSites          = "https://public-api.wordpress.com/rest/v1.2/sites/";

        public WordpressLoginProvider()
        {
        }

        public WordpressLoginProvider(string name, int order, Dictionary<string, string> props, Dictionary<string, string> additional = null) : base(name, order, props, additional)
        {
        }

        public static string GetWordpressMeInfo(string token)
        {
            var headers = new Dictionary<string, string>
                {
                    { "Authorization", "bearer " + token }
                };
            return RequestHelper.PerformRequest(WordpressMeInfoUrl, "", "GET", "", headers);
        }

        public static bool CreateWordpressPost(string title, string content, string status, string blogId, OAuth20Token token)
        {
            try
            {
                var uri = WordpressSites + blogId + "/posts/new";
                const string contentType = "application/x-www-form-urlencoded";
                const string method = "POST";
                var body = "title=" + HttpUtility.UrlEncode(title) + "&content=" + HttpUtility.UrlEncode(content) + "&status=" + status + "&format=standard";
                var headers = new Dictionary<string, string>
                    {
                        { "Authorization", "bearer " + token.AccessToken }
                    };

                RequestHelper.PerformRequest(uri, contentType, method, body, headers);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override string CodeUrl
        {
            get { return "https://public-api.wordpress.com/oauth2/authorize"; }
        }

        public override string AccessTokenUrl
        {
            get { return "https://public-api.wordpress.com/oauth2/token"; }
        }

        public override string RedirectUri
        {
            get { return this["wpRedirectUrl"]; }
        }

        public override string ClientID
        {
            get { return this["wpClientId"]; }
        }

        public override string ClientSecret
        {
            get { return this["wpClientSecret"]; }
        }

        public override LoginProfile GetLoginProfile(string accessToken)
        {
            throw new NotImplementedException();
        }
    }
}