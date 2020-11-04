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
using System.Globalization;
using System.Web;
using ASC.FederatedLogin.Profile;
using Twitterizer;

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

        public override bool IsEnabled
        {
            get
            {
                return !string.IsNullOrEmpty(ClientID) &&
                       !string.IsNullOrEmpty(ClientSecret);
            }
        }

        public TwitterLoginProvider() { }
        public TwitterLoginProvider(string name, int order, Dictionary<string, string> props, Dictionary<string, string> additional = null) : base(name, order, props, additional) { }

        public override LoginProfile ProcessAuthoriztion(HttpContext context, IDictionary<string, string> @params)
        {
            if (!string.IsNullOrEmpty(context.Request["denied"]))
            {
                return LoginProfile.FromError(new Exception("Canceled at provider"));
            }

            if (string.IsNullOrEmpty(context.Request["oauth_token"]))
            {
                var callbackAddress = new UriBuilder(RedirectUri)
                    {
                        Query = "state=" + HttpUtility.UrlEncode(context.Request.GetUrlRewriter().AbsoluteUri)
                    };

                var reqToken = OAuthUtility.GetRequestToken(TwitterKey, TwitterSecret, callbackAddress.ToString());
                var url = OAuthUtility.BuildAuthorizationUri(reqToken.Token).ToString();
                context.Response.Redirect(url, true);
                return null;
            }

            var requestToken = context.Request["oauth_token"];
            var pin = context.Request["oauth_verifier"];

            var tokens = OAuthUtility.GetAccessToken(TwitterKey, TwitterSecret, requestToken, pin);

            var accesstoken = new OAuthTokens
                {
                    AccessToken = tokens.Token,
                    AccessTokenSecret = tokens.TokenSecret,
                    ConsumerKey = TwitterKey,
                    ConsumerSecret = TwitterSecret
                };

            var account = TwitterAccount.VerifyCredentials(accesstoken).ResponseObject;
            return ProfileFromTwitter(account);
        }

        protected override OAuth20Token Auth(HttpContext context, string scopes, Dictionary<string, string> additional = null)
        {
            throw new NotImplementedException();
        }

        public override LoginProfile GetLoginProfile(string accessToken)
        {
            throw new NotImplementedException();
        }

        internal static LoginProfile ProfileFromTwitter(TwitterUser twitterUser)
        {
            return twitterUser == null
                       ? null
                       : new LoginProfile
                           {
                               Name = twitterUser.Name,
                               DisplayName = twitterUser.ScreenName,
                               Avatar = twitterUser.ProfileImageSecureLocation,
                               TimeZone = twitterUser.TimeZone,
                               Locale = twitterUser.Language,
                               Id = twitterUser.Id.ToString(CultureInfo.InvariantCulture),
                               Provider = ProviderConstants.Twitter
                           };
        }
    }
}