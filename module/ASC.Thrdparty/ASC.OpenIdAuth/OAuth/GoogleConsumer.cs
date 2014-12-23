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
using OpenIdAuth.Utils;

namespace OpenIdAuth.OAuth {
    /// <summary>
	/// A consumer capable of communicating with Google Data APIs.
	/// </summary>
	public static class GoogleConsumer {
		/// <summary>
		/// The Consumer to use for accessing Google data APIs.
		/// </summary>
		public static readonly ServiceProviderDescription ServiceDescription = new ServiceProviderDescription {
			RequestTokenEndpoint = new MessageReceivingEndpoint("https://www.google.com/accounts/OAuthGetRequestToken", HttpDeliveryMethods.AuthorizationHeaderRequest | HttpDeliveryMethods.GetRequest),
			UserAuthorizationEndpoint = new MessageReceivingEndpoint("https://www.google.com/accounts/OAuthAuthorizeToken", HttpDeliveryMethods.AuthorizationHeaderRequest | HttpDeliveryMethods.GetRequest),
			AccessTokenEndpoint = new MessageReceivingEndpoint("https://www.google.com/accounts/OAuthGetAccessToken", HttpDeliveryMethods.AuthorizationHeaderRequest | HttpDeliveryMethods.GetRequest),
			TamperProtectionElements = new ITamperProtectionChannelBindingElement[] { new HmacSha1SigningBindingElement() },
		};

		/// <summary>
		/// A mapping between Google's applications and their URI scope values.
		/// </summary>
		private static readonly Dictionary<Applications, string> DataScopeUris = new Dictionary<Applications, string> {
			{ Applications.Analytics, "https://www.google.com/analytics/feeds/" },
			{ Applications.GoogleBase, "http://www.google.com/base/feeds/" },
			{ Applications.Blogger, "http://www.blogger.com/feeds" },
			{ Applications.BookSearch, "http://www.google.com/books/feeds/" },
			{ Applications.Calendar, "http://www.google.com/calendar/feeds/" },
			{ Applications.Contacts, "http://www.google.com/m8/feeds/" },
			{ Applications.DocumentsList, "http://docs.google.com/feeds/" },
			{ Applications.Finance, "http://finance.google.com/finance/feeds/" },
			{ Applications.Gmail, "https://mail.google.com/mail/feed/atom" },
			{ Applications.Health, "https://www.google.com/h9/feeds/" },
			{ Applications.Maps, "http://maps.google.com/maps/feeds/" },
			{ Applications.OpenSocial, "http://sandbox.gmodules.com/api/" },
			{ Applications.PicasaWeb, "http://picasaweb.google.com/data/" },
			{ Applications.Spreadsheets, "http://spreadsheets.google.com/feeds/" },
			{ Applications.WebmasterTools, "http://www.google.com/webmasters/tools/feeds/" },
			{ Applications.YouTube, "http://gdata.youtube.com" },
		};

		/// <summary>
		/// The URI to get contacts once authorization is granted.
		/// </summary>
		private static readonly MessageReceivingEndpoint GetContactsEndpoint = new MessageReceivingEndpoint("http://www.google.com/m8/feeds/contacts/default/full/", HttpDeliveryMethods.GetRequest);

		/// <summary>
		/// The many specific authorization scopes Google offers.
		/// </summary>
		[Flags]
		public enum Applications : long {
			/// <summary>
			/// The Gmail address book.
			/// </summary>
			Contacts = 0x1,

			/// <summary>
			/// Appointments in Google Calendar.
			/// </summary>
			Calendar = 0x2,

			/// <summary>
			/// Blog post authoring.
			/// </summary>
			Blogger = 0x4,

			/// <summary>
			/// Google Finance
			/// </summary>
			Finance = 0x8,

			/// <summary>
			/// Google Gmail
			/// </summary>
			Gmail = 0x10,

			/// <summary>
			/// Google Health
			/// </summary>
			Health = 0x20,

			/// <summary>
			/// Google OpenSocial
			/// </summary>
			OpenSocial = 0x40,

			/// <summary>
			/// Picasa Web
			/// </summary>
			PicasaWeb = 0x80,

			/// <summary>
			/// Google Spreadsheets
			/// </summary>
			Spreadsheets = 0x100,

			/// <summary>
			/// Webmaster Tools
			/// </summary>
			WebmasterTools = 0x200,

			/// <summary>
			/// YouTube service
			/// </summary>
			YouTube = 0x400,

			/// <summary>
			/// Google Docs
			/// </summary>
			DocumentsList = 0x800,

			/// <summary>
			/// Google Book Search
			/// </summary>
			BookSearch = 0x1000,

			/// <summary>
			/// Google Base
			/// </summary>
			GoogleBase = 0x2000,

			/// <summary>
			/// Google Analytics
			/// </summary>
			Analytics = 0x4000,

			/// <summary>
			/// Google Maps
			/// </summary>
			Maps = 0x8000,
		}

		/// <summary>
		/// The service description to use for accessing Google data APIs using an X509 certificate.
		/// </summary>
		/// <param name="signingCertificate">The signing certificate.</param>
		/// <returns>A service description that can be used to create an instance of
		/// <see cref="DesktopConsumer"/> or <see cref="WebConsumer"/>. </returns>
		public static ServiceProviderDescription CreateRsaSha1ServiceDescription(X509Certificate2 signingCertificate) {
			if (signingCertificate == null) {
				throw new ArgumentNullException("signingCertificate");
			}

			return new ServiceProviderDescription {
				RequestTokenEndpoint = new MessageReceivingEndpoint("https://www.google.com/accounts/OAuthGetRequestToken", HttpDeliveryMethods.AuthorizationHeaderRequest | HttpDeliveryMethods.GetRequest),
				UserAuthorizationEndpoint = new MessageReceivingEndpoint("https://www.google.com/accounts/OAuthAuthorizeToken", HttpDeliveryMethods.AuthorizationHeaderRequest | HttpDeliveryMethods.GetRequest),
				AccessTokenEndpoint = new MessageReceivingEndpoint("https://www.google.com/accounts/OAuthGetAccessToken", HttpDeliveryMethods.AuthorizationHeaderRequest | HttpDeliveryMethods.GetRequest),
				TamperProtectionElements = new ITamperProtectionChannelBindingElement[] { new RsaSha1SigningBindingElement(signingCertificate) },
			};
		}

		/// <summary>
		/// Requests authorization from Google to access data from a set of Google applications.
		/// </summary>
		/// <param name="consumer">The Google consumer previously constructed using <see cref="CreateWebConsumer"/> or <see cref="CreateDesktopConsumer"/>.</param>
		/// <param name="requestedAccessScope">The requested access scope.</param>
		public static void RequestAuthorization(WebConsumer consumer, Applications requestedAccessScope) {
			if (consumer == null) {
				throw new ArgumentNullException("consumer");
			}

			var extraParameters = new Dictionary<string, string> {
				{ "scope", GetScopeUri(requestedAccessScope) },
			};
			Uri callback = Util.GetCallbackUrlFromContext();
			var request = consumer.PrepareRequestUserAuthorization(callback, extraParameters, null);
			consumer.Channel.Send(request);
		}

		/// <summary>
		/// Requests authorization from Google to access data from a set of Google applications.
		/// </summary>
		/// <param name="consumer">The Google consumer previously constructed using <see cref="CreateWebConsumer"/> or <see cref="CreateDesktopConsumer"/>.</param>
		/// <param name="requestedAccessScope">The requested access scope.</param>
		/// <param name="requestToken">The unauthorized request token assigned by Google.</param>
		/// <returns>The request token</returns>
		public static Uri RequestAuthorization(DesktopConsumer consumer, Applications requestedAccessScope, out string requestToken) {
			if (consumer == null) {
				throw new ArgumentNullException("consumer");
			}

			var extraParameters = new Dictionary<string, string> {
				{ "scope", GetScopeUri(requestedAccessScope) },
			};

			return consumer.RequestUserAuthorization(extraParameters, null, out requestToken);
		}

		
		/// <summary>
		/// Gets the scope URI in Google's format.
		/// </summary>
		/// <param name="scope">The scope, which may include one or several Google applications.</param>
		/// <returns>A space-delimited list of URIs for the requested Google applications.</returns>
		public static string GetScopeUri(Applications scope) {
			return string.Join(" ", Util.GetIndividualFlags(scope).Select(app => DataScopeUris[(Applications)app]).ToArray());
		}
	}
}
