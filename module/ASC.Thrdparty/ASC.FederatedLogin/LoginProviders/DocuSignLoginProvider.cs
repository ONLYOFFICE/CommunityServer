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
using System.Text;
using ASC.Core.Common.Configuration;
using ASC.FederatedLogin.Helpers;

namespace ASC.FederatedLogin.LoginProviders
{
    public class DocuSignLoginProvider : Consumer, IOAuthProvider
    {
        public static DocuSignLoginProvider Instance
        {
            get { return ConsumerFactory.Get<DocuSignLoginProvider>(); }
        }

        public string Scopes { get { return "signature"; } }
        public string CodeUrl { get { return DocuSignHost + "/oauth/auth"; } }
        public string AccessTokenUrl { get { return DocuSignHost + "/oauth/token"; } }
        public string RedirectUri { get { return this["docuSignRedirectUrl"]; } }
        public string ClientID { get { return this["docuSignClientId"]; } }
        public string ClientSecret { get { return this["docuSignClientSecret"]; } }
        public string DocuSignHost { get { return "https://" + this["docuSignHost"]; } }

        public bool IsEnabled
        {
            get
            {
                return !string.IsNullOrEmpty(ClientID) &&
                       !string.IsNullOrEmpty(ClientSecret) &&
                       !string.IsNullOrEmpty(RedirectUri);
            }
        }

        public DocuSignLoginProvider() { }

        public DocuSignLoginProvider(string name, int order, Dictionary<string, string> props, Dictionary<string, string> additional = null)
            : base(name, order, props, additional)
        {
        }


        private string AuthHeader
        {
            get
            {
                var codeAuth = string.Format("{0}:{1}", ClientID, ClientSecret);
                var codeAuthBytes = Encoding.UTF8.GetBytes(codeAuth);
                var codeAuthBase64 = Convert.ToBase64String(codeAuthBytes);
                return "Basic " + codeAuthBase64;
            }
        }

        public OAuth20Token GetAccessToken(string authCode)
        {
            if (string.IsNullOrEmpty(authCode)) throw new ArgumentNullException("authCode");
            if (string.IsNullOrEmpty(ClientID)) throw new ArgumentException("clientID");
            if (string.IsNullOrEmpty(ClientSecret)) throw new ArgumentException("clientSecret");

            var data = string.Format("grant_type=authorization_code&code={0}", authCode);
            var headers = new Dictionary<string, string> {{"Authorization", AuthHeader}};

            var json = RequestHelper.PerformRequest(AccessTokenUrl, "application/x-www-form-urlencoded", "POST", data, headers);
            if (json == null) throw new Exception("Can not get token");

            if (!json.StartsWith("{"))
            {
                json = "{\"" + json.Replace("=", "\":\"").Replace("&", "\",\"") + "\"}";
            }

            var token = OAuth20Token.FromJson(json);
            if (token == null) return null;

            token.ClientID = ClientID;
            token.ClientSecret = ClientSecret;
            token.RedirectUri = RedirectUri;
            return token;
        }

        public OAuth20Token RefreshToken(string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken) || string.IsNullOrEmpty(ClientID) || string.IsNullOrEmpty(ClientSecret))
                throw new ArgumentException("Can not refresh given token");

            var data = string.Format("grant_type=refresh_token&refresh_token={0}", refreshToken);
            var headers = new Dictionary<string, string> {{"Authorization", AuthHeader}};

            var json = RequestHelper.PerformRequest(AccessTokenUrl, "application/x-www-form-urlencoded", "POST", data, headers);
            if (json == null) throw new Exception("Can not get token");

            var refreshed = OAuth20Token.FromJson(json);
            refreshed.ClientID = ClientID;
            refreshed.ClientSecret = ClientSecret;
            refreshed.RedirectUri = RedirectUri;
            refreshed.RefreshToken = refreshed.RefreshToken ?? refreshToken;
            return refreshed;
        }
    }
}