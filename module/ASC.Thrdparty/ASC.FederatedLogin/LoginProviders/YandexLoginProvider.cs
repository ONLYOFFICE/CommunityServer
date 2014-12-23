/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Collections.Generic;
using System.Threading;
using System.Web;
using ASC.FederatedLogin.Helpers;
using ASC.FederatedLogin.Profile;
using ASC.Thrdparty;
using ASC.Thrdparty.Configuration;
using Newtonsoft.Json.Linq;

namespace ASC.FederatedLogin.LoginProviders
{
    public class YandexLoginProvider : ILoginProvider
    {
        private const string YandexProfileUrl = "https://login.yandex.ru/info";

        private const string YandexOauthUrl = "https://oauth.yandex.ru/";
        public const string YandexOauthCodeUrl = YandexOauthUrl + "authorize";
        public const string YandexOauthTokenUrl = YandexOauthUrl + "token";


        public static string YandexOAuth20ClientId
        {
            get { return KeyStorage.Get("yandexClientId"); }
        }

        public static string YandexOAuth20ClientSecret
        {
            get { return KeyStorage.Get("yandexClientSecret"); }
        }

        public LoginProfile ProcessAuthoriztion(HttpContext context, IDictionary<string, string> @params)
        {
            try
            {
                var token = Auth(context);
                return RequestProfile(token);
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

        public static OAuth20Token Auth(HttpContext context)
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
                OAuth20TokenHelper.RequestCode(HttpContext.Current,
                                               YandexOauthCodeUrl,
                                               YandexOAuth20ClientId,
                                               "",
                                               "");
                return null;
            }

            var token = OAuth20TokenHelper.GetAccessToken(YandexOauthTokenUrl,
                                                          YandexOAuth20ClientId,
                                                          YandexOAuth20ClientSecret,
                                                          "",
                                                          code);
            return token;
        }

        private static LoginProfile RequestProfile(OAuth20Token token)
        {
            var yandexProfile = RequestHelper.PerformRequest(YandexProfileUrl + "?format=json&oauth_token=" + token.AccessToken);
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