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
// <copyright file="YammerConsumer.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.ApplicationBlock {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth;
	using DotNetOpenAuth.OAuth.ChannelElements;
	using DotNetOpenAuth.OAuth.Messages;

	public static class YammerConsumer {
		/// <summary>
		/// The Consumer to use for accessing Google data APIs.
		/// </summary>
		public static readonly ServiceProviderDescription ServiceDescription = new ServiceProviderDescription {
			RequestTokenEndpoint = new MessageReceivingEndpoint("https://www.yammer.com/oauth/request_token", HttpDeliveryMethods.AuthorizationHeaderRequest | HttpDeliveryMethods.PostRequest),
			UserAuthorizationEndpoint = new MessageReceivingEndpoint("https://www.yammer.com/oauth/authorize", HttpDeliveryMethods.AuthorizationHeaderRequest | HttpDeliveryMethods.GetRequest),
			AccessTokenEndpoint = new MessageReceivingEndpoint("https://www.yammer.com/oauth/access_token", HttpDeliveryMethods.AuthorizationHeaderRequest | HttpDeliveryMethods.PostRequest),
			TamperProtectionElements = new ITamperProtectionChannelBindingElement[] { new PlaintextSigningBindingElement() },
			ProtocolVersion = ProtocolVersion.V10,
		};

		public static DesktopConsumer CreateConsumer(IConsumerTokenManager tokenManager) {
			return new DesktopConsumer(ServiceDescription, tokenManager);
		}

		public static Uri PrepareRequestAuthorization(DesktopConsumer consumer, out string requestToken) {
			if (consumer == null) {
				throw new ArgumentNullException("consumer");
			}

			Uri authorizationUrl = consumer.RequestUserAuthorization(null, null, out requestToken);
			return authorizationUrl;
		}

		public static AuthorizedTokenResponse CompleteAuthorization(DesktopConsumer consumer, string requestToken, string userCode) {
			// Because Yammer has a proprietary callback_token parameter, and it's passed
			// with the message that specifically bans extra arguments being passed, we have
			// to cheat by adding the data to the URL itself here.
			var customServiceDescription = new ServiceProviderDescription {
				RequestTokenEndpoint = ServiceDescription.RequestTokenEndpoint,
				UserAuthorizationEndpoint = ServiceDescription.UserAuthorizationEndpoint,
				AccessTokenEndpoint = new MessageReceivingEndpoint(ServiceDescription.AccessTokenEndpoint.Location.AbsoluteUri + "?oauth_verifier=" + Uri.EscapeDataString(userCode), HttpDeliveryMethods.AuthorizationHeaderRequest | HttpDeliveryMethods.PostRequest),
				TamperProtectionElements = ServiceDescription.TamperProtectionElements,
				ProtocolVersion = ProtocolVersion.V10,
			};

			// To use a custom service description we also must create a new WebConsumer.
			var customConsumer = new DesktopConsumer(customServiceDescription, consumer.TokenManager);
			var response = customConsumer.ProcessUserAuthorization(requestToken, userCode);
			return response;
		}
	}
}
