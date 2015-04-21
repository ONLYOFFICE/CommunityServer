//-----------------------------------------------------------------------
// <copyright file="XAuthUtility.cs" company="Patrick 'Ricky' Smith">
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
// <summary>The xAuth Utility Class</summary>
//-----------------------------------------------------------------------

namespace Twitterizer
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Text.RegularExpressions;
    using Twitterizer.Core;

    /// <summary>
    /// The XAuthUtility class.
    /// </summary>
    public static class XAuthUtility
    {
        /// <summary>
        /// Allows OAuth applications to directly exchange Twitter usernames and passwords for OAuth access tokens and secrets.
        /// </summary>
        /// <param name="consumerKey">The consumer key.</param>
        /// <param name="consumerSecret">The consumer secret.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns>A <see cref="OAuthTokenResponse"/> instance.</returns>
        public static OAuthTokenResponse GetAccessTokens(string consumerKey, string consumerSecret, string username, string password)
        {
            if (string.IsNullOrEmpty(consumerKey))
            {
                throw new ArgumentNullException("consumerKey");
            }

            if (string.IsNullOrEmpty(consumerSecret))
            {
                throw new ArgumentNullException("consumerSecret");
            }

            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException("username");
            }

            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException("password");
            }

            OAuthTokenResponse response = new OAuthTokenResponse();

            try
            {
                WebRequestBuilder builder = new WebRequestBuilder(
                    new Uri("https://api.twitter.com/oauth/access_token"),
                    HTTPVerb.POST,
                    new OAuthTokens { ConsumerKey = consumerKey, ConsumerSecret = consumerSecret });

                builder.Parameters.Add("x_auth_username", username);
                builder.Parameters.Add("x_auth_password", password);
                builder.Parameters.Add("x_auth_mode", "client_auth");

                string responseBody = new StreamReader(builder.ExecuteRequest().GetResponseStream()).ReadToEnd();

                response.Token = Regex.Match(responseBody, @"oauth_token=([^&]+)").Groups[1].Value;
                response.TokenSecret = Regex.Match(responseBody, @"oauth_token_secret=([^&]+)").Groups[1].Value;
                if (responseBody.Contains("user_id="))
                    response.UserId = long.Parse(Regex.Match(responseBody, @"user_id=([^&]+)").Groups[1].Value, CultureInfo.CurrentCulture);
                response.ScreenName = Regex.Match(responseBody, @"screen_name=([^&]+)").Groups[1].Value;
            }
            catch (WebException wex)
            {
                throw new TwitterizerException(wex.Message, wex);
            }

            return response;
        }
    }
}
