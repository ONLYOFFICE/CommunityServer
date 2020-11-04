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
    public class FacebookLoginProvider : BaseLoginProvider<FacebookLoginProvider>
    {
        private const string FacebookProfileUrl = "https://graph.facebook.com/v2.7/me?fields=email,id,birthday,link,first_name,last_name,gender,timezone,locale";

        public override string AccessTokenUrl { get { return "https://graph.facebook.com/v2.7/oauth/access_token"; } }
        public override string RedirectUri { get { return this["facebookRedirectUrl"]; } }
        public override string ClientID { get { return this["facebookClientId"]; } }
        public override string ClientSecret { get { return this["facebookClientSecret"]; } }
        public override string CodeUrl { get { return "https://www.facebook.com/v2.7/dialog/oauth/"; } }
        public override string Scopes { get { return "email,public_profile"; } }

        public FacebookLoginProvider() { }
        public FacebookLoginProvider(string name, int order, Dictionary<string, string> props, Dictionary<string, string> additional = null) : base(name, order, props, additional) {}

        public override LoginProfile GetLoginProfile(string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
                throw new Exception("Login failed");

            return RequestProfile(accessToken);
        }

        private static LoginProfile RequestProfile(string accessToken)
        {
            var facebookProfile = RequestHelper.PerformRequest(FacebookProfileUrl + "&access_token=" + accessToken);
            var loginProfile = ProfileFromFacebook(facebookProfile);
            return loginProfile;
        }

        internal static LoginProfile ProfileFromFacebook(string facebookProfile)
        {
            var jProfile = JObject.Parse(facebookProfile);
            if (jProfile == null) throw new Exception("Failed to correctly process the response");

            var profile = new LoginProfile
                              {
                                  BirthDay = jProfile.Value<string>("birthday"),
                                  Link = jProfile.Value<string>("link"),
                                  FirstName = jProfile.Value<string>("first_name"),
                                  LastName = jProfile.Value<string>("last_name"),
                                  Gender = jProfile.Value<string>("gender"),
                                  EMail = jProfile.Value<string>("email"),
                                  Id = jProfile.Value<string>("id"),
                                  TimeZone = jProfile.Value<string>("timezone"),
                                  Locale = jProfile.Value<string>("locale"),
                                  Provider = ProviderConstants.Facebook,
                                  Avatar = string.Format("http://graph.facebook.com/{0}/picture?type=large", jProfile.Value<string>("id"))
                              };

            return profile;
        }
    }
}