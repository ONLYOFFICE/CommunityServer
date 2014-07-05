//-----------------------------------------------------------------------
// <copyright file="TwitterUserCategory.cs" company="Patrick 'Ricky' Smith">
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
// <summary>The TwitterUserCategory and TwitterUserCategoryCollection classes.</summary>
//-----------------------------------------------------------------------
namespace Twitterizer
{
    using System;
    using Newtonsoft.Json;
    using Twitterizer.Core;

    /// <summary>
    /// Represents a suggested user category
    /// </summary>
#if !SILVERLIGHT 
    [Serializable]
#endif
    public class TwitterUserCategory : Core.TwitterObject
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the slug.
        /// </summary>
        /// <value>The slug.</value>
        [JsonProperty(PropertyName = "slug")]
        public string Slug { get; set; }

        /// <summary>
        /// Gets or sets the number of users.
        /// Only available in list of categories.
        /// </summary>
        /// <value>The number of users.</value>
        [JsonProperty(PropertyName = "size")]
        public int NumberOfUsers { get; set; }

        /// <summary>
        /// Gets or sets the users.
        /// Users are only returned for a single category.
        /// </summary>
        /// <value>The users.</value>
        [JsonProperty(PropertyName = "users")]
        public TwitterUserCollection Users { get; set; }

        /// <summary>
        /// Access to Twitter's suggested user list. This returns the list of suggested user categories. The category can be used in the users/suggestions/category endpoint to get the users in that category.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="options">The options.</param>
        /// <returns>A collection of categories without user data.</returns>
        public static TwitterResponse<TwitterUserCategoryCollection> SuggestedUserCategories(OAuthTokens tokens, OptionalProperties options)
        {
            Commands.SuggestedUserCategoriesCommand command = new Commands.SuggestedUserCategoriesCommand(tokens, options);

            return CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// Access to Twitter's suggested user list. This returns the list of suggested user categories. The category can be used in the users/suggestions/category endpoint to get the users in that category.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <returns>
        /// A collection of categories without user data.
        /// </returns>
        public static TwitterResponse<TwitterUserCategoryCollection> SuggestedUserCategories(OAuthTokens tokens)
        {
            return SuggestedUserCategories(tokens, null);
        }

        /// <summary>
        /// Access the users in a given category of the Twitter suggested user list.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="categorySlug">The category slug.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        /// <remarks>It is recommended that end clients cache this data for no more than one hour.</remarks>
        public static TwitterResponse<TwitterUserCategory> SuggestedUsers(OAuthTokens tokens, string categorySlug, OptionalProperties options)
        {
            Commands.SuggestedUsersCommand command = new Commands.SuggestedUsersCommand(tokens, categorySlug, options);

            return CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// Access the users in a given category of the Twitter suggested user list.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="categorySlug">The category slug.</param>
        /// <returns></returns>
        /// <remarks>It is recommended that end clients cache this data for no more than one hour.</remarks>
        public static TwitterResponse<TwitterUserCategory> SuggestedUsers(OAuthTokens tokens, string categorySlug)
        {
            return SuggestedUsers(tokens, categorySlug, null);
        }
    }

    /// <summary>
    /// Represents a suggested category
    /// </summary>
#if !SILVERLIGHT 
    [Serializable]
#endif
    public class TwitterUserCategoryCollection : Core.TwitterCollection<TwitterUserCategory>, ITwitterObject
    {
        // This intentionally left blank.
        // Check out Girl Talk. He's a great DJ.
    }
}
