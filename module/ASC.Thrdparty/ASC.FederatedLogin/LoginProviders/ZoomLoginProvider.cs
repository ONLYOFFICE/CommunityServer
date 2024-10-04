/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;

using ASC.Core.Common.Configuration;
using ASC.FederatedLogin.Helpers;
using ASC.FederatedLogin.Profile;

using Newtonsoft.Json;

namespace ASC.FederatedLogin.LoginProviders
{
    public class ZoomLoginProvider : BaseLoginProvider<ZoomLoginProvider>
    {
        public override string AccessTokenUrl => "https://zoom.us/oauth/token";
        public override string RedirectUri => this["zoomRedirectUrl"];
        public override string ClientID => this["zoomClientId"];
        public override string ClientSecret => this["zoomClientSecret"];
        public override string CodeUrl => "https://zoom.us/oauth/authorize";
        public override string Scopes => "";

        // used in ZoomService
        public const string ApiUrl = "https://api.zoom.us/v2";
        private const string UserProfileUrl = "https://api.zoom.us/v2/users/me";


        public ZoomLoginProvider() { }
        public ZoomLoginProvider(string name, int order, Dictionary<string, Prop> props, Dictionary<string, Prop> additional = null)
                : base(name, order, props, additional)
        {
        }

        public override LoginProfile ProcessAuthoriztion(HttpContext context, IDictionary<string, string> @params)
        {
            try
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
                    var additionalArgs = new Dictionary<string, string>(@params);
                    OAuth20TokenHelper.RequestCode<ZoomLoginProvider>(context, Scopes, additionalArgs);
                }

                var token = GetAccessToken(code);
                return GetLoginProfile(token);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return new LoginProfile() { AuthorizationError = ex.Message };
            }
        }

        // used in ZoomService
        public OAuth20Token GetAccessToken(string code, string redirectUri = null, string codeVerifier = null)
        {
            var clientPair = $"{ClientID}:{ClientSecret}";
            var base64ClientPair = Convert.ToBase64String(Encoding.UTF8.GetBytes(clientPair));

            var body = new Dictionary<string, string>
        {
            { "code", code },
            { "grant_type", "authorization_code" },
            { "redirect_uri", redirectUri ?? RedirectUri }
        };

            if (codeVerifier != null)
            {
                body.Add("code_verifier", codeVerifier);
            }

            var json = RequestHelper.PerformRequest(AccessTokenUrl, "application/x-www-form-urlencoded", "POST",
                body: string.Join("&", body.Select(kv => $"{HttpUtility.UrlEncode(kv.Key)}={HttpUtility.UrlEncode(kv.Value)}")),
                headers: new Dictionary<string, string> { { "Authorization", $"Basic {base64ClientPair}" } }
            );

            return OAuth20Token.FromJson(json);
        }

        public override LoginProfile GetLoginProfile(string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                throw new Exception("Login failed");
            }

            var (loginProfile, _) = RequestProfile(accessToken);

            return loginProfile;
        }

        public (LoginProfile, ZoomProfile) GetLoginProfileAndRaw(string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                throw new Exception("Login failed");
            }

            var (loginProfile, raw) = RequestProfile(accessToken);

            return (loginProfile, raw);
        }

        public LoginProfile GetMinimalProfile(string uid)
        {
            return new LoginProfile
            {
                Id = uid,
                Provider = ProviderConstants.Zoom
            };
        }

        private (LoginProfile, ZoomProfile) ProfileFromZoom(string zoomProfile)
        {
            var jsonProfile = JsonConvert.DeserializeObject<ZoomProfile>(zoomProfile);

            var profile = new LoginProfile
            {
                Id = jsonProfile.Id,
                Avatar = jsonProfile.PicUrl?.ToString(),
                EMail = jsonProfile.Email,
                FirstName = jsonProfile.FirstName,
                LastName = jsonProfile.LastName,
                Locale = jsonProfile.Language,
                TimeZone = jsonProfile.Timezone,
                DisplayName = jsonProfile.DisplayName,
                Provider = ProviderConstants.Zoom
            };

            return (profile, jsonProfile);
        }

        private (LoginProfile, ZoomProfile) RequestProfile(string accessToken)
        {
            var json = RequestHelper.PerformRequest(UserProfileUrl, headers: new Dictionary<string, string> { { "Authorization", "Bearer " + accessToken } });
            var (loginProfile, jsonProfile) = ProfileFromZoom(json);

            return (loginProfile, jsonProfile);
        }

        public class ZoomProfile
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("first_name")]
            public string FirstName { get; set; }

            [JsonProperty("last_name")]
            public string LastName { get; set; }

            [JsonProperty("display_name")]
            public string DisplayName { get; set; }

            [JsonProperty("email")]
            public string Email { get; set; }

            [JsonProperty("role_name")]
            public string RoleName { get; set; }

            [JsonProperty("pmi")]
            public long Pmi { get; set; }

            [JsonProperty("use_pmi")]
            public bool UsePmi { get; set; }

            [JsonProperty("personal_meeting_url")]
            public Uri PersonalMeetingUrl { get; set; }

            [JsonProperty("timezone")]
            public string Timezone { get; set; }

            [JsonProperty("created_at")]
            public DateTimeOffset CreatedAt { get; set; }

            [JsonProperty("last_login_time")]
            public DateTimeOffset LastLoginTime { get; set; }

            [JsonProperty("pic_url")]
            public Uri PicUrl { get; set; }

            [JsonProperty("jid")]
            public string Jid { get; set; }

            [JsonProperty("account_id")]
            public string AccountId { get; set; }

            [JsonProperty("language")]
            public string Language { get; set; }

            [JsonProperty("phone_country")]
            public string PhoneCountry { get; set; }

            [JsonProperty("phone_number")]
            public string PhoneNumber { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("job_title")]
            public string JobTitle { get; set; }

            [JsonProperty("location")]
            public string Location { get; set; }

            [JsonProperty("account_number")]
            public long AccountNumber { get; set; }

            [JsonProperty("cluster")]
            public string Cluster { get; set; }

            [JsonProperty("user_created_at")]
            public DateTimeOffset UserCreatedAt { get; set; }
        }
    }
}