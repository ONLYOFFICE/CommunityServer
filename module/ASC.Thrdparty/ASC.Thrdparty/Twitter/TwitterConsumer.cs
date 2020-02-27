/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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