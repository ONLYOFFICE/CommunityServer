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
// <copyright file="WindowsLiveClient.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.ApplicationBlock {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.OAuth2;

	public class WindowsLiveClient : WebServerClient {
		private static readonly AuthorizationServerDescription WindowsLiveDescription = new AuthorizationServerDescription {
			TokenEndpoint = new Uri("https://oauth.live.com/token"),
			AuthorizationEndpoint = new Uri("https://oauth.live.com/authorize"),
		};

		/// <summary>
		/// Initializes a new instance of the <see cref="WindowsLiveClient"/> class.
		/// </summary>
		public WindowsLiveClient()
			: base(WindowsLiveDescription, "", "") {
		}

		/// <summary>
		/// Well-known scopes defined by the Windows Live service.
		/// </summary>
		/// <remarks>
		/// This sample includes just a few scopes.  For a complete list of scopes please refer to:
		/// http://msdn.microsoft.com/en-us/library/hh243646.aspx
		/// </remarks>
		public static class Scopes {
			/// <summary>
			/// The ability of an app to read and update a user's info at any time. Without this scope, an app can access the user's info only while the user is signed in to Live Connect and is using your app.
			/// </summary>
			public const string OfflineAccess = "wl.offline_access";

			/// <summary>
			/// Single sign-in behavior. With single sign-in, users who are already signed in to Live Connect are also signed in to your website.
			/// </summary>
			public const string SignIn = "wl.signin";

			/// <summary>
			/// Read access to a user's basic profile info. Also enables read access to a user's list of contacts.
			/// </summary>
			public const string Basic = "wl.basic";
		}
	}
}
