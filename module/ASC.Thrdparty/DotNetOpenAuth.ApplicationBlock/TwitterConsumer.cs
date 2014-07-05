/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

//-----------------------------------------------------------------------
// <copyright file="TwitterConsumer.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DotNetOpenAuth.ApplicationBlock
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Web;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using DotNetOpenAuth.Messaging;
    using DotNetOpenAuth.OAuth;
    using DotNetOpenAuth.OAuth.ChannelElements;

    /// <summary>
    /// A consumer capable of communicating with Twitter.
    /// </summary>
    public static class TwitterConsumer
    {
        /// <summary>
        /// The description of Twitter's OAuth protocol URIs for use with actually reading/writing
        /// a user's private Twitter data.
        /// </summary>
        public static readonly ServiceProviderDescription ServiceDescription = new ServiceProviderDescription
        {
            RequestTokenEndpoint = new MessageReceivingEndpoint("https://twitter.com/oauth/request_token", HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.AuthorizationHeaderRequest),
            UserAuthorizationEndpoint = new MessageReceivingEndpoint("https://twitter.com/oauth/authorize", HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.AuthorizationHeaderRequest),
            AccessTokenEndpoint = new MessageReceivingEndpoint("https://twitter.com/oauth/access_token", HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.AuthorizationHeaderRequest),
            TamperProtectionElements = new ITamperProtectionChannelBindingElement[] { new HmacSha1SigningBindingElement() },
        };

        /// <summary>
        /// The description of Twitter's OAuth protocol URIs for use with their "Sign in with Twitter" feature.
        /// </summary>
        public static readonly ServiceProviderDescription SignInWithTwitterServiceDescription = new ServiceProviderDescription
        {
            RequestTokenEndpoint = new MessageReceivingEndpoint("https://twitter.com/oauth/request_token", HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.AuthorizationHeaderRequest),
            UserAuthorizationEndpoint = new MessageReceivingEndpoint("https://twitter.com/oauth/authenticate", HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.AuthorizationHeaderRequest),
            AccessTokenEndpoint = new MessageReceivingEndpoint("https://twitter.com/oauth/access_token", HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.AuthorizationHeaderRequest),
            TamperProtectionElements = new ITamperProtectionChannelBindingElement[] { new HmacSha1SigningBindingElement() },
        };

        /// <summary>
        /// The URI to get a user's favorites.
        /// </summary>
        private static readonly MessageReceivingEndpoint GetFavoritesEndpoint = new MessageReceivingEndpoint("https://twitter.com/favorites.json", HttpDeliveryMethods.GetRequest);

        /// <summary>
        /// The URI to get the data on the user's home page.
        /// </summary>
        private static readonly MessageReceivingEndpoint GetFriendTimelineStatusEndpoint = new MessageReceivingEndpoint("https://twitter.com/1.1/statuses/friends_timeline.json", HttpDeliveryMethods.GetRequest);
        private static readonly MessageReceivingEndpoint GetHomeTimeLineEndpoint = new MessageReceivingEndpoint("https://api.twitter.com/1.1/statuses/home_timeline.json", HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.AuthorizationHeaderRequest);
        private static readonly MessageReceivingEndpoint GetUserTimeLineEndPoint = new MessageReceivingEndpoint("https://api.twitter.com/1.1/statuses/user_timeline.json", HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.AuthorizationHeaderRequest);
        private static readonly MessageReceivingEndpoint UpdateProfileBackgroundImageEndpoint = new MessageReceivingEndpoint("https://twitter.com/account/update_profile_background_image.json", HttpDeliveryMethods.PostRequest | HttpDeliveryMethods.AuthorizationHeaderRequest);
        private static readonly MessageReceivingEndpoint UpdateProfileImageEndpoint = new MessageReceivingEndpoint("https://twitter.com/account/update_profile_image.json", HttpDeliveryMethods.PostRequest | HttpDeliveryMethods.AuthorizationHeaderRequest);
        private static readonly MessageReceivingEndpoint VerifyCredentialsEndpoint = new MessageReceivingEndpoint("https://api.twitter.com/1.1/account/verify_credentials.json", HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.AuthorizationHeaderRequest);
        private static readonly MessageReceivingEndpoint UpdateStatusEndpoint = new MessageReceivingEndpoint("https://twitter.com/account/statuses/update.json", HttpDeliveryMethods.PostRequest | HttpDeliveryMethods.AuthorizationHeaderRequest);

        public static JArray GetUserTimeLine(WebConsumer consumer, String accessToken, int userId, String screenName, bool includeRetweets, int count)
        {
            var parameters = new Dictionary<String, string>();

            parameters.Add("count", count.ToString(CultureInfo.InvariantCulture));
            parameters.Add("include_rts", includeRetweets.ToString());

            if (!String.IsNullOrEmpty(screenName))
                parameters.Add("screen_name", screenName);

            if (userId > 0)
                parameters.Add("user_id", userId.ToString(CultureInfo.InvariantCulture));

            HttpWebRequest request = consumer.PrepareAuthorizedRequest(GetUserTimeLineEndPoint, accessToken, parameters);
            IncomingWebResponse response = consumer.Channel.WebRequestHandler.GetResponse(request);

            using (var responseReader = response.GetResponseReader())
            {
                var result = responseReader.ReadToEnd();

                return JArray.Parse(result);

            }
        }



        public static JArray GetHomeTimeLine(WebConsumer consumer, String accessToken, bool includeRetweets, int count)
        {
            var parameters = new Dictionary<String, string>();
            parameters.Add("count", count.ToString());
            parameters.Add("include_rts", includeRetweets.ToString());

            HttpWebRequest request = consumer.PrepareAuthorizedRequest(GetHomeTimeLineEndpoint, accessToken, parameters);
            IncomingWebResponse response = consumer.Channel.WebRequestHandler.GetResponse(request);

            using (var responseReader = response.GetResponseReader())
            {
                var result = responseReader.ReadToEnd();

                return JArray.Parse(result);

            }
        }
  
        public static JObject GetUserInfo(WebConsumer consumer, int userid, String screenName, string accessToken)
        {
            var baseUri = "https://api.twitter.com/1.1/users/show.json";
            String uri = String.Empty;

            if (userid > 0 && String.IsNullOrEmpty(screenName))
                uri = String.Concat(baseUri, "?user_id=", userid);

            if (userid == 0 && !String.IsNullOrEmpty(screenName))
                uri = String.Concat(baseUri, "?screen_name=", screenName);

            var endpoint = new MessageReceivingEndpoint(uri, HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.AuthorizationHeaderRequest);
            IncomingWebResponse response = consumer.PrepareAuthorizedRequestAndSend(endpoint, accessToken);

            using (var responseReader = response.GetResponseReader())
            {
                var result = responseReader.ReadToEnd();

                return JObject.Parse(result);

            }
        }



        public static JArray SearchUsers(WebConsumer consumer, String search, string accessToken)
        {
            var endpoint = new MessageReceivingEndpoint("https://api.twitter.com/1.1/users/search.json?q=" + search, HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.AuthorizationHeaderRequest);

            IncomingWebResponse response = consumer.PrepareAuthorizedRequestAndSend(endpoint, accessToken);

            using (var responseReader = response.GetResponseReader())
            {
                var result = responseReader.ReadToEnd();

                return JArray.Parse(result);

            }

        }


        /// <summary>
        ///    Updates the authenticating user's status, also known as tweeting.
        /// </summary>
        /// <param name="twitter"></param>
        /// <param name="accessToken"></param>
        /// <param name="status">The text of your status update, typically up to 140 characters. URL encode as necessary. t.co link wrapping may effect character counts.</param>
        /// <param name="includeEntities">When set to either true, t or 1, each tweet will include a node called "entities,". This node offers a variety of metadata about the tweet in a discreet structure, including: user_mentions, urls, and hashtags. While entities are opt-in on timelines at present, they will be made a default component of output in the future. See Tweet Entities for more detail on entities.
        ///  </param>
        /// <returns></returns>
        public static JObject UpdateStatus(ConsumerBase twitter, string accessToken, String status, bool includeEntities)
        {

            var parts = new[] {
				MultipartPostPart.CreateFormPart("status", status),
                MultipartPostPart.CreateFormPart("include_entities", includeEntities.ToString()),
			};

            HttpWebRequest request = twitter.PrepareAuthorizedRequest(UpdateStatusEndpoint, accessToken, parts);
            IncomingWebResponse response = twitter.Channel.WebRequestHandler.GetResponse(request);
          
            using (var responseReader = response.GetResponseReader())
            {
                var result = responseReader.ReadToEnd();

                return JObject.Parse(result);

            }
        }




        /// <summary>
        /// The consumer used for the Sign in to Twitter feature.
        /// </summary>
        private static WebConsumer signInConsumer;

        /// <summary>
        /// The lock acquired to initialize the <see cref="signInConsumer"/> field.
        /// </summary>
        private static object signInConsumerInitLock = new object();

        /// <summary>
        /// Initializes static members of the <see cref="TwitterConsumer"/> class.
        /// </summary>
        static TwitterConsumer()
        {
            // Twitter can't handle the Expect 100 Continue HTTP header. 
            ServicePointManager.FindServicePoint(GetFavoritesEndpoint.Location).Expect100Continue = false;
        }

        /// <summary>
        /// Gets a value indicating whether the Twitter consumer key and secret are set in the web.config file.
        /// </summary>
        public static bool IsTwitterConsumerConfigured
        {
            get
            {
                return !string.IsNullOrEmpty(ConfigurationManager.AppSettings["twitterConsumerKey"]) &&
                    !string.IsNullOrEmpty(ConfigurationManager.AppSettings["twitterConsumerSecret"]);
            }
        }

        /// <summary>
        /// Gets the consumer to use for the Sign in to Twitter feature.
        /// </summary>
        /// <value>The twitter sign in.</value>
        private static WebConsumer TwitterSignIn
        {
            get
            {
                if (signInConsumer == null)
                {
                    lock (signInConsumerInitLock)
                    {
                        if (signInConsumer == null)
                        {
                            signInConsumer = new WebConsumer(SignInWithTwitterServiceDescription, ShortTermUserSessionTokenManager);
                        }
                    }
                }

                return signInConsumer;
            }
        }

        private static InMemoryTokenManager ShortTermUserSessionTokenManager
        {
            get
            {
                var store = HttpContext.Current.Session;
                var tokenManager = (InMemoryTokenManager)store["TwitterShortTermUserSessionTokenManager"];
                if (tokenManager == null)
                {
                    string consumerKey = ConfigurationManager.AppSettings["twitterConsumerKey"];
                    string consumerSecret = ConfigurationManager.AppSettings["twitterConsumerSecret"];
                    if (IsTwitterConsumerConfigured)
                    {
                        tokenManager = new InMemoryTokenManager(consumerKey, consumerSecret);
                        store["TwitterShortTermUserSessionTokenManager"] = tokenManager;
                    }
                    else
                    {
                        throw new InvalidOperationException("No Twitter OAuth consumer key and secret could be found in web.config AppSettings.");
                    }
                }

                return tokenManager;
            }
        }

        public static XDocument GetUpdates(ConsumerBase twitter, string accessToken)
        {
            IncomingWebResponse response = twitter.PrepareAuthorizedRequestAndSend(GetFriendTimelineStatusEndpoint, accessToken);
            return XDocument.Load(XmlReader.Create(response.GetResponseReader()));
        }

        public static XDocument GetFavorites(ConsumerBase twitter, string accessToken)
        {
            IncomingWebResponse response = twitter.PrepareAuthorizedRequestAndSend(GetFavoritesEndpoint, accessToken);
            return XDocument.Load(XmlReader.Create(response.GetResponseReader()));
        }

        public static XDocument UpdateProfileBackgroundImage(ConsumerBase twitter, string accessToken, string image, bool tile)
        {
            var parts = new[] {
				MultipartPostPart.CreateFormFilePart("image", image, "image/" + Path.GetExtension(image).Substring(1).ToLowerInvariant()),
				MultipartPostPart.CreateFormPart("tile", tile.ToString().ToLowerInvariant()),
			};
            HttpWebRequest request = twitter.PrepareAuthorizedRequest(UpdateProfileBackgroundImageEndpoint, accessToken, parts);
            request.ServicePoint.Expect100Continue = false;
            IncomingWebResponse response = twitter.Channel.WebRequestHandler.GetResponse(request);
            string responseString = response.GetResponseReader().ReadToEnd();
            return XDocument.Parse(responseString);
        }

        public static XDocument UpdateProfileImage(ConsumerBase twitter, string accessToken, string pathToImage)
        {
            string contentType = "image/" + Path.GetExtension(pathToImage).Substring(1).ToLowerInvariant();
            return UpdateProfileImage(twitter, accessToken, File.OpenRead(pathToImage), contentType);
        }

        public static XDocument UpdateProfileImage(ConsumerBase twitter, string accessToken, Stream image, string contentType)
        {
            var parts = new[] {
				MultipartPostPart.CreateFormFilePart("image", "twitterPhoto", contentType, image),
			};
            HttpWebRequest request = twitter.PrepareAuthorizedRequest(UpdateProfileImageEndpoint, accessToken, parts);
            IncomingWebResponse response = twitter.Channel.WebRequestHandler.GetResponse(request);
            string responseString = response.GetResponseReader().ReadToEnd();
            return XDocument.Parse(responseString);
        }

        public static XDocument VerifyCredentials(ConsumerBase twitter, string accessToken)
        {
            IncomingWebResponse response = twitter.PrepareAuthorizedRequestAndSend(VerifyCredentialsEndpoint, accessToken);
            return XDocument.Load(XmlReader.Create(response.GetResponseReader()));
        }

        public static string GetUsername(ConsumerBase twitter, string accessToken)
        {
            XDocument xml = VerifyCredentials(twitter, accessToken);
            XPathNavigator nav = xml.CreateNavigator();
            return nav.SelectSingleNode("/user/screen_name").Value;
        }

        /// <summary>
        /// Prepares a redirect that will send the user to Twitter to sign in.
        /// </summary>
        /// <param name="forceNewLogin">if set to <c>true</c> the user will be required to re-enter their Twitter credentials even if already logged in to Twitter.</param>
        /// <returns>The redirect message.</returns>
        /// <remarks>
        /// Call <see cref="OutgoingWebResponse.Send"/> or
        /// <c>return StartSignInWithTwitter().<see cref="MessagingUtilities.AsActionResult">AsActionResult()</see></c>
        /// to actually perform the redirect.
        /// </remarks>
        public static OutgoingWebResponse StartSignInWithTwitter(bool forceNewLogin)
        {
            var redirectParameters = new Dictionary<string, string>();
            if (forceNewLogin)
            {
                redirectParameters["force_login"] = "true";
            }
            Uri callback = MessagingUtilities.GetRequestUrlFromContext().StripQueryArgumentsWithPrefix("oauth_");
            var request = TwitterSignIn.PrepareRequestUserAuthorization(callback, null, redirectParameters);
            return TwitterSignIn.Channel.PrepareResponse(request);
        }

        /// <summary>
        /// Checks the incoming web request to see if it carries a Twitter authentication response,
        /// and provides the user's Twitter screen name and unique id if available.
        /// </summary>
        /// <param name="screenName">The user's Twitter screen name.</param>
        /// <param name="userId">The user's Twitter unique user ID.</param>
        /// <returns>
        /// A value indicating whether Twitter authentication was successful;
        /// otherwise <c>false</c> to indicate that no Twitter response was present.
        /// </returns>
        public static bool TryFinishSignInWithTwitter(out string screenName, out int userId)
        {
            screenName = null;
            userId = 0;
            var response = TwitterSignIn.ProcessUserAuthorization();
            if (response == null)
            {
                return false;
            }

            screenName = response.ExtraData["screen_name"];
            userId = int.Parse(response.ExtraData["user_id"]);

            // If we were going to make this LOOK like OpenID even though it isn't,
            // this seems like a reasonable, secure claimed id to allow the user to assume.
            OpenId.Identifier fake_claimed_id = string.Format(CultureInfo.InvariantCulture, "https://twitter.com/{0}#{1}", screenName, userId);

            return true;
        }
    }
}
