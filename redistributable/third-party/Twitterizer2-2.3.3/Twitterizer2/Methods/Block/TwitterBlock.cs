//-----------------------------------------------------------------------
// <copyright file="TwitterBlock.cs" company="Patrick 'Ricky' Smith">
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
// <summary>The twitter block class.</summary>
//-----------------------------------------------------------------------
namespace Twitterizer
{
    /// <summary>
    /// Provides methods for interacting with user blocks.
    /// </summary>
    public static class TwitterBlock
    {
        /// <summary>
        /// Blocks the user specified as the authenticating user. Destroys a friendship to the blocked user if it exists.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// The blocked user in the requested format when successful.
        /// </returns>
        public static TwitterResponse<TwitterUser> Create(OAuthTokens tokens, decimal userId, OptionalProperties options)
        {
            Commands.CreateBlockCommand command = new Commands.CreateBlockCommand(tokens, string.Empty, userId, options);

            return Core.CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// Blocks the user specified as the authenticating user. Destroys a friendship to the blocked user if it exists.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="userId">The user id.</param>
        /// <returns>
        /// The blocked user in the requested format when successful.
        /// </returns>
        public static TwitterResponse<TwitterUser> Create(OAuthTokens tokens, decimal userId)
        {
            return Create(tokens, userId, null);
        }

        /// <summary>
        /// Blocks the user specified as the authenticating user. Destroys a friendship to the blocked user if it exists.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="screenName">The user's screen name.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// The blocked user in the requested format when successful.
        /// </returns>
        public static TwitterResponse<TwitterUser> Create(OAuthTokens tokens, string screenName, OptionalProperties options)
        {
            Commands.CreateBlockCommand command = new Commands.CreateBlockCommand(tokens, screenName, -1, options);

            return Core.CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// Blocks the user specified as the authenticating user. Destroys a friendship to the blocked user if it exists.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="screenName">The user's screen name.</param>
        /// <returns>
        /// The blocked user in the requested format when successful.
        /// </returns>
        public static TwitterResponse<TwitterUser> Create(OAuthTokens tokens, string screenName)
        {
            return Create(tokens, screenName, null);
        }

        /// <summary>
        /// Unblocks the user specified as the authenticating user.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// The unblocked user in the requested format when successful.
        /// </returns>
        public static TwitterResponse<TwitterUser> Destroy(OAuthTokens tokens, decimal userId, OptionalProperties options)
        {
            Commands.DestroyBlockCommand command = new Commands.DestroyBlockCommand(tokens, string.Empty, userId, options);

            return Core.CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// Unblocks the user specified as the authenticating user.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="userId">The user id.</param>
        /// <returns>
        /// The unblocked user in the requested format when successful.
        /// </returns>
        public static TwitterResponse<TwitterUser> Destroy(OAuthTokens tokens, decimal userId)
        {
            return Destroy(tokens, userId, null);
        }

        /// <summary>
        /// Unblocks the user specified as the authenticating user.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="screenName">The user's screen name.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// The unblocked user in the requested format when successful.
        /// </returns>
        public static TwitterResponse<TwitterUser> Destroy(OAuthTokens tokens, string screenName, OptionalProperties options)
        {
            Commands.DestroyBlockCommand command = new Commands.DestroyBlockCommand(tokens, screenName, -1, options);

            return Core.CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// Unblocks the user specified as the authenticating user.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="screenName">The user's screen name.</param>
        /// <returns>
        /// The unblocked user in the requested format when successful.
        /// </returns>
        public static TwitterResponse<TwitterUser> Destroy(OAuthTokens tokens, string screenName)
        {
            return Destroy(tokens, screenName, null);
        }

        /// <summary>
        /// Checks for a block against the the user specified as the authenticating user.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// The blocked user in the requested format when successful.
        /// </returns>
        public static TwitterResponse<TwitterUser> Exists(OAuthTokens tokens, decimal userId, OptionalProperties options)
        {
            Commands.ExistsBlockCommand command = new Commands.ExistsBlockCommand(tokens, string.Empty, userId, options);

            return Core.CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// Checks for a block against the the user specified as the authenticating user.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="userId">The user id.</param>
        /// <returns>
        /// The blocked user in the requested format when successful.
        /// </returns>
        public static TwitterResponse<TwitterUser> Exists(OAuthTokens tokens, decimal userId)
        {
            return Exists(tokens, userId, null);
        }

        /// <summary>
        /// Checks for a block against the the user specified as the authenticating user.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="screenName">The user's screen name.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// The blocked user in the requested format when successful.
        /// </returns>
        public static TwitterResponse<TwitterUser> Exists(OAuthTokens tokens, string screenName, OptionalProperties options)
        {
            Commands.ExistsBlockCommand command = new Commands.ExistsBlockCommand(tokens, screenName, -1, options);

            return Core.CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// Checks for a block against the the user specified as the authenticating user.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="screenName">The user's screen name.</param>
        /// <returns>
        /// The blocked user in the requested format when successful.
        /// </returns>
        public static TwitterResponse<TwitterUser> Exists(OAuthTokens tokens, string screenName)
        {
            return Exists(tokens, screenName, null);
        }

        /// <summary>
        /// Returns a collection of user objects that the authenticating user is blocking.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public static TwitterResponse<TwitterUserCollection> Blocking(OAuthTokens tokens, BlockingOptions options)
        {
            Commands.BlockingCommand command = new Commands.BlockingCommand(tokens, options);
            return Core.CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// Returns a collection of user objects that the authenticating user is blocking.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static TwitterResponse<TwitterUserCollection> Blocking(OAuthTokens tokens)
        {
            return Blocking(tokens, null);
        }

        /// <summary>
        /// Returns an collection of user ids the authenticating user is blocking.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="options">The options.</param>
        /// <returns>A collection of user ids.</returns>
        public static TwitterResponse<TwitterIdCollection> BlockingIds(OAuthTokens tokens, OptionalProperties options)
        {
            Commands.BlockingIdsCommand command = new Commands.BlockingIdsCommand(tokens, options);

            return Core.CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// Returns an collection of user ids the authenticating user is blocking.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <returns>A collection of user ids.</returns>
        public static TwitterResponse<TwitterIdCollection> BlockingIds(OAuthTokens tokens)
        {
            return BlockingIds(tokens, null);
        }
    }
}
