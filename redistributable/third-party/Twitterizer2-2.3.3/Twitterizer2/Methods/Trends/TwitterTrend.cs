//-----------------------------------------------------------------------
// <copyright file="TwitterTrend.cs" company="Patrick 'Ricky' Smith">
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
// <summary>The Twitter Trend class</summary>
//-----------------------------------------------------------------------

namespace Twitterizer
{
    using System;
    using System.Runtime.Serialization;
    using Twitterizer.Core;

    /// <summary>
    /// The TwitterTrend class.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    [DataContract]
    public class TwitterTrend : TwitterObject
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name of the trend.</value>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the address.
        /// </summary>
        /// <value>The address.</value>
        [DataMember]
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets the search query.
        /// </summary>
        /// <value>The search query.</value>
        [DataMember]
        public string SearchQuery { get; set; }

        /// <summary>
        /// Gets or sets the promoted content value.
        /// </summary>
        /// <value>Promoted Content.</value>
        [DataMember]
        public string PromotedContent { get; set; }

        /// <summary>
        /// Gets or sets the events.
        /// </summary>
        /// <value>The events.</value>
        [DataMember]
        public string Events { get; set; }

        /// <summary>
        /// Gets the trends with the specified WOEID.
        /// </summary>
        /// <param name="tokens">The request tokens.</param>
        /// <param name="WoeID">The WOEID.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// A collection of <see cref="Twitterizer.TwitterTrend"/> objects.
        /// </returns>
        public static TwitterResponse<TwitterTrendCollection> Trends(OAuthTokens tokens, int WoeID, LocalTrendsOptions options)
        {
            Commands.TrendsCommand command = new Twitterizer.Commands.TrendsCommand(tokens, WoeID, options);

            return Core.CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// Gets the current trends.
        /// </summary>
        /// <param name="tokens">The request tokens.</param>
        /// <param name="WoeID">The WOEID.</param>
        /// <returns>
        /// A collection of <see cref="Twitterizer.TwitterTrend"/> objects.
        /// </returns>
        public static TwitterResponse<TwitterTrendCollection> Trends(OAuthTokens tokens, int WoeID)
        {
            return Trends(tokens, WoeID, null);
        }

        /// <summary>
        /// Gets the trends with the specified WOEID.
        /// </summary>
        /// <param name="WoeID">The WOEID.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// A collection of <see cref="Twitterizer.TwitterTrend"/> objects.
        /// </returns>
        public static TwitterResponse<TwitterTrendCollection> Trends(int WoeID, LocalTrendsOptions options)
        {
            return Trends(null, WoeID, options);
        }

        /// <summary>
        /// Gets the current trends.
        /// </summary>
        /// <param name="WoeID">The WOEID.</param>
        /// <returns>
        /// A collection of <see cref="Twitterizer.TwitterTrend"/> objects.
        /// </returns>
        public static TwitterResponse<TwitterTrendCollection> Trends(int WoeID)
        {
            return Trends(null, WoeID, null);
        }

        /// <summary>
        /// Gets the locations where trends are available.
        /// </summary>   
        /// <param name="tokens">The request tokens.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// A collection of <see cref="Twitterizer.TwitterTrendLocation"/> objects.
        /// </returns>
        public static TwitterResponse<TwitterTrendLocationCollection> Available(OAuthTokens tokens, AvailableTrendsOptions options)
        {
            Commands.AvailableTrendsCommand command = new Twitterizer.Commands.AvailableTrendsCommand(tokens, options);

            return Core.CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// Gets the locations where trends are available.
        /// </summary>          
        /// <param name="options">The options.</param>
        /// <returns>
        /// A collection of <see cref="Twitterizer.TwitterTrendLocation"/> objects.
        /// </returns>
        public static TwitterResponse<TwitterTrendLocationCollection> Available(AvailableTrendsOptions options)
        {
            return Available(null, options);
        }

        /// <summary>
        /// Gets the locations where trends are available.
        /// </summary>          
        /// <returns>
        /// A collection of <see cref="Twitterizer.TwitterTrendLocation"/> objects.
        /// </returns>
        public static TwitterResponse<TwitterTrendLocationCollection> Available()
        {
            return Available(null, null);
        }


        /// <summary>
        /// Gets the daily global trends
        /// </summary>
        /// <param name="tokens">The request tokens.</param>
        /// <param name="options">The options.</param>
        public static TwitterResponse<TwitterTrendDictionary> Daily(OAuthTokens tokens, TrendsOptions options)
        {
            Commands.DailyTrendsCommand command = new Twitterizer.Commands.DailyTrendsCommand(tokens, options);

            return Core.CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// Gets the daily global trends
        /// </summary>
        /// <param name="options">The options.</param>
        public static TwitterResponse<TwitterTrendDictionary> Daily(TrendsOptions options)
        {
            return Daily(null, options);
        }

        /// <summary>
        /// Gets the daily global trends
        /// </summary>
        public static TwitterResponse<TwitterTrendDictionary> Daily()
        {
            return Daily(null, null);
        }

        /// <summary>
        /// Gets the weekly global trends
        /// </summary>
        /// <param name="tokens">The request tokens.</param>
        /// <param name="options">The options.</param>
        public static TwitterResponse<TwitterTrendDictionary> Weekly(OAuthTokens tokens, TrendsOptions options)
        {
            Commands.WeeklyTrendsCommand command = new Twitterizer.Commands.WeeklyTrendsCommand(tokens, options);

            return Core.CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// Gets the weekly global trends
        /// </summary>
        /// <param name="options">The options.</param>
        public static TwitterResponse<TwitterTrendDictionary> Weekly(TrendsOptions options)
        {
            return Weekly(null, options);
        }

        /// <summary>
        /// Gets the weekly global trends
        /// </summary>
        public static TwitterResponse<TwitterTrendDictionary> Weekly()
        {
            return Weekly(null, null);
        }
    }
}
