/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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
