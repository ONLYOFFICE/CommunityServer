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
using System.Web;
using ASC.FederatedLogin.Helpers;
using ASC.Thrdparty.Configuration;

namespace ASC.FederatedLogin.LoginProviders
{
    public class YahooLoginProvider
    {
        public const string YahooOauthCodeUrl = "https://api.login.yahoo.com/oauth2/request_auth";
        public const string YahooOauthTokenUrl = "https://api.login.yahoo.com/oauth2/get_token";

        public const string YahooScopeContacts = "sdct-r";

        public const string YahooUrlUserGuid = "https://social.yahooapis.com/v1/me/guid";
        public const string YahooUrlContactsFormat = "https://social.yahooapis.com/v1/user/{0}/contacts";


        public static string YahooOAuth20ClientId
        {
            get { return KeyStorage.Get("yahooClientId"); }
        }

        public static string YahooOAuth20ClientSecret
        {
            get { return KeyStorage.Get("yahooClientSecret"); }
        }

        public static string YahooOAuth20RedirectUrl
        {
            get { return KeyStorage.Get("yahooRedirectUrl"); }
        }

        public static OAuth20Token Auth(HttpContext context, string scopes)
        {
            var error = context.Request["error"];
            if (!string.IsNullOrEmpty(error))
            {
                if (error == "access_denied")
                {
                    error = "Canceled at provider";
                }
                throw new Exception(error);
            }

            var code = context.Request["code"];
            if (string.IsNullOrEmpty(code))
            {
                var additionalArgs =
                    (context.Request["access_type"] ?? "") == "offline"
                        ? new Dictionary<string, string>
                            {
                                {"access_type", "offline"},
                                {"approval_prompt", "force"}
                            }
                        : null;

                OAuth20TokenHelper.RequestCode(HttpContext.Current,
                                               YahooOauthCodeUrl,
                                               YahooOAuth20ClientId,
                                               YahooOAuth20RedirectUrl,
                                               scopes,
                                               additionalArgs);
                return null;
            }

            var token = OAuth20TokenHelper.GetAccessToken(YahooOauthTokenUrl,
                                                          YahooOAuth20ClientId,
                                                          YahooOAuth20ClientSecret,
                                                          YahooOAuth20RedirectUrl,
                                                          code);
            return token;
        }
    }
}
