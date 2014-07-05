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
