//-----------------------------------------------------------------------
// <copyright file="TwitterNotification.cs" company="Patrick 'Ricky' Smith">
//  This file is part of the Twitterizer library (http://www.twitterizer.net)
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
// <summary>The twitter notification class.</summary>
//-----------------------------------------------------------------------
namespace Twitterizer
{
    /// <summary>
    /// Provides methods to update a user's preferences on notifications. For example, whether a user will be notified on mention via SMS.
    /// </summary>
    public static class TwitterNotification
    {
        /// <summary>
        /// Enables device notifications for updates from the specified user. Returns the specified user when successful.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public static TwitterResponse<TwitterUser> Follow(OAuthTokens tokens, decimal userId, OptionalProperties options)
        {
            Commands.NotificationFollowCommand command = new Commands.NotificationFollowCommand(tokens, userId, string.Empty, options);

            return Core.CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// Enables device notifications for updates from the specified user. Returns the specified user when successful.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="userId">The user id.</param>
        /// <returns></returns>
        public static TwitterResponse<TwitterUser> Follow(OAuthTokens tokens, decimal userId)
        {
            return Follow(tokens, userId, null);
        }

        /// <summary>
        /// Enables device notifications for updates from the specified user. Returns the specified user when successful.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="screenName">The user's screen name.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public static TwitterResponse<TwitterUser> Follow(OAuthTokens tokens, string screenName, OptionalProperties options)
        {
            Commands.NotificationFollowCommand command = new Commands.NotificationFollowCommand(tokens, 0, screenName, options);

            return Core.CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// Enables device notifications for updates from the specified user. Returns the specified user when successful.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="screenName">The user's screen name.</param>
        /// <returns></returns>
        public static TwitterResponse<TwitterUser> Follow(OAuthTokens tokens, string screenName)
        {
            return Follow(tokens, screenName, null);
        }

        /// <summary>
        /// Disables notifications for updates from the specified user to the authenticating user. Returns the specified user when successful.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public static TwitterResponse<TwitterUser> Leave(OAuthTokens tokens, decimal userId, OptionalProperties options)
        {
            Commands.NotificationLeaveCommand command = new Commands.NotificationLeaveCommand(tokens, userId, string.Empty, options);

            return Core.CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// Disables notifications for updates from the specified user to the authenticating user. Returns the specified user when successful.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="userId">The user id.</param>
        /// <returns></returns>
        public static TwitterResponse<TwitterUser> Leave(OAuthTokens tokens, decimal userId)
        {
            return Follow(tokens, userId, null);
        }

        /// <summary>
        /// Disables notifications for updates from the specified user to the authenticating user. Returns the specified user when successful.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="screenName">The user's screen name.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public static TwitterResponse<TwitterUser> Leave(OAuthTokens tokens, string screenName, OptionalProperties options)
        {
            Commands.NotificationLeaveCommand command = new Commands.NotificationLeaveCommand(tokens, 0, screenName, options);

            return Core.CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// Disables notifications for updates from the specified user to the authenticating user. Returns the specified user when successful.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="screenName">The user's screen name.</param>
        /// <returns></returns>
        public static TwitterResponse<TwitterUser> Leave(OAuthTokens tokens, string screenName)
        {
            return Follow(tokens, screenName, null);
        }
    }
}
