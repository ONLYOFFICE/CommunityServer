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
using System.Text;
using ASC.FederatedLogin.Helpers;
using ASC.Thrdparty.Configuration;

namespace ASC.FederatedLogin.LoginProviders
{
    public class DocuSignLoginProvider
    {
        public static string DocuSignHost
        {
            get { return "https://" + KeyStorage.Get("docuSignHost"); }
        }

        public static string DocuSignOauthCodeUrl
        {
            get { return DocuSignHost + "/oauth/auth"; }
        }

        public static string DocuSignOauthTokenUrl
        {
            get { return DocuSignHost + "/oauth/token"; }
        }

        public const string DocuSignScope = "signature";

        public static string DocuSignOAuth20ClientId
        {
            get { return KeyStorage.Get("docuSignClientId"); }
        }

        public static string DocuSignOAuth20ClientSecret
        {
            get { return KeyStorage.Get("docuSignClientSecret"); }
        }

        public static string DocuSignOAuth20RedirectUrl
        {
            get { return KeyStorage.Get("docuSignRedirectUrl"); }
        }


        private static string AuthHeader
        {
            get
            {
                var codeAuth = string.Format("{0}:{1}", DocuSignOAuth20ClientId, DocuSignOAuth20ClientSecret);
                var codeAuthBytes = Encoding.UTF8.GetBytes(codeAuth);
                var codeAuthBase64 = Convert.ToBase64String(codeAuthBytes);
                return "Basic " + codeAuthBase64;
            }
        }

        public static OAuth20Token GetAccessToken(string authCode)
        {
            if (string.IsNullOrEmpty(authCode)) throw new ArgumentNullException("authCode");
            if (string.IsNullOrEmpty(DocuSignOAuth20ClientId)) throw new ArgumentException("clientID");
            if (string.IsNullOrEmpty(DocuSignOAuth20ClientSecret)) throw new ArgumentException("clientSecret");

            var data = string.Format("grant_type=authorization_code&code={0}", authCode);
            var headers = new Dictionary<string, string> {{"Authorization", AuthHeader}};

            var json = RequestHelper.PerformRequest(DocuSignOauthTokenUrl, "application/x-www-form-urlencoded", "POST", data, headers);
            if (json == null) throw new Exception("Can not get token");

            if (!json.StartsWith("{"))
            {
                json = "{\"" + json.Replace("=", "\":\"").Replace("&", "\",\"") + "\"}";
            }

            var token = OAuth20Token.FromJson(json);
            if (token == null) return null;

            token.ClientID = DocuSignOAuth20ClientId;
            token.ClientSecret = DocuSignOAuth20ClientSecret;
            token.RedirectUri = DocuSignOAuth20RedirectUrl;
            return token;
        }

        public static OAuth20Token RefreshToken(string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken) || string.IsNullOrEmpty(DocuSignOAuth20ClientId) || string.IsNullOrEmpty(DocuSignOAuth20ClientSecret))
                throw new ArgumentException("Can not refresh given token");

            var data = string.Format("grant_type=refresh_token&refresh_token={0}", refreshToken);
            var headers = new Dictionary<string, string> {{"Authorization", AuthHeader}};

            var json = RequestHelper.PerformRequest(DocuSignOauthTokenUrl, "application/x-www-form-urlencoded", "POST", data, headers);
            if (json == null) throw new Exception("Can not get token");

            var refreshed = OAuth20Token.FromJson(json);
            refreshed.ClientID = DocuSignOAuth20ClientId;
            refreshed.ClientSecret = DocuSignOAuth20ClientSecret;
            refreshed.RedirectUri = DocuSignOAuth20RedirectUrl;
            refreshed.RefreshToken = refreshed.RefreshToken ?? refreshToken;
            return refreshed;
        }

    }
}