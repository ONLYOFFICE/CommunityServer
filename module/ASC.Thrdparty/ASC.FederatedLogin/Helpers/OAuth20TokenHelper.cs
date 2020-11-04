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
using System.Web;
using ASC.Core.Common.Configuration;
using ASC.FederatedLogin.LoginProviders;

namespace ASC.FederatedLogin.Helpers
{
    public static class OAuth20TokenHelper
    {
        public static void RequestCode<T>(HttpContext context, string scope = null, Dictionary<string, string> additionalArgs = null) where T : Consumer, IOAuthProvider, new ()
        {
            var loginProvider = ConsumerFactory.Get<T>();
            var requestUrl = loginProvider.CodeUrl;
            var clientID = loginProvider.ClientID;
            var redirectUri = loginProvider.RedirectUri;

            var uriBuilder = new UriBuilder(requestUrl);

            var query = uriBuilder.Query;
            if (!string.IsNullOrEmpty(query)) query += "&";
            query += "response_type=code";

            if (!string.IsNullOrEmpty(clientID)) query += "&client_id=" + HttpUtility.UrlEncode(clientID);
            if (!string.IsNullOrEmpty(redirectUri)) query += "&redirect_uri=" + HttpUtility.UrlEncode(redirectUri);
            if (!string.IsNullOrEmpty(scope)) query += "&scope=" + HttpUtility.UrlEncode(scope);

            query += "&state=" + HttpUtility.UrlEncode(context.Request.GetUrlRewriter().AbsoluteUri);

            if (additionalArgs != null)
            {
                query = additionalArgs.Keys.Where(additionalArg => additionalArg != null)
                                      .Aggregate(query, (current, additionalArg) =>
                                                        additionalArg != null ? current
                                                                                + ("&" + HttpUtility.UrlEncode((additionalArg).Trim())
                                                                                   + "=" + HttpUtility.UrlEncode((additionalArgs[additionalArg] ?? "").Trim())) : null);
            }

            context.Response.Redirect(uriBuilder.Uri + "?" + query, true);
        }

        public static OAuth20Token GetAccessToken<T>(string authCode) where T : Consumer, IOAuthProvider, new()
        {
            var loginProvider = ConsumerFactory.Get<T>();
            var requestUrl = loginProvider.AccessTokenUrl;
            var clientID = loginProvider.ClientID;
            var clientSecret = loginProvider.ClientSecret;
            var redirectUri = loginProvider.RedirectUri;

            if (String.IsNullOrEmpty(authCode)) throw new ArgumentNullException("authCode");
            if (String.IsNullOrEmpty(clientID)) throw new ArgumentNullException("clientID");
            if (String.IsNullOrEmpty(clientSecret)) throw new ArgumentNullException("clientSecret");

            var data = string.Format("code={0}&client_id={1}&client_secret={2}",
                                     HttpUtility.UrlEncode(authCode),
                                     HttpUtility.UrlEncode(clientID),
                                     HttpUtility.UrlEncode(clientSecret));

            if (!String.IsNullOrEmpty(redirectUri))
                data += "&redirect_uri=" + HttpUtility.UrlEncode(redirectUri);

            data += "&grant_type=authorization_code";

            var json = RequestHelper.PerformRequest(requestUrl, "application/x-www-form-urlencoded", "POST", data);
            if (json != null)
            {
                if (!json.StartsWith("{"))
                {
                    json = "{\"" + json.Replace("=", "\":\"").Replace("&", "\",\"") + "\"}";
                }

                var token = OAuth20Token.FromJson(json);
                if (token == null) return null;

                token.ClientID = clientID;
                token.ClientSecret = clientSecret;
                token.RedirectUri = redirectUri;
                return token;
            }

            return null;
        }

        public static OAuth20Token RefreshToken<T>(OAuth20Token token) where T : Consumer, IOAuthProvider, new()
        {
            var loginProvider = ConsumerFactory.Get<T>();
            return RefreshToken(loginProvider.AccessTokenUrl, token);
        }

        public static OAuth20Token RefreshToken(string requestUrl, OAuth20Token token)
        {
            if (token == null || !CanRefresh(token)) throw new ArgumentException("Can not refresh given token", "token");

            var data = String.Format("client_id={0}&client_secret={1}&refresh_token={2}&grant_type=refresh_token",
                                     HttpUtility.UrlEncode(token.ClientID),
                                     HttpUtility.UrlEncode(token.ClientSecret),
                                     HttpUtility.UrlEncode(token.RefreshToken));

            var json = RequestHelper.PerformRequest(requestUrl, "application/x-www-form-urlencoded", "POST", data);
            if (json != null)
            {
                var refreshed = OAuth20Token.FromJson(json);
                refreshed.ClientID = token.ClientID;
                refreshed.ClientSecret = token.ClientSecret;
                refreshed.RedirectUri = token.RedirectUri;
                refreshed.RefreshToken = refreshed.RefreshToken ?? token.RefreshToken;
                return refreshed;
            }

            return token;
        }

        private static bool CanRefresh(OAuth20Token token)
        {
            return !String.IsNullOrEmpty(token.ClientID) && !String.IsNullOrEmpty(token.ClientSecret);
        }
    }
}
