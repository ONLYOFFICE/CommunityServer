//-----------------------------------------------------------------------
// <copyright file="TwitterRateLimitStatus.cs" company="Patrick 'Ricky' Smith">
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
// <summary>The twitter rate limit status class.</summary>
//-----------------------------------------------------------------------
namespace Twitterizer
{
    using System;
    using Newtonsoft.Json;
    using Twitterizer.Core;

    /// <summary>
    /// The Twitter Rate Limit Status class
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class TwitterRateLimitStatus : TwitterObject
    {
        #region API Properties
        /// <summary>
        /// Gets or sets the remaining hits.
        /// </summary>
        /// <value>The remaining hits.</value>
        [JsonProperty(PropertyName = "remaining_hits")]
        public int RemainingHits { get; set; }

        /// <summary>
        /// Gets or sets the hourly limit.
        /// </summary>
        /// <value>The hourly limit.</value>
        [JsonProperty(PropertyName = "hourly_limit")]
        public int HourlyLimit { get; set; }

        /// <summary>
        /// Gets or sets the UTC string value of the time rate limiting will reset.
        /// </summary>
        /// <value>The reset time string.</value>
        [JsonProperty(PropertyName = "reset_time")]
        [JsonConverter(typeof(TwitterizerDateConverter))]
        public DateTime ResetTime { get; set; }
        #endregion

        /// <summary>
        /// Gets the rate limiting status status for the authenticated user.
        /// </summary>
        /// <param name="tokens">The OAuth tokens.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// A <see cref="TwitterRateLimitStatus"/> instance.
        /// </returns>
        public static TwitterResponse<TwitterRateLimitStatus> GetStatus(OAuthTokens tokens, OptionalProperties options)
        {
            Commands.RateLimitStatusCommand command = new Twitterizer.Commands.RateLimitStatusCommand(tokens, options);
            TwitterResponse<TwitterRateLimitStatus> result = Core.CommandPerformer.PerformAction(command);

            return result;
        }

        /// <summary>
        /// Gets the rate limiting status status based on the application's IP address.
        /// </summary>
        /// <param name="tokens">The OAuth tokens.</param>
        /// <returns>
        /// A <see cref="TwitterRateLimitStatus"/> instance.
        /// </returns>
        public static TwitterResponse<TwitterRateLimitStatus> GetStatus(OAuthTokens tokens)
        {
            return GetStatus(tokens, null);
        }

        /// <summary>
        /// Gets the rate limiting status status based on the application's IP address.
        /// </summary>
        /// <returns>
        /// A <see cref="TwitterRateLimitStatus"/> instance.
        /// </returns>
        public static TwitterResponse<TwitterRateLimitStatus> GetStatus()
        {
            return GetStatus(null, null);
        }
    }
}