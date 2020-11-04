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
using ASC.FederatedLogin.Helpers;
using ASC.FederatedLogin.Profile;
using Newtonsoft.Json.Linq;

namespace ASC.FederatedLogin.LoginProviders
{
    public class LinkedInLoginProvider : BaseLoginProvider<LinkedInLoginProvider>
    {
        private const string LinkedInProfileUrl = "https://api.linkedin.com/v2/me";
        private const string LinkedInEmailUrl = "https://api.linkedin.com/v2/emailAddress?q=members&projection=(elements*(handle~))";

        public override string AccessTokenUrl
        {
            get { return "https://www.linkedin.com/oauth/v2/accessToken"; }
        }

        public override string RedirectUri
        {
            get { return this["linkedInRedirectUrl"]; }
        }

        public override string ClientID
        {
            get { return this["linkedInKey"]; }
        }

        public override string ClientSecret
        {
            get { return this["linkedInSecret"]; }
        }

        public override string CodeUrl
        {
            get { return "https://www.linkedin.com/oauth/v2/authorization"; }
        }

        public override string Scopes
        {
            get { return "r_liteprofile r_emailaddress"; }
        }

        public LinkedInLoginProvider() { }
        public LinkedInLoginProvider(string name, int order, Dictionary<string, string> props, Dictionary<string, string> additional = null) : base(name, order, props, additional) { }

        public override LoginProfile GetLoginProfile(string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
                throw new Exception("Login failed");

            return RequestProfile(accessToken);
        }

        private static LoginProfile RequestProfile(string accessToken)
        {
            var linkedInProfile = RequestHelper.PerformRequest(LinkedInProfileUrl,
                headers: new Dictionary<string, string> {{"Authorization", "Bearer " + accessToken}});
            var loginProfile = ProfileFromLinkedIn(linkedInProfile);

            var linkedInEmail = RequestHelper.PerformRequest(LinkedInEmailUrl,
                headers: new Dictionary<string, string> {{"Authorization", "Bearer " + accessToken}});
            loginProfile.EMail = EmailFromLinkedIn(linkedInEmail);

            return loginProfile;
        }

        internal static LoginProfile ProfileFromLinkedIn(string linkedInProfile)
        {
            var jProfile = JObject.Parse(linkedInProfile);
            if (jProfile == null) throw new Exception("Failed to correctly process the response");

            var profile = new LoginProfile
            {
                Id = jProfile.Value<string>("id"),
                FirstName = jProfile.Value<string>("localizedFirstName"),
                LastName = jProfile.Value<string>("localizedLastName"),
                EMail = jProfile.Value<string>("emailAddress"),
                Provider = ProviderConstants.LinkedIn,
            };

            return profile;
        }

        internal static string EmailFromLinkedIn(string linkedInEmail)
        {
            var jEmail = JObject.Parse(linkedInEmail);
            if (jEmail == null) throw new Exception("Failed to correctly process the response");

            return jEmail.SelectToken("elements[0].handle~.emailAddress").ToString();
        }
    }
}