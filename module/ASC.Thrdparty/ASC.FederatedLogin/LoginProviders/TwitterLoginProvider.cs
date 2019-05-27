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