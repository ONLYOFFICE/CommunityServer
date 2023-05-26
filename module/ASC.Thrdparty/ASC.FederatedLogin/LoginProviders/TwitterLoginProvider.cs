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
using System.Globalization;
using System.Web;

using ASC.Core.Common.Configuration;
using ASC.FederatedLogin.Profile;

using Newtonsoft.Json.Linq;

using Tweetinvi;
using Tweetinvi.Auth;
using Tweetinvi.Parameters;

namespace ASC.FederatedLogin.LoginProviders
{
    public class TwitterLoginProvider : BaseLoginProvider<TwitterLoginProvider>
    {
        public static string TwitterKey { get { return Instance.ClientID; } }
        public static string TwitterSecret { get { return Instance.ClientSecret; } }
        public static string TwitterDefaultAccessToken { get { return Instance["twitterAccessToken_Default"]; } }
        public static string TwitterAccessTokenSecret { get { return Instance["twitterAccessTokenSecret_Default"]; } }

        public override string AccessTokenUrl { get { return "https://api.twitter.com/oauth/access_token"; } }
        public override string RedirectUri { get { return this["twitterRedirectUrl"]; } }
        public override string ClientID { get { return this["twitterKey"]; } }
        public override string ClientSecret { get { return this["twitterSecret"]; } }
        public override string CodeUrl { get { return "https://api.twitter.com/oauth/request_token"; } }

        private static readonly IAuthenticationRequestStore _myAuthRequestStore = new LocalAuthenticationRequestStore();

        public override bool IsEnabled
        {
            get
            {
                return !string.IsNullOrEmpty(ClientID) &&
                       !string.IsNullOrEmpty(ClientSecret);
            }
        }

        public TwitterLoginProvider() { }
        public TwitterLoginProvider(string name, int order, Dictionary<string, Prop> props, Dictionary<string, Prop> additional = null) : base(name, order, props, additional) { }

        public override LoginProfile ProcessAuthoriztion(HttpContext context, IDictionary<string, string> @params)
        {
            if (!string.IsNullOrEmpty(context.Request["denied"]))
            {
                return LoginProfile.FromError(new Exception("Canceled at provider"));
            }

            var appClient = new TwitterClient(TwitterKey, TwitterSecret);

            if (string.IsNullOrEmpty(context.Request["oauth_token"]))
            {
                var callbackAddress = new UriBuilder(RedirectUri)
                {
                    Query = "state=" + HttpUtility.UrlEncode(context.Request.GetUrlRewriter().AbsoluteUri)
                };

                var authenticationRequestId = Guid.NewGuid().ToString();

                // Add the user identifier as a query parameters that will be received by `ValidateTwitterAuth`
                var redirectURL = _myAuthRequestStore.AppendAuthenticationRequestIdToCallbackUrl(callbackAddress.ToString(), authenticationRequestId);

                // Initialize the authentication process
                var authenticationRequestToken = appClient.Auth.RequestAuthenticationUrlAsync(redirectURL)
                                       .ConfigureAwait(false)
                                       .GetAwaiter()
                                       .GetResult();

                // Store the token information in the store
                _myAuthRequestStore.AddAuthenticationTokenAsync(authenticationRequestId, authenticationRequestToken)
                                       .ConfigureAwait(false)
                                       .GetAwaiter()
                                       .GetResult();

                context.Response.Redirect(authenticationRequestToken.AuthorizationURL, true);

                return null;
            }

            // Extract the information from the redirection url
            var requestParameters = RequestCredentialsParameters.FromCallbackUrlAsync(context.Request.RawUrl, _myAuthRequestStore).GetAwaiter().GetResult();
            // Request Twitter to generate the credentials.
            var userCreds = appClient.Auth.RequestCredentialsAsync(requestParameters)
                                       .ConfigureAwait(false)
                                       .GetAwaiter()
                                       .GetResult();

            // Congratulations the user is now authenticated!
            var userClient = new TwitterClient(userCreds);

            var user = userClient.Users.GetAuthenticatedUserAsync()
                                       .ConfigureAwait(false)
                                       .GetAwaiter()
                                       .GetResult();

            var userSettings = userClient.AccountSettings.GetAccountSettingsAsync()
                                       .ConfigureAwait(false)
                                       .GetAwaiter()
                                       .GetResult();

            return user == null
                       ? null
                       : new LoginProfile
                       {
                           Name = user.Name,
                           DisplayName = user.ScreenName,
                           Avatar = user.ProfileImageUrl,
                           Locale = userSettings.Language.ToString(),
                           Id = user.Id.ToString(CultureInfo.InvariantCulture),
                           Provider = ProviderConstants.Twitter
                       };

        }

        protected override OAuth20Token Auth(HttpContext context, string scopes, Dictionary<string, string> additional = null)
        {
            throw new NotImplementedException();
        }

        public override LoginProfile GetLoginProfile(string accessToken)
        {
            throw new NotImplementedException();
        }

        internal static LoginProfile ProfileFromTwitter(string twitterProfile)
        {
            var jProfile = JObject.Parse(twitterProfile);
            if (jProfile == null) throw new Exception("Failed to correctly process the response");

            return new LoginProfile
            {
                DisplayName = jProfile.Value<string>("name"),
                Locale = jProfile.Value<string>("lang"),
                Id = jProfile.Value<string>("id"),
                Provider = ProviderConstants.Twitter
            };
        }
    }
}