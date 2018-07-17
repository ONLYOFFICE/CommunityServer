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
 * Pursuant to Section 7 В§ 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 В§ 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using ASC.FederatedLogin.Helpers;
using ASC.Thrdparty.Configuration;

namespace ASC.FederatedLogin.LoginProviders
{
    public class WordpressLoginProvider
    {
        public const string WordpressOauthCodeUrl   = "https://public-api.wordpress.com/oauth2/authorize";
        public const string WordpressOauthTokenUrl  = "https://public-api.wordpress.com/oauth2/token";
        public const string WordpressMeInfoUrl      = "https://public-api.wordpress.com/rest/v1/me";
        public const string WordpressSites          = "https://public-api.wordpress.com/rest/v1.2/sites/";

        public static string WordpressOAuth20ClientId
        {
            get { return KeyStorage.Get("wpClientId"); }
        }

        public static string WordpressOAuth20ClientSecret
        {
            get { return KeyStorage.Get("wpClientSecret"); }
        }

        public static string WordpressOAuth20RedirectUrl
        {
            get { return KeyStorage.Get("wpRedirectUrl"); }
        }

        //GetAccessToken
        private static string AuthHeader
        {
            get
            {
                var codeAuth = string.Format("{0}:{1}", WordpressOAuth20ClientId, WordpressOAuth20ClientSecret);
                var codeAuthBytes = Encoding.UTF8.GetBytes(codeAuth);
                var codeAuthBase64 = Convert.ToBase64String(codeAuthBytes);
                return "Basic " + codeAuthBase64;
            }
        }

        public static OAuth20Token GetAccessToken(string authCode)
        {
            try
            {
                var token = OAuth20TokenHelper.GetAccessToken(WordpressOauthTokenUrl,
                                                          WordpressOAuth20ClientId,
                                                          WordpressOAuth20ClientSecret,
                                                          WordpressOAuth20RedirectUrl, authCode);
                return token;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string GetWordpressMeInfo(string token)
        {
            var headers = new Dictionary<string, string>()
                {
                    {"Authorization",  "bearer " + token}
                };
            return RequestHelper.PerformRequest(WordpressMeInfoUrl,"","GET","", headers);
        }

        public static bool CreateWordpressPost(string title, string content, string status, string blogId, OAuth20Token token)
        {
            try
            {
                var uri = WordpressSites + blogId + "/posts/new";
                const string contentType = "application/x-www-form-urlencoded";
                const string method = "POST";
                var body = "title=" + HttpUtility.UrlEncode(title) + "&content=" + HttpUtility.UrlEncode(content) + "&status=" + status + "&format=standard";
                var headers = new Dictionary<string, string>()
                    {
                        {"Authorization", "bearer " + token.AccessToken}
                    };

                RequestHelper.PerformRequest(uri, contentType, method, body, headers);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
           
        }
    }
}