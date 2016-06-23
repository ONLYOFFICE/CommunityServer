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
