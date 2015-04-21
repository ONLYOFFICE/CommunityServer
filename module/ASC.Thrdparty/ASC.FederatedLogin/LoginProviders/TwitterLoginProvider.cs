/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using System.Xml.Linq;
using System.Xml.XPath;
using ASC.FederatedLogin.Helpers;
using ASC.FederatedLogin.Profile;
using ASC.Thrdparty;
using ASC.Thrdparty.Configuration;
using TweetSharp;

namespace ASC.FederatedLogin.LoginProviders
{
    public class TwitterLoginProvider : ILoginProvider
    {
        public LoginProfile ProcessAuthoriztion(HttpContext context, IDictionary<string, string> @params)
        {
            var twitterService = new TwitterService(KeyStorage.Get("twitterKey"), KeyStorage.Get("twitterSecret"));

            if (String.IsNullOrEmpty(context.Request["oauth_token"]) ||
                String.IsNullOrEmpty(context.Request["oauth_verifier"]))
            {
                var requestToken = twitterService.GetRequestToken(context.Request.GetUrlRewriter().AbsoluteUri);

                var uri = twitterService.GetAuthorizationUri(requestToken);

                context.Response.Redirect(uri.ToString(), true);
            }
            else
            {
                var requestToken = new OAuthRequestToken {Token = context.Request["oauth_token"]};
                var accessToken = twitterService.GetAccessToken(requestToken, context.Request["oauth_verifier"]);
                twitterService.AuthenticateWith(accessToken.Token, accessToken.TokenSecret);

                var user = twitterService.VerifyCredentials(new VerifyCredentialsOptions());

                return ProfileFromTwitter(user);
            }

            return new LoginProfile();

        }

        public LoginProfile GetLoginProfile(string accessToken)
        {
            var twitterService = new TwitterService(KeyStorage.Get("twitterKey"), KeyStorage.Get("twitterSecret"));

            //??? tokenSecret
            twitterService.AuthenticateWith(accessToken, null);

            var user = twitterService.VerifyCredentials(new VerifyCredentialsOptions());

            return ProfileFromTwitter(user);
        }

        internal static LoginProfile ProfileFromTwitter(TwitterUser twitterUser)
        {
            return twitterUser == null
                       ? null
                       : new LoginProfile
                           {
                               Name = twitterUser.Name,
                               DisplayName = twitterUser.ScreenName,
                               Avatar = twitterUser.ProfileImageUrl,
                               TimeZone = twitterUser.TimeZone,
                               Locale = twitterUser.Location,
                               Id = twitterUser.Id.ToString(CultureInfo.InvariantCulture),
                               Link = twitterUser.Url,
                               Provider = ProviderConstants.Twitter
                           };
        }

        internal static LoginProfile ProfileFromTwitter(XDocument info)
        {
            var nav = info.CreateNavigator();
            var profile = new LoginProfile
                {
                    Name = nav.SelectNodeValue("//screen_name"),
                    DisplayName = nav.SelectNodeValue("//name"),
                    Avatar = nav.SelectNodeValue("//profile_image_url"),
                    TimeZone = nav.SelectNodeValue("//time_zone"),
                    Locale = nav.SelectNodeValue("//lang"),
                    Id = nav.SelectNodeValue("//id"),
                    Link = nav.SelectNodeValue("//url"),
                    Provider = ProviderConstants.Twitter
                };
            return profile;
        }
    }
}