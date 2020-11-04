//-----------------------------------------------------------------------
// <copyright file="TwitterSavedSearch.cs" company="Patrick 'Ricky' Smith">
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
// <author>David Golden</author>
// <summary>The twitter saved search class.</summary>
//-----------------------------------------------------------------------

namespace Twitterizer
{
    using System;
    using Twitterizer.Core;

    /// <summary>
    /// The TwitterSavedSearch class. Provides static methods for manipulating saved searches tweets.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    public sealed class TwitterSavedSearch : TwitterObject
    {
        ///// <summary>
        ///// Prevents a default instance of the TwitterSavedSearch class from being created.
        ///// </summary>
        //private TwitterSavedSearch()
        //{ 
        //}

        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        /// <value>The Id of the saved search.</value>
        public decimal Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name of the saved search.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the query.
        /// </summary>
        /// <value>The query.</value>
        public string Query { get; set; }

        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        /// <value>The position.</value>
        public int? Position { get; set; }

        /// <summary>
        /// Gets or sets the created at date time.
        /// </summary>
        /// <value>The created at.</value>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Creates the saved search specified in the query parameter as the authenticating user.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="query">The query.</param>
        /// <param name="options">The options.</param>
        /// <returns>The saved search when successful.</returns>
        public static TwitterResponse<TwitterSavedSearch> Create(OAuthTokens tokens, string query, OptionalProperties options)
        {
            return CommandPerformer.PerformAction(
                new Commands.CreateSavedSearchCommand(tokens, query, options));
        }

        /// <summary>
        /// Creates the saved search specified in the query parameter as the authenticating user.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="query">The query.</param>
        /// <returns>The saved search when successful.</returns>
        public static TwitterResponse<TwitterSavedSearch> Create(OAuthTokens tokens, string query)
        {
            return Create(tokens, query, null);
        }

        /// <summary>
        /// Deletes the saved search specified in the ID parameter as the authenticating user.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="savedsearchId">The saved search id.</param>
        /// <param name="options">The options.</param>
        /// <returns>The deleted saved search in the requested format when successful.</returns>
        public static TwitterResponse<TwitterSavedSearch> Delete(OAuthTokens tokens, decimal savedsearchId, OptionalProperties options)
        {
            return CommandPerformer.PerformAction(
                new Commands.DeleteSavedSearchCommand(tokens, savedsearchId, options));
        }

        /// <summary>
        /// Deletes the saved search specified in the ID parameter as the authenticating user.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="savedsearchId">The saved search id.</param>
        /// <returns>
        /// The deleted saved search in the requested format when successful
        /// </returns>
        public static TwitterResponse<TwitterSavedSearch> Delete(OAuthTokens tokens, decimal savedsearchId)
        {
            return Delete(tokens, savedsearchId, null);
        }

        /// <summary>
        /// Returns the the authenticating user's saved search queries in the requested format.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="options">The options.</param>
        /// <returns>The saved searches</returns>
        public static TwitterResponse<TwitterSavedSearchCollection> SavedSearches(OAuthTokens tokens, OptionalProperties options)
        {
            return CommandPerformer.PerformAction(
                new Commands.SavedSearchesCommand(tokens, options));
        }

        /// <summary>
        /// Returns the the authenticating user's saved search queries in the requested format.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <returns>The saved searches</returns>
        public static TwitterResponse<TwitterSavedSearchCollection> SavedSearches(OAuthTokens tokens)
        {
            return SavedSearches(tokens, null);
        }

        /// <summary>
        /// Returns the the authenticating user's saved search queries in the requested format.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>The saved searches</returns>
        public static TwitterResponse<TwitterSavedSearchCollection> SavedSearches(OptionalProperties options)
        {
            return SavedSearches(null, options);
        }
    }
}
