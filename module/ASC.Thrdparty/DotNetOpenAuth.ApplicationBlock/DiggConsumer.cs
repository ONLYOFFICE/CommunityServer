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


//-----------------------------------------------------------------------
// <copyright file="GoogleConsumer.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.ApplicationBlock {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Security.Cryptography.X509Certificates;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Xml;
	using System.Xml.Linq;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth;
	using DotNetOpenAuth.OAuth.ChannelElements;

	/// <summary>
	/// A consumer capable of communicating with Google Data APIs.
	/// </summary>
	public static class DiggConsumer {
		/// <summary>
		/// The Consumer to use for accessing Google data APIs.
		/// </summary>
		public static readonly ServiceProviderDescription ServiceDescription = new ServiceProviderDescription {
            RequestTokenEndpoint = new MessageReceivingEndpoint("http://services.digg.com/oauth/request_token", HttpDeliveryMethods.AuthorizationHeaderRequest | HttpDeliveryMethods.GetRequest),
            UserAuthorizationEndpoint = new MessageReceivingEndpoint("http://digg.com/oauth/authorize", HttpDeliveryMethods.AuthorizationHeaderRequest | HttpDeliveryMethods.GetRequest),
            AccessTokenEndpoint = new MessageReceivingEndpoint("http://services.digg.com/oauth/access_token", HttpDeliveryMethods.AuthorizationHeaderRequest | HttpDeliveryMethods.GetRequest),
			TamperProtectionElements = new ITamperProtectionChannelBindingElement[] { new HmacSha1SigningBindingElement() },
		};

		
        //
		/// <summary>
		/// The URI to get contacts once authorization is granted.
		/// </summary>
        private static readonly MessageReceivingEndpoint DiggEndpoint = new MessageReceivingEndpoint("http://services.digg.com/2.0/story.digg", HttpDeliveryMethods.AuthorizationHeaderRequest | HttpDeliveryMethods.GetRequest);
        private static readonly MessageReceivingEndpoint GetDiggsEndpoint = new MessageReceivingEndpoint("http://services.digg.com/2.0/story.getDiggs", HttpDeliveryMethods.AuthorizationHeaderRequest | HttpDeliveryMethods.GetRequest);
        private static readonly MessageReceivingEndpoint FollowUserEndpoint = new MessageReceivingEndpoint("http://services.digg.com/2.0/user.follow", HttpDeliveryMethods.AuthorizationHeaderRequest | HttpDeliveryMethods.GetRequest);

		/// <summary>
		/// Requests authorization from Google to access data from a set of Google applications.
		/// </summary>
		/// <param name="consumer">The Google consumer previously constructed using <see cref="CreateWebConsumer"/> or <see cref="CreateDesktopConsumer"/>.</param>
		/// <param name="requestedAccessScope">The requested access scope.</param>
		public static void RequestAuthorization(WebConsumer consumer) {
			if (consumer == null) {
				throw new ArgumentNullException("consumer");
			}

			
			Uri callback = Util.GetCallbackUrlFromContext();
			var request = consumer.PrepareRequestUserAuthorization(callback, null, null);
			consumer.Channel.Send(request);
		}

        public static string Digg(ConsumerBase consumer, string accessToken, string id)
        {
            if (consumer == null)
            {
                throw new ArgumentNullException("consumer");
            }

            var extraData = new Dictionary<string, string>() {
				{ "story_id", id }
			};
            var request = consumer.PrepareAuthorizedRequest(DiggEndpoint, accessToken, extraData);
            var response = consumer.Channel.WebRequestHandler.GetResponse(request);
            string body = response.GetResponseReader().ReadToEnd();
            return body;
        }

        public static string GetDigs(ConsumerBase consumer, string accessToken, string id)
        {
            if (consumer == null)
            {
                throw new ArgumentNullException("consumer");
            }

            var extraData = new Dictionary<string, string>() {
				{ "story_id", id }
			};
            var request = consumer.PrepareAuthorizedRequest(GetDiggsEndpoint, accessToken, extraData);
            var response = consumer.Channel.WebRequestHandler.GetResponse(request);
            string body = response.GetResponseReader().ReadToEnd();
            return body;
        }

        public static string Follow(ConsumerBase consumer, string accessToken, string username)
        {
            if (consumer == null)
            {
                throw new ArgumentNullException("consumer");
            }

            var extraData = new Dictionary<string, string>() {
				{ "username", username }
			};
            var request = consumer.PrepareAuthorizedRequest(FollowUserEndpoint, accessToken, extraData);
            var response = consumer.Channel.WebRequestHandler.GetResponse(request);
            string body = response.GetResponseReader().ReadToEnd();
            return body;
        }

	}
}
