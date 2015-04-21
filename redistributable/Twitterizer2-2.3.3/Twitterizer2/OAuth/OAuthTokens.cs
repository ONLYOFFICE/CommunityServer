//-----------------------------------------------------------------------
// <copyright file="OAuthTokens.cs" company="Patrick 'Ricky' Smith">
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
// <summary>Contains assembly information.</summary>
//-----------------------------------------------------------------------

namespace Twitterizer
{
    /// <include file='OAuthTokens.xml' path='OAuthTokens/OAuthTokens/*'/>
#if !SILVERLIGHT
    [System.Serializable]
#endif
    public class OAuthTokens
    {
        /// <summary>
        /// Gets or sets the access token.
        /// </summary>
        /// <value>The access token.</value>
        public string AccessToken { internal get; set; }

        /// <summary>
        /// Gets or sets the access token secret.
        /// </summary>
        /// <value>The access token secret.</value>
        public string AccessTokenSecret { internal get; set; }

        /// <summary>
        /// Gets or sets the consumer key.
        /// </summary>
        /// <value>The consumer key.</value>
        public string ConsumerKey { internal get; set; }

        /// <summary>
        /// Gets or sets the consumer secret.
        /// </summary>
        /// <value>The consumer secret.</value>
        public string ConsumerSecret { internal get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance has consumer token values.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has consumer token; otherwise, <c>false</c>.
        /// </value>
        public bool HasConsumerToken
        {
            get
            {
                return !string.IsNullOrEmpty(this.ConsumerKey) && !string.IsNullOrEmpty(this.ConsumerSecret);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has access token values.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has access token; otherwise, <c>false</c>.
        /// </value>
        public bool HasAccessToken
        {
            get
            {
                return !string.IsNullOrEmpty(this.AccessToken) && !string.IsNullOrEmpty(this.AccessTokenSecret);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has values. This does not verify that the values are correct.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has values; otherwise, <c>false</c>.
        /// </value>
        public bool HasBothTokens
        {
            get
            {
                return this.HasAccessToken && this.HasConsumerToken;
            }
        }
    }
}
