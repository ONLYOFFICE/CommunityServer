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
// <copyright file="YubikeyRelyingParty.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.ApplicationBlock {
	using System;
	using System.Collections.Specialized;
	using System.Globalization;
	using System.IO;
	using System.Net;
	using System.Text;
	using System.Text.RegularExpressions;

	/// <summary>
	/// The set of possible results from verifying a Yubikey token.
	/// </summary>
	public enum YubikeyResult {
		/// <summary>
		/// The OTP is valid.
		/// </summary>
		Ok,

		/// <summary>
		/// The OTP is invalid format.
		/// </summary>
		BadOtp,

		/// <summary>
		/// The OTP has already been seen by the service.
		/// </summary>
		ReplayedOtp,

		/// <summary>
		/// The HMAC signature verification failed.
		/// </summary>
		/// <remarks>
		/// This indicates a bug in the relying party code.
		/// </remarks>
		BadSignature,

		/// <summary>
		/// The request lacks a parameter.
		/// </summary>
		/// <remarks>
		/// This indicates a bug in the relying party code.
		/// </remarks>
		MissingParameter,

		/// <summary>
		/// The request id does not exist.
		/// </summary>
		NoSuchClient,

		/// <summary>
		/// The request id is not allowed to verify OTPs.
		/// </summary>
		OperationNotAllowed,

		/// <summary>
		/// Unexpected error in our server. Please contact Yubico if you see this error.
		/// </summary>
		BackendError,
	}

	/// <summary>
	/// Provides verification of a Yubikey one-time password (OTP) as a means of authenticating
	/// a user at your web site or application.
	/// </summary>
	/// <remarks>
	/// Please visit http://yubico.com/ for more information about this authentication method.
	/// </remarks>
	public class YubikeyRelyingParty {
		/// <summary>
		/// The default Yubico authorization server to use for validation and replay protection.
		/// </summary>
		private const string DefaultYubicoAuthorizationServer = "https://api.yubico.com/wsapi/verify";

		/// <summary>
		/// The format of the lines in the Yubico server response.
		/// </summary>
		private static readonly Regex ResultLineMatcher = new Regex(@"^(?<key>[^=]+)=(?<value>.*)$");

		/// <summary>
		/// The Yubico authorization server to use for validation and replay protection.
		/// </summary>
		private readonly string yubicoAuthorizationServer;

		/// <summary>
		/// The authorization ID assigned to your individual site by Yubico.
		/// </summary>
		private readonly int yubicoAuthorizationId;

		/// <summary>
		/// Initializes a new instance of the <see cref="YubikeyRelyingParty"/> class
		/// that uses the default Yubico server for validation and replay protection.
		/// </summary>
		/// <param name="authorizationId">The authorization ID assigned to your individual site by Yubico.
		/// Get one from https://upgrade.yubico.com/getapikey/</param>
		public YubikeyRelyingParty(int authorizationId)
			: this(authorizationId, DefaultYubicoAuthorizationServer) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="YubikeyRelyingParty"/> class.
		/// </summary>
		/// <param name="authorizationId">The authorization ID assigned to your individual site by Yubico.
		/// Contact tech@yubico.com if you haven't got an authId for your site.</param>
		/// <param name="yubicoAuthorizationServer">The Yubico authorization server to use for validation and replay protection.</param>
		public YubikeyRelyingParty(int authorizationId, string yubicoAuthorizationServer) {
			if (authorizationId < 0) {
				throw new ArgumentOutOfRangeException("authorizationId");
			}

			if (!Uri.IsWellFormedUriString(yubicoAuthorizationServer, UriKind.Absolute)) {
				throw new ArgumentException("Invalid authorization server URI", "yubicoAuthorizationServer");
			}

			if (!yubicoAuthorizationServer.StartsWith(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)) {
				throw new ArgumentException("HTTPS is required for the Yubico server.  HMAC response verification not supported.", "yubicoAuthorizationServer");
			}

			this.yubicoAuthorizationId = authorizationId;
			this.yubicoAuthorizationServer = yubicoAuthorizationServer;
		}

		/// <summary>
		/// Extracts the username out of a Yubikey token.
		/// </summary>
		/// <param name="yubikeyToken">The yubikey token.</param>
		/// <returns>A 12 character string that is unique for this particular Yubikey device.</returns>
		public static string ExtractUsername(string yubikeyToken) {
			EnsureWellFormedToken(yubikeyToken);
			return yubikeyToken.Substring(0, 12);
		}

		/// <summary>
		/// Determines whether the specified yubikey token is valid and has not yet been used.
		/// </summary>
		/// <param name="yubikeyToken">The yubikey token.</param>
		/// <returns>
		/// 	<c>true</c> if the specified yubikey token is valid; otherwise, <c>false</c>.
		/// </returns>
		/// <exception cref="WebException">Thrown when the validity of the token could not be confirmed due to network issues.</exception>
		public YubikeyResult IsValid(string yubikeyToken) {
			EnsureWellFormedToken(yubikeyToken);

			StringBuilder authorizationUri = new StringBuilder(this.yubicoAuthorizationServer);
			authorizationUri.Append("?id=");
			authorizationUri.Append(Uri.EscapeDataString(this.yubicoAuthorizationId.ToString(CultureInfo.InvariantCulture)));
			authorizationUri.Append("&otp=");
			authorizationUri.Append(Uri.EscapeDataString(yubikeyToken));

			var request = WebRequest.Create(authorizationUri.ToString());
			using (var response = request.GetResponse()) {
				using (var responseReader = new StreamReader(response.GetResponseStream())) {
					string line;
					var result = new NameValueCollection();
					while ((line = responseReader.ReadLine()) != null) {
						Match m = ResultLineMatcher.Match(line);
						if (m.Success) {
							result[m.Groups["key"].Value] = m.Groups["value"].Value;
						}
					}

					return ParseResult(result["status"]);
				}
			}
		}

		/// <summary>
		/// Parses the Yubico server result.
		/// </summary>
		/// <param name="status">The status field from the response.</param>
		/// <returns>The enum value representing the result.</returns>
		private static YubikeyResult ParseResult(string status) {
			switch (status) {
				case "OK": return YubikeyResult.Ok;
				case "BAD_OTP": return YubikeyResult.BadOtp;
				case "REPLAYED_OTP": return YubikeyResult.ReplayedOtp;
				case "BAD_SIGNATURE": return YubikeyResult.BadSignature;
				case "MISSING_PARAMETER": return YubikeyResult.MissingParameter;
				case "NO_SUCH_CLIENT": return YubikeyResult.NoSuchClient;
				case "OPERATION_NOT_ALLOWED": return YubikeyResult.OperationNotAllowed;
				case "BACKEND_ERROR": return YubikeyResult.BackendError;
				default: throw new ArgumentOutOfRangeException("status", status, "Unexpected status value.");
			}
		}

		/// <summary>
		/// Ensures the OTP is well formed.
		/// </summary>
		/// <param name="yubikeyToken">The yubikey token.</param>
		private static void EnsureWellFormedToken(string yubikeyToken) {
			if (yubikeyToken == null) {
				throw new ArgumentNullException("yubikeyToken");
			}

			yubikeyToken = yubikeyToken.Trim();

			if (yubikeyToken.Length <= 12) {
				throw new ArgumentException("Yubikey token has unexpected length.");
			}
		}
	}
}
