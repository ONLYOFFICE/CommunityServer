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
using ASC.FederatedLogin.Profile;

namespace ASC.FederatedLogin.LoginProviders
{
    public class YahooLoginProvider : BaseLoginProvider<YahooLoginProvider>
    {
        public const string YahooUrlUserGuid = "https://social.yahooapis.com/v1/me/guid";
        public const string YahooUrlContactsFormat = "https://social.yahooapis.com/v1/user/{0}/contacts";

        public override string CodeUrl { get { return "https://api.login.yahoo.com/oauth2/request_auth"; } }
        public override string AccessTokenUrl { get { return "https://api.login.yahoo.com/oauth2/get_token"; } }
        public override string RedirectUri { get { return this["yahooRedirectUrl"]; } }
        public override string ClientID { get { return this["yahooClientId"]; } }
        public override string ClientSecret { get { return this["yahooClientSecret"]; } }
        public override string Scopes { get { return "sdct-r"; } }

        public YahooLoginProvider() { }
        public YahooLoginProvider(string name, int order, Dictionary<string, string> props, Dictionary<string, string> additional = null) : base(name, order, props, additional) { }

        public OAuth20Token Auth(HttpContext context)
        {
            return Auth(context, Scopes);
        }

        public override LoginProfile GetLoginProfile(string accessToken)
        {
            throw new NotImplementedException();
        }
    }
}
