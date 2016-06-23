/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
	public static class GoogleConsumer {
        /// <summary>
        /// The Consumer to use for accessing Google data APIs.
        /// </summary>
        public static readonly ServiceProviderDescription ServiceDescription = new ServiceProviderDescription
        {
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
			{ Applications.DocumentsList, "https://docs.google.com/feeds/" },
			{ Applications.UserContent, "http://docs.googleusercontent.com/" },
			{ Applications.Gmail, "https://mail.google.com/mail/feed/atom" },
			{ Applications.Health, "https://www.google.com/h9/feeds/" },
			{ Applications.Maps, "http://maps.google.com/maps/feeds/" },
			{ Applications.OpenSocial, "http://sandbox.gmodules.com/api/" },
			{ Applications.PicasaWeb, "http://picasaweb.google.com/data/" },
			{ Applications.Spreadsheets, "https://spreadsheets.google.com/feeds/" },
			{ Applications.WebmasterTools, "http://www.google.com/webmasters/tools/feeds/" },
			{ Applications.YouTube, "http://gdata.youtube.com" },
		};
        //
		/// <summary>
		/// The URI to get contacts once authorization is granted.
		/// </summary>
        private static readonly MessageReceivingEndpoint GetContactsEndpoint = new MessageReceivingEndpoint("https://www.google.com/m8/feeds/contacts/default/full/", HttpDeliveryMethods.AuthorizationHeaderRequest | HttpDeliveryMethods.GetRequest);

        private static readonly MessageReceivingEndpoint GetDocsEntriesEndpoint = new MessageReceivingEndpoint("https://docs.google.com/feeds/default/private/full?showfolders=true&v=3", HttpDeliveryMethods.AuthorizationHeaderRequest | HttpDeliveryMethods.GetRequest);

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
            UserContent = 0x8,

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
                TamperProtectionElements = new ITamperProtectionChannelBindingElement[] { new HmacSha1SigningBindingElement() },
			};
		}

        /// <summary>
        /// Requests authorization from Google to access data from a set of Google applications.
        /// </summary>
        /// <param name="consumer">The Google consumer previously constructed using <see cref="CreateWebConsumer"/> or <see cref="CreateDesktopConsumer"/>.</param>
        /// <param name="requestedAccessScope">The requested access scope.</param>
        public static void RequestAuthorization(WebConsumer consumer, Applications requestedAccessScope)
        {
            if (consumer == null)
            {
                throw new ArgumentNullException("consumer");
            }

            var extraParameters = new Dictionary<string, string> {
				{ "scope", GetScopeUri(requestedAccessScope) },
			};
            Uri callback = Util.GetCallbackUrlFromContext();
            var request = consumer.PrepareRequestUserAuthorization(callback, extraParameters, null);
            consumer.Channel.Send(request);
        }



        public static void RequestAuthorization(WebConsumer consumer, string scope)
        {
            if (consumer == null)
            {
                throw new ArgumentNullException("consumer");
            }

            var extraParameters = new Dictionary<string, string> {
				{ "scope",scope },
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
        public static Uri RequestAuthorization(DesktopConsumer consumer, Applications requestedAccessScope, out string requestToken)
        {
            if (consumer == null)
            {
                throw new ArgumentNullException("consumer");
            }

            var extraParameters = new Dictionary<string, string> {
				{ "scope", GetScopeUri(requestedAccessScope) },
			};

            return consumer.RequestUserAuthorization(extraParameters, null, out requestToken);
        }

		/// <summary>
		/// Gets the Gmail address book's contents.
		/// </summary>
		/// <param name="consumer">The Google consumer.</param>
		/// <param name="accessToken">The access token previously retrieved.</param>
		/// <param name="maxResults">The maximum number of entries to return. If you want to receive all of the contacts, rather than only the default maximum, you can specify a very large number here.</param>
		/// <param name="startIndex">The 1-based index of the first result to be retrieved (for paging).</param>
		/// <returns>An XML document returned by Google.</returns>
		public static XDocument GetContacts(ConsumerBase consumer, string accessToken, int maxResults/* = 25*/, int startIndex/* = 1*/) {
			if (consumer == null) {
				throw new ArgumentNullException("consumer");
			}

			var extraData = new Dictionary<string, string>() {
				{ "start-index", startIndex.ToString(CultureInfo.InvariantCulture) },
				{ "max-results", maxResults.ToString(CultureInfo.InvariantCulture) },
			};
			var request = consumer.PrepareAuthorizedRequest(GetContactsEndpoint, accessToken,extraData);
			var response = consumer.Channel.WebRequestHandler.GetResponse(request);
			string body = response.GetResponseReader().ReadToEnd();
			XDocument result = XDocument.Parse(body);
			return result;
		}

	    public static Stream GetDoc(ConsumerBase consumer, string accessToken, string docEndpoint)
	    {
	        long size;
	        return GetDoc(consumer, accessToken, docEndpoint, out size);
	    }

	    public static Stream GetDoc(ConsumerBase consumer, string accessToken, string docEndpoint, out long size)
        {
            if (consumer == null)
            {
                throw new ArgumentNullException("consumer");
            }
            var endpoint = new MessageReceivingEndpoint(docEndpoint, HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.AuthorizationHeaderRequest);
            var extraData = new Dictionary<string, string>() /*{ {"GData-Version","3.0"} }*/;
            var request = consumer.PrepareAuthorizedRequest(endpoint, accessToken, extraData);
            var response = consumer.Channel.WebRequestHandler.GetResponse(request);
            if (!string.IsNullOrEmpty(response.Headers[HttpResponseHeader.ContentLength]))
            {
                long.TryParse(response.Headers[HttpResponseHeader.ContentLength], out size);
            }
            else
            {
                size = 0;
            }
            return response.ResponseStream;
        }

	    public static XDocument GetDocList(ConsumerBase consumer, string accessToken, string nextEndpoint)
        {
            if (consumer == null)
            {
                throw new ArgumentNullException("consumer");
            }
            var endpoint = GetDocsEntriesEndpoint;
            if (!string.IsNullOrEmpty(nextEndpoint))
            {
                endpoint = new MessageReceivingEndpoint(nextEndpoint, HttpDeliveryMethods.GetRequest);
            }

            var extraData = new Dictionary<string, string>() /*{ {"GData-Version","3.0"} }*/;
            //var request = consumer.PrepareAuthorizedRequest(endpoint, accessToken, extraData);
            //var response = consumer.Channel.WebRequestHandler.GetResponse(request);
            //string body = response.GetResponseReader().ReadToEnd();
            //XDocument result = XDocument.Parse(body);
            //return result;

            var request = consumer.PrepareAuthorizedRequest(endpoint, accessToken, extraData);

            // Enable gzip compression.  Google only compresses the response for recognized user agent headers. - Mike Lim
            request.AutomaticDecompression = DecompressionMethods.GZip;
            request.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US) AppleWebKit/534.16 (KHTML, like Gecko) Chrome/10.0.648.151 Safari/534.16";

            var response = consumer.Channel.WebRequestHandler.GetResponse(request);
            string body = response.GetResponseReader().ReadToEnd();
            XDocument result = XDocument.Parse(body);
            return result;

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
