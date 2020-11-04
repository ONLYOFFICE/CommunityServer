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


//-----------------------------------------------------------------------
// <copyright file="TwitterConsumer.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OAuth;
using DotNetOpenAuth.OAuth.ChannelElements;
using Newtonsoft.Json.Linq;

namespace ASC.Thrdparty.Twitter
{
    /// <summary>
    /// A consumer capable of communicating with Twitter.
    /// </summary>
    public static class TwitterConsumer
    {
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
        private static readonly MessageReceivingEndpoint GetHomeTimeLineEndpoint = new MessageReceivingEndpoint("https://api.twitter.com/1.1/statuses/home_timeline.json", HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.AuthorizationHeaderRequest);
        private static readonly MessageReceivingEndpoint GetUserTimeLineEndPoint = new MessageReceivingEndpoint("https://api.twitter.com/1.1/statuses/user_timeline.json", HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.AuthorizationHeaderRequest);

        public static JArray GetUserTimeLine(WebConsumer consumer, String accessToken, int userId, String screenName, bool includeRetweets, int count)
        {
            var parameters = new Dictionary<String, string>
                {
                    { "count", count.ToString(CultureInfo.InvariantCulture) },
                    { "include_rts", includeRetweets.ToString() }
                };

            if (!String.IsNullOrEmpty(screenName))
                parameters.Add("screen_name", screenName);

            if (userId > 0)
                parameters.Add("user_id", userId.ToString(CultureInfo.InvariantCulture));

            var request = consumer.PrepareAuthorizedRequest(GetUserTimeLineEndPoint, accessToken, parameters);
            var response = consumer.Channel.WebRequestHandler.GetResponse(request);

            using (var responseReader = response.GetResponseReader())
            {
                var result = responseReader.ReadToEnd();

                return JArray.Parse(result);
            }
        }

        public static JArray GetHomeTimeLine(WebConsumer consumer, String accessToken, bool includeRetweets, int count)
        {
            var parameters = new Dictionary<String, string>
                {
                    { "count", count.ToString(CultureInfo.InvariantCulture) },
                    { "include_rts", includeRetweets.ToString() }
                };

            var request = consumer.PrepareAuthorizedRequest(GetHomeTimeLineEndpoint, accessToken, parameters);
            var response = consumer.Channel.WebRequestHandler.GetResponse(request);

            using (var responseReader = response.GetResponseReader())
            {
                var result = responseReader.ReadToEnd();

                return JArray.Parse(result);
            }
        }

        public static JObject GetUserInfo(WebConsumer consumer, int userid, String screenName, string accessToken)
        {
            const string baseUri = "https://api.twitter.com/1.1/users/show.json";
            var uri = String.Empty;

            if (userid > 0 && String.IsNullOrEmpty(screenName))
                uri = String.Concat(baseUri, "?user_id=", userid);

            if (userid == 0 && !String.IsNullOrEmpty(screenName))
                uri = String.Concat(baseUri, "?screen_name=", screenName);

            var endpoint = new MessageReceivingEndpoint(uri, HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.AuthorizationHeaderRequest);
            var response = consumer.PrepareAuthorizedRequestAndSend(endpoint, accessToken);

            using (var responseReader = response.GetResponseReader())
            {
                var result = responseReader.ReadToEnd();

                return JObject.Parse(result);
            }
        }

        public static JArray SearchUsers(WebConsumer consumer, String search, string accessToken)
        {
            var endpoint = new MessageReceivingEndpoint("https://api.twitter.com/1.1/users/search.json?q=" + Uri.EscapeDataString(search), HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.AuthorizationHeaderRequest);

            var response = consumer.PrepareAuthorizedRequestAndSend(endpoint, accessToken);

            using (var responseReader = response.GetResponseReader())
            {
                var result = responseReader.ReadToEnd();

                return JArray.Parse(result);
            }
        }

        /// <summary>
        /// Initializes static members of the <see cref="TwitterConsumer"/> class.
        /// </summary>
        static TwitterConsumer()
        {
            // Twitter can't handle the Expect 100 Continue HTTP header. 
            ServicePointManager.FindServicePoint(GetFavoritesEndpoint.Location).Expect100Continue = false;
        }
    }
}