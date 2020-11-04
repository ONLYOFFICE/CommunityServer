/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.Collections.Generic;
using System.Linq;
using DotNetOpenAuth.OAuth;
using DotNetOpenAuth.OAuth.ChannelElements;
using DotNetOpenAuth.OAuth.Messages;
using DotNetOpenAuth.OpenId.Extensions.OAuth;

namespace ASC.Thrdparty
{
    public class InMemoryTokenManager : IConsumerTokenManager, IOpenIdOAuthTokenManager
    {
        private readonly Dictionary<string, string> _tokensAndSecrets = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _tokensAndAssociations = new Dictionary<string, string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryTokenManager"/> class.
        /// </summary>
        /// <param name="consumerKey">The consumer key.</param>
        /// <param name="consumerSecret">The consumer secret.</param>
        public InMemoryTokenManager(string consumerKey, string consumerSecret)
        {
            if (String.IsNullOrEmpty(consumerKey))
            {
                throw new ArgumentNullException("consumerKey");
            }

            ConsumerKey = consumerKey;
            ConsumerSecret = consumerSecret;
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
        public string GetTokenSecret(string token)
        {
            return _tokensAndSecrets[token];
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
        public void StoreNewRequestToken(UnauthorizedTokenRequest request, ITokenSecretContainingMessage response)
        {
            _tokensAndSecrets[response.Token] = response.TokenSecret;
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
        public void ExpireRequestTokenAndStoreNewAccessToken(string consumerKey, string requestToken, string accessToken, string accessTokenSecret)
        {
            _tokensAndSecrets.Remove(requestToken);
            _tokensAndSecrets[accessToken] = accessTokenSecret;
        }

        /// <summary>
        /// Classifies a token as a request token or an access token.
        /// </summary>
        /// <param name="token">The token to classify.</param>
        /// <returns>Request or Access token, or invalid if the token is not recognized.</returns>
        public TokenType GetTokenType(string token)
        {
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
        public void StoreOpenIdAuthorizedRequestToken(string consumerKey, AuthorizationApprovedResponse authorization)
        {
            _tokensAndSecrets[authorization.RequestToken] = String.Empty;
        }

        #endregion

        public IEnumerable<string> GetAccessTokens()
        {
            return _tokensAndSecrets.Keys;
        }

        public IEnumerable<string> GetAssociations()
        {
            return _tokensAndAssociations.Values;
        }

        public string GetAssociation(string token)
        {
            return _tokensAndAssociations[token];
        }

        public IEnumerable<string> GetTokensByAssociation(string associateData)
        {
            return _tokensAndAssociations.Where(x => x.Value == associateData).Select(x => x.Key);
        }

        public void AssociateToken(string token, string associateData)
        {
            _tokensAndAssociations.Add(token, associateData);
        }

        public void RemoveAssociationFromToken(string token)
        {
            _tokensAndAssociations.Remove(token);
        }

        public void ExpireToken(string token)
        {
            _tokensAndSecrets.Remove(token);
        }
    }
}