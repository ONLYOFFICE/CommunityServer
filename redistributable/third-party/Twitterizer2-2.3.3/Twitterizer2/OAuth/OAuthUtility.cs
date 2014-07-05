//-----------------------------------------------------------------------
// <copyright file="OAuthUtility.cs" company="Patrick 'Ricky' Smith">
//  This file is part of the Twitterizer library (http://www.twitterizer.net/)
// 
//  Copyright (c) 2010, Patrick "Ricky" Smith (ricky@digitally-born.com)
//  All rights reserved.
//  
//  Redistribution and use in source and binary forms, with or without modification, are 
//  permitted provided that the following conditions are met:
// 
//  - Redistributions of source code must retain the above copyright notice, this list 
//    of conditions and the following disclaimer.
//  - Redistributions in binary form must reproduce the above copyright notice, this list 
//    of conditions and the following disclaimer in the documentation and/or other 
//    materials provided with the distribution.
//  - Neither the name of the Twitterizer nor the names of its contributors may be 
//    used to endorse or promote products derived from this software without specific 
//    prior written permission.
// 
//  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
//  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
//  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
//  IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
//  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT 
//  NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR 
//  PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
//  WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
//  ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
//  POSSIBILITY OF SUCH DAMAGE.
// </copyright>
// <author>Ricky Smith</author>
// <summary>Provides simple methods to simplify OAuth interaction.</summary>
//-----------------------------------------------------------------------

namespace Twitterizer
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;
#if !SILVERLIGHT
    using System.Web;
#endif

    /// <include file='OAuthUtility.xml' path='OAuthUtility/OAuthUtility/*'/>
    public static class OAuthUtility
    {
        #region Public Methods
        /// <summary>
        /// Gets the request token.
        /// </summary>
        /// <param name="consumerKey">The consumer key.</param>
        /// <param name="consumerSecret">The consumer secret.</param>
        /// <param name="callbackAddress">The callback address. For PIN-based authentication "oob" should be supplied.</param>
        /// <returns></returns>
        public static OAuthTokenResponse GetRequestToken(string consumerKey, string consumerSecret, string callbackAddress)
        {
            if (string.IsNullOrEmpty(consumerKey))
            {
                throw new ArgumentNullException("consumerKey");
            }

            if (string.IsNullOrEmpty(consumerSecret))
            {
                throw new ArgumentNullException("consumerSecret");
            }

            if (string.IsNullOrEmpty(callbackAddress))
            {
                throw new ArgumentNullException("callbackAddress", @"You must always provide a callback url when obtaining a request token. For PIN-based authentication, use ""oob"" as the callback url.");
            }

            WebRequestBuilder builder = new WebRequestBuilder(
                new Uri("https://api.twitter.com/oauth/request_token"),
                HTTPVerb.POST,
                new OAuthTokens { ConsumerKey = consumerKey, ConsumerSecret = consumerSecret });

            if (!string.IsNullOrEmpty(callbackAddress))
            {
                builder.Parameters.Add("oauth_callback", callbackAddress);
            }

            string responseBody = null;

            try
            {
                HttpWebResponse webResponse = builder.ExecuteRequest();
                Stream responseStream = webResponse.GetResponseStream();
                if (responseStream != null) responseBody = new StreamReader(responseStream).ReadToEnd();
            }
            catch (WebException wex)
            {
                throw new TwitterizerException(wex.Message, wex);
            }

            return new OAuthTokenResponse
            {
                Token = ParseQuerystringParameter("oauth_token", responseBody),
                TokenSecret = ParseQuerystringParameter("oauth_token_secret", responseBody),
                VerificationString = ParseQuerystringParameter("oauth_verifier", responseBody)
            };
        }

        /// <summary>
        /// Tries to the parse querystring parameter.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="text">The text.</param>
        /// <returns>The value of the parameter or an empty string.</returns>
        /// <remarks></remarks>
        private static string ParseQuerystringParameter(string parameterName, string text)
        {
            Match expressionMatch = Regex.Match(text, string.Format(@"{0}=(?<value>[^&]+)", parameterName));

            if (!expressionMatch.Success)
            {
                return string.Empty;
            }

            return expressionMatch.Groups["value"].Value;
        }

#if !SILVERLIGHT
        /// <summary>
        /// Gets a new OAuth request token from the twitter api.
        /// </summary>
        /// <param name="consumerKey">The consumer key.</param>
        /// <param name="consumerSecret">The consumer secret.</param>
        /// <param name="callbackAddress">The callback address. For PIN-based authentication "oob" should be supplied.</param>
        /// <param name="proxy">The proxy.</param>
        /// <returns>
        /// A new <see cref="Twitterizer.OAuthTokenResponse"/> instance.
        /// </returns>
        public static OAuthTokenResponse GetRequestToken(string consumerKey, string consumerSecret, string callbackAddress, WebProxy proxy)
        {
            if (string.IsNullOrEmpty(consumerKey))
            {
                throw new ArgumentNullException("consumerKey");
            }

            if (string.IsNullOrEmpty(consumerSecret))
            {
                throw new ArgumentNullException("consumerSecret");
            }

            if (string.IsNullOrEmpty(callbackAddress))
            {
                throw new ArgumentNullException("callbackAddress", @"You must always provide a callback url when obtaining a request token. For PIN-based authentication, use ""oob"" as the callback url.");
            }

            WebRequestBuilder builder = new WebRequestBuilder(
                new Uri("https://api.twitter.com/oauth/request_token"),
                HTTPVerb.POST,
                new OAuthTokens { ConsumerKey = consumerKey, ConsumerSecret = consumerSecret }) { Proxy = proxy };

            if (!string.IsNullOrEmpty(callbackAddress))
            {
                builder.Parameters.Add("oauth_callback", callbackAddress);
            }

            string responseBody = null;

            try
            {
                HttpWebResponse webResponse = builder.ExecuteRequest();
                Stream responseStream = webResponse.GetResponseStream();
                if (responseStream != null) responseBody = new StreamReader(responseStream).ReadToEnd();
            }
            catch (WebException wex)
            {
                throw new TwitterizerException(wex.Message, wex);
            }

            Match matchedValues = Regex.Match(responseBody,
                                              @"oauth_token=(?<token>[^&]+)|oauth_token_secret=(?<secret>[^&]+)|oauth_verifier=(?<verifier>[^&]+)");

            return new OAuthTokenResponse
            {
                Token = matchedValues.Groups["token"].Value,
                TokenSecret = matchedValues.Groups["secret"].Value,
                VerificationString = matchedValues.Groups["verifier"].Value
            };
        }
#endif

        /// <summary>
        /// Gets the access token.
        /// </summary>
        /// <param name="consumerKey">The consumer key.</param>
        /// <param name="consumerSecret">The consumer secret.</param>
        /// <param name="requestToken">The request token.</param>
        /// <param name="verifier">The pin number or verifier string.</param>
        /// <returns>
        /// An <see cref="OAuthTokenResponse"/> class containing access token information.
        /// </returns>
        public static OAuthTokenResponse GetAccessToken(string consumerKey, string consumerSecret, string requestToken, string verifier)
        {
            if (string.IsNullOrEmpty(consumerKey))
            {
                throw new ArgumentNullException("consumerKey");
            }

            if (string.IsNullOrEmpty(consumerSecret))
            {
                throw new ArgumentNullException("consumerSecret");
            }

            if (string.IsNullOrEmpty(requestToken))
            {
                throw new ArgumentNullException("requestToken");
            }

            WebRequestBuilder builder = new WebRequestBuilder(
                new Uri("https://api.twitter.com/oauth/access_token"),
                HTTPVerb.GET,
				new OAuthTokens { ConsumerKey = consumerKey, ConsumerSecret = consumerSecret });

            if (!string.IsNullOrEmpty(verifier))
            {
                builder.Parameters.Add("oauth_verifier", verifier);
            }

            builder.Parameters.Add("oauth_token", requestToken);

            string responseBody;

            try
            {
                HttpWebResponse webResponse = builder.ExecuteRequest();

                responseBody = new StreamReader(webResponse.GetResponseStream()).ReadToEnd();
            }
            catch (WebException wex)
            {
                throw new TwitterizerException(wex.Message, wex);
            }

            OAuthTokenResponse response = new OAuthTokenResponse();
            response.Token = Regex.Match(responseBody, @"oauth_token=([^&]+)").Groups[1].Value;
            response.TokenSecret = Regex.Match(responseBody, @"oauth_token_secret=([^&]+)").Groups[1].Value;
            response.UserId = long.Parse(Regex.Match(responseBody, @"user_id=([^&]+)").Groups[1].Value, CultureInfo.CurrentCulture);
            response.ScreenName = Regex.Match(responseBody, @"screen_name=([^&]+)").Groups[1].Value;
            return response;
        }

#if !SILVERLIGHT
        /// <summary>
        /// Gets the access token.
        /// </summary>
        /// <param name="consumerKey">The consumer key.</param>
        /// <param name="consumerSecret">The consumer secret.</param>
        /// <param name="requestToken">The request token.</param>
        /// <param name="verifier">The pin number or verifier string.</param>
        /// <param name="proxy">The proxy.</param>
        /// <returns>
        /// An <see cref="OAuthTokenResponse"/> class containing access token information.
        /// </returns>
        public static OAuthTokenResponse GetAccessToken(string consumerKey, string consumerSecret, string requestToken, string verifier, WebProxy proxy)
        {
            if (string.IsNullOrEmpty(consumerKey))
            {
                throw new ArgumentNullException("consumerKey");
            }

            if (string.IsNullOrEmpty(consumerSecret))
            {
                throw new ArgumentNullException("consumerSecret");
            }

            if (string.IsNullOrEmpty(requestToken))
            {
                throw new ArgumentNullException("requestToken");
            }

            WebRequestBuilder builder = new WebRequestBuilder(
                new Uri("https://api.twitter.com/oauth/access_token"),
                HTTPVerb.GET,
				new OAuthTokens { ConsumerKey = consumerKey, ConsumerSecret = consumerSecret });

            builder.Proxy = proxy;

            if (!string.IsNullOrEmpty(verifier))
            {
                builder.Parameters.Add("oauth_verifier", verifier);
            }

            builder.Parameters.Add("oauth_token", requestToken);

            string responseBody;

            try
            {
                HttpWebResponse webResponse = builder.ExecuteRequest();

                responseBody = new StreamReader(webResponse.GetResponseStream()).ReadToEnd();
            }
            catch (WebException wex)
            {
                throw new TwitterizerException(wex.Message, wex);
            }

            OAuthTokenResponse response = new OAuthTokenResponse();
            response.Token = Regex.Match(responseBody, @"oauth_token=([^&]+)").Groups[1].Value;
            response.TokenSecret = Regex.Match(responseBody, @"oauth_token_secret=([^&]+)").Groups[1].Value;
            response.UserId = long.Parse(Regex.Match(responseBody, @"user_id=([^&]+)").Groups[1].Value, CultureInfo.CurrentCulture);
            response.ScreenName = Regex.Match(responseBody, @"screen_name=([^&]+)").Groups[1].Value;
            return response;
        }
#endif
        #endregion

        /// <summary>
        /// Builds the authorization URI.
        /// </summary>
        /// <param name="requestToken">The request token.</param>
        /// <returns>A new <see cref="Uri"/> instance.</returns>
        public static Uri BuildAuthorizationUri(string requestToken)
        {
            return BuildAuthorizationUri(requestToken, false);
        }

        /// <summary>
        /// Builds the authorization URI.
        /// </summary>
        /// <param name="requestToken">The request token.</param>
        /// <param name="authenticate">if set to <c>true</c>, the authenticate url will be used. (See: "Sign in with Twitter")</param>
        /// <returns>A new <see cref="Uri"/> instance.</returns>
        public static Uri BuildAuthorizationUri(string requestToken, bool authenticate)
        {
            StringBuilder parameters = new StringBuilder("https://twitter.com/oauth/");

            if (authenticate)
            {
                parameters.Append("authenticate");
            }
            else
            {
                parameters.Append("authorize");
            }

            parameters.AppendFormat("?oauth_token={0}", requestToken);

            return new Uri(parameters.ToString());
        }

#if !LITE && !SILVERLIGHT
        /// <summary>
        /// Adds the OAuth Echo header to the supplied web request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="tokens">The tokens.</param>
        public static void AddOAuthEchoHeader(WebRequest request, OAuthTokens tokens)
        {
            WebRequestBuilder builder = new WebRequestBuilder(
                new Uri("https://api.twitter.com/1/account/verify_credentials.json"), 
                HTTPVerb.POST,
				tokens);

            builder.PrepareRequest();

            request.Headers.Add("X-Verify-Credentials-Authorization", builder.GenerateAuthorizationHeader());
            request.Headers.Add("X-Auth-Service-Provider", "https://api.twitter.com/1/account/verify_credentials.json");
        }
#endif
    }
}
