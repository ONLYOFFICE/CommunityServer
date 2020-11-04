//-----------------------------------------------------------------------
// <copyright file="TwitterSearch.cs" company="Patrick 'Ricky' Smith">
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
// <summary>The twitter search class</summary>
//-----------------------------------------------------------------------

namespace Twitterizer
{
    /// <summary>
    /// The Twitter Search Class
    /// </summary>
    public static class TwitterSearch
    {
        /// <summary>
        /// Searches Twitter with the the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>
        /// A <see cref="TwitterSearchResultCollection"/> instance.
        /// </returns>
        public static TwitterResponse<TwitterSearchResultCollection> Search(string query)
        {
            return Search(query, null);
        }

        /// <summary>
        /// Searches Twitter with the the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// A <see cref="TwitterSearchResultCollection"/> instance.
        /// </returns>
        public static TwitterResponse<TwitterSearchResultCollection> Search(string query, SearchOptions options)
        {
            return Search(null, query, options);
        }

        /// <summary>
        /// Searches Twitter with the the specified query.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="query">The query.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// A <see cref="TwitterSearchResultCollection"/> instance.
        /// </returns>
        public static TwitterResponse<TwitterSearchResultCollection> Search(OAuthTokens tokens, string query, SearchOptions options)
        {
            if (options == null)
                options = new SearchOptions();

            Commands.SearchCommand command = new Twitterizer.Commands.SearchCommand(tokens, query, options);

            TwitterResponse<TwitterSearchResultCollection> results =
                Core.CommandPerformer.PerformAction(command);

            return results;
        }


    }
}
