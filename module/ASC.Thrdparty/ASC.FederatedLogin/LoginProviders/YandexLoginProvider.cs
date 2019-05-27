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