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
// <copyright file="InMemoryTokenManager.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.ApplicationBlock {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using DotNetOpenAuth.OAuth.ChannelElements;
	using DotNetOpenAuth.OAuth.Messages;
	using DotNetOpenAuth.OpenId.Extensions.OAuth;

#if SAMPLESONLY
	/// <summary>
	/// A token manager that only retains tokens in memory. 
	/// Meant for SHORT TERM USE TOKENS ONLY.
	/// </summary>
	/// <remarks>
	/// A likely application of this class is for "Sign In With Twitter",
	/// where the user only signs in without providing any authorization to access
	/// Twitter APIs except to authenticate, since that access token is only useful once.
	/// </remarks>
	internal class InMemoryTokenManager : IConsumerTokenManager, IOpenIdOAuthTokenManager {
		private Dictionary<string, string> tokensAndSecrets = new Dictionary<string, string>();

		/// <summary>
		/// Initializes a new instance of the <see cref="InMemoryTokenManager"/> class.
		/// </summary>
		/// <param name="consumerKey">The consumer key.</param>
		/// <param name="consumerSecret">The consumer secret.</param>
		public InMemoryTokenManager(string consumerKey, string consumerSecret) {
			if (string.IsNullOrEmpty(consumerKey)) {
				throw new ArgumentNullException("consumerKey");
			}

			this.ConsumerKey = consumerKey;
			this.ConsumerSecret = consumerSecret;
		}

		/// <summary>
		/// Gets the consumer key.
		/// </summary>
		/// <value>The consumer key.</value>
		public string ConsumerKey { get; private set; }

		/// <summary>
		/// Gets the consumer secret.
		/// </summary>
		/// <value>The consumer secret.</value>
		public string ConsumerSecret { get; private set; }

		#region ITokenManager Members

		/// <summary>
		/// Gets the Token Secret given a request or access token.
		/// </summary>
		/// <param name="token">The request or access token.</param>
		/// <returns>
		/// The secret associated with the given token.
		/// </returns>
		/// <exception cref="ArgumentException">Thrown if the secret cannot be found for the given token.</exception>
		public string GetTokenSecret(string token) {
			return this.tokensAndSecrets[token];
		}

		/// <summary>
		/// Stores a newly generated unauthorized request token, secret, and optional
		/// application-specific parameters for later recall.
		/// </summary>
		/// <param name="request">The request message that resulted in the generation of a new unauthorized request token.</param>
		/// <param name="response">The response message that includes the unauthorized request token.</param>
		/// <exception cref="ArgumentException">Thrown if the consumer key is not registered, or a required parameter was not found in the parameters collection.</exception>
		/// <remarks>
		/// Request tokens stored by this method SHOULD NOT associate any user account with this token.
		/// It usually opens up security holes in your application to do so.  Instead, you associate a user
		/// account with access tokens (not request tokens) in the <see cref="ExpireRequestTokenAndStoreNewAccessToken"/>
		/// method.
		/// </remarks>
		public void StoreNewRequestToken(UnauthorizedTokenRequest request, ITokenSecretContainingMessage response) {
			this.tokensAndSecrets[response.Token] = response.TokenSecret;
		}

		/// <summary>
		/// Deletes a request token and its associated secret and stores a new access token and secret.
		/// </summary>
		/// <param name="consumerKey">The Consumer that is exchanging its request token for an access token.</param>
		/// <param name="requestToken">The Consumer's request token that should be deleted/expired.</param>
		/// <param name="accessToken">The new access token that is being issued to the Consumer.</param>
		/// <param name="accessTokenSecret">The secret associated with the newly issued access token.</param>
		/// <remarks>
		/// 	<para>
		/// Any scope of granted privileges associated with the request token from the
		/// original call to <see cref="StoreNewRequestToken"/> should be carried over
		/// to the new Access Token.
		/// </para>
		/// 	<para>
		/// To associate a user account with the new access token,
		/// <see cref="System.Web.HttpContext.User">HttpContext.Current.User</see> may be
		/// useful in an ASP.NET web application within the implementation of this method.
		/// Alternatively you may store the access token here without associating with a user account,
		/// and wait until <see cref="WebConsumer.ProcessUserAuthorization()"/> or
		/// <see cref="DesktopConsumer.ProcessUserAuthorization(string, string)"/> return the access
		/// token to associate the access token with a user account at that point.
		/// </para>
		/// </remarks>
		public void ExpireRequestTokenAndStoreNewAccessToken(string consumerKey, string requestToken, string accessToken, string accessTokenSecret) {
			this.tokensAndSecrets.Remove(requestToken);
			this.tokensAndSecrets[accessToken] = accessTokenSecret;
		}

		/// <summary>
		/// Classifies a token as a request token or an access token.
		/// </summary>
		/// <param name="token">The token to classify.</param>
		/// <returns>Request or Access token, or invalid if the token is not recognized.</returns>
		public TokenType GetTokenType(string token) {
			throw new NotImplementedException();
		}

		#endregion

		#region IOpenIdOAuthTokenManager Members

		/// <summary>
		/// Stores a new request token obtained over an OpenID request.
		/// </summary>
		/// <param name="consumerKey">The consumer key.</param>
		/// <param name="authorization">The authorization message carrying the request token and authorized access scope.</param>
		/// <remarks>
		/// 	<para>The token secret is the empty string.</para>
		/// 	<para>Tokens stored by this method should be short-lived to mitigate
		/// possible security threats.  Their lifetime should be sufficient for the
		/// relying party to receive the positive authentication assertion and immediately
		/// send a follow-up request for the access token.</para>
		/// </remarks>
		public void StoreOpenIdAuthorizedRequestToken(string consumerKey, AuthorizationApprovedResponse authorization) {
			this.tokensAndSecrets[authorization.RequestToken] = string.Empty;
		}

		#endregion
	}
#else
#error The InMemoryTokenManager class is only for samples as it forgets all tokens whenever the application restarts!  You should implement IConsumerTokenManager in your own app that stores tokens in a persistent store (like a SQL database).
#endif
}