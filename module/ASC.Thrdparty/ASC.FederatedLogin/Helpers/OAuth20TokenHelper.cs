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
