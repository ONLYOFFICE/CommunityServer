/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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
using ASC.FederatedLogin.Helpers;
using ASC.FederatedLogin.Profile;
using Newtonsoft.Json.Linq;

namespace ASC.FederatedLogin.LoginProviders
{
    public class MicrosoftLoginProvider : BaseLoginProvider<MicrosoftLoginProvider>
    {
        private const string MicrosoftProfileUrl = "https://graph.microsoft.com/oidc/userinfo";

        public override string AccessTokenUrl { get { return "https://login.microsoftonline.com/consumers/oauth2/v2.0/token"; } }
        public override string RedirectUri { get { return this["microsoftRedirectUrl"]; } }
        public override string ClientID { get { return this["microsoftClientId"]; } }
        public override string ClientSecret { get { return this["microsoftClientSecret"]; } }
        public override string CodeUrl { get { return "https://login.microsoftonline.com/consumers/oauth2/v2.0/authorize"; } }
        public override string Scopes { get { return "openid,email,profile"; } }

        public MicrosoftLoginProvider() { }
        public MicrosoftLoginProvider(string name, int order, Dictionary<string, string> props, Dictionary<string, string> additional = null) : base(name, order, props, additional) {}

        public override LoginProfile GetLoginProfile(string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
                throw new Exception("Login failed");

            return RequestProfile(accessToken);
        }

        private static LoginProfile RequestProfile(string accessToken)
        {
            var openidProfile = RequestHelper.PerformRequest(MicrosoftProfileUrl, headers: new Dictionary<string, string>() { { "Authorization", "Bearer " + accessToken } });
            var loginProfile = ProfileFromMicrosoft(openidProfile);
            return loginProfile;
        }

        internal static LoginProfile ProfileFromMicrosoft(string openidProfile)
        {
            var jProfile = JObject.Parse(openidProfile);
            if (jProfile == null) throw new Exception("Failed to correctly process the response");

            var profile = new LoginProfile
                              {
                                  FirstName = jProfile.Value<string>("given_name"),
                                  LastName = jProfile.Value<string>("family_name"),
                                  EMail = jProfile.Value<string>("email"),
                                  Id = jProfile.Value<string>("sub"),
                                  Provider = ProviderConstants.Microsoft
                              };

            return profile;
        }
    }
}