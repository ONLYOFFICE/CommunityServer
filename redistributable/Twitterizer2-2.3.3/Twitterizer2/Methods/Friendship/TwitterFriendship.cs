//-----------------------------------------------------------------------
// <copyright file="TwitterFriendship.cs" company="Patrick 'Ricky' Smith">
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
// <summary>The TwitterFriendship class.</summary>
//-----------------------------------------------------------------------

namespace Twitterizer
{
    using Core;

    /// <summary>
    /// Provides interaction with the Twitter API to obtain and manage relationships between users.
    /// </summary>
    public static class TwitterFriendship
    {
        #region Followers
        /// <summary>
        /// Returns the authenticating user's followers, each with current status inline.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// A <see cref="TwitterStatusCollection"/> instance.
        /// </returns>
        public static TwitterResponse<TwitterUserCollection> Followers(OAuthTokens tokens, FollowersOptions options)
        {
            Commands.FollowersCommand command = new Commands.FollowersCommand(tokens, options);

            return CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// Returns the authenticating user's followers, each with current status inline.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <returns>
        /// A <see cref="TwitterStatusCollection"/> instance.
        /// </returns>
        public static TwitterResponse<TwitterUserCollection> Followers(OAuthTokens tokens)
        {
            return Followers(tokens, null);
        }

        /// <summary>
        /// Returns the authenticating user's followers, each with current status inline.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>
        /// A <see cref="TwitterStatusCollection"/> instance.
        /// </returns>
        public static TwitterResponse<TwitterUserCollection> Followers(FollowersOptions options)
        {
            return Followers(null, options);
        }
        #endregion

        #region Friends

        /// <summary>
        /// Returns a user's friends, each with current status inline. They are ordered by the order in which the user followed them, most recently followed first, 100 at a time.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// A <see cref="TwitterUserCollection"/> instance.
        /// </returns>
        /// <remarks>Please note that the result set isn't guaranteed to be 100 every time as suspended users will be filtered out.</remarks>
        [System.Obsolete("This method is deprecated as it will only return information about users who have Tweeted recently. It is not a functional way to retrieve all of a users friends. Instead of using this method use a combination of friends/ids and users/lookup.")]
        public static TwitterResponse<TwitterUserCollection> Friends(OAuthTokens tokens, FriendsOptions options)
        {
            Commands.FriendsCommand command = new Commands.FriendsCommand(tokens, options);

            return CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// Returns a user's friends, each with current status inline. They are ordered by the order in which the user followed them, most recently followed first, 100 at a time.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <returns>
        /// A <see cref="TwitterUserCollection"/> instance.
        /// </returns>
        /// <remarks>Please note that the result set isn't guaranteed to be 100 every time as suspended users will be filtered out.</remarks>
        [System.Obsolete("This method is deprecated as it will only return information about users who have Tweeted recently. It is not a functional way to retrieve all of a users friends. Instead of using this method use a combination of friends/ids and users/lookup.")]
        public static TwitterResponse<TwitterUserCollection> Friends(OAuthTokens tokens)
        {
            return Friends(tokens, null);
        }

        /// <summary>
        /// Returns a user's friends, each with current status inline. They are ordered by the order in which the user followed them, most recently followed first, 100 at a time.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>
        /// A <see cref="TwitterUserCollection"/> instance.
        /// </returns>
        /// <remarks>Please note that the result set isn't guaranteed to be 100 every time as suspended users will be filtered out.</remarks>
        [System.Obsolete("This method is deprecated as it will only return information about users who have Tweeted recently. It is not a functional way to retrieve all of a users friends. Instead of using this method use a combination of friends/ids and users/lookup.")]
        public static TwitterResponse<TwitterUserCollection> Friends(FriendsOptions options)
        {
            return Friends(null, options);
        }
        #endregion

        #region Create Friendship

        /// <summary>
        /// Allows the authenticating users to follow the user specified in the userID parameter.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="userId">The user id.</param>        
        /// <returns>
        /// Returns the followed user in the requested format when successful.
        /// </returns>
        public static TwitterResponse<TwitterUser> Create(OAuthTokens tokens, decimal userId)
        {
            return Create(tokens, userId, null);
        }

        /// <summary>
        /// Allows the authenticating users to follow the user specified in the userID parameter.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// Returns the followed user in the requested format when successful.
        /// </returns>
        public static TwitterResponse<TwitterUser> Create(OAuthTokens tokens, decimal userId, CreateFriendshipOptions options)
        {
            Commands.CreateFriendshipCommand command = new Commands.CreateFriendshipCommand(tokens, userId, options);
            return CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// Allows the authenticating users to follow the user specified in the userName parameter.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="userName">The user name.</param>
        /// <returns>
        /// Returns the followed user in the requested format when successful.
        /// </returns>
        public static TwitterResponse<TwitterUser> Create(OAuthTokens tokens, string userName)
        {
            return Create(tokens, userName, null);
        }

        /// <summary>
        /// Allows the authenticating users to follow the user specified in the userName parameter.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="userName">The user name.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// Returns the followed user in the requested format when successful.
        /// </returns>
        public static TwitterResponse<TwitterUser> Create(OAuthTokens tokens, string userName, CreateFriendshipOptions options)
        {
            Commands.CreateFriendshipCommand command = new Commands.CreateFriendshipCommand(tokens, userName, options);
            return CommandPerformer.PerformAction(command);
        }

        #endregion

        #region Delete Friendship

        /// <summary>
        /// Allows the authenticating users to unfollow the user specified in the ID parameter.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="userId">The user id.</param>
        /// <returns>
        /// Returns the unfollowed user in the requested format when successful.
        /// </returns>
        public static TwitterResponse<TwitterUser> Delete(OAuthTokens tokens, decimal userId)
        {
           return Delete(tokens, userId, null);
        }

        /// <summary>
        /// Allows the authenticating users to unfollow the user specified in the ID parameter.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// Returns the unfollowed user in the requested format when successful.
        /// </returns>
        public static TwitterResponse<TwitterUser> Delete(OAuthTokens tokens, decimal userId, OptionalProperties options)
        {
            Commands.DeleteFriendshipCommand command = new Commands.DeleteFriendshipCommand(tokens, userId, string.Empty, options);
            return CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// Allows the authenticating users to unfollow the user specified in the ID parameter.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="userName">The username.</param>
        /// <returns>
        /// Returns the unfollowed user in the requested format when successful.
        /// </returns>
        public static TwitterResponse<TwitterUser> Delete(OAuthTokens tokens, string userName)
        {
            return Delete(tokens, userName, null);
        }

        /// <summary>
        /// Allows the authenticating users to unfollow the user specified in the ID parameter.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="userName">The username.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// Returns the unfollowed user in the requested format when successful.
        /// </returns>
        public static TwitterResponse<TwitterUser> Delete(OAuthTokens tokens, string userName, OptionalProperties options)
        {
            Commands.DeleteFriendshipCommand command = new Commands.DeleteFriendshipCommand(tokens, 0, userName, options);
            return Core.CommandPerformer.PerformAction(command);
        }
        #endregion

        #region Show Friendship

        /// <summary>
        /// Returns detailed information about the relationship between two users.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="targetUserId">The target user id.</param>
        /// <returns>A <see cref="TwitterRelationship"/> instance.</returns>
        public static TwitterResponse<TwitterRelationship> Show(OAuthTokens tokens, decimal targetUserId)
        {
            return Show(tokens, targetUserId, null);
        }

        /// <summary>
        /// Returns detailed information about the relationship between two users.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="targetUserId">The target user id.</param>
        /// <param name="options">The options.</param>
        /// <returns>A <see cref="TwitterRelationship"/> instance.</returns>
        public static TwitterResponse<TwitterRelationship> Show(OAuthTokens tokens, decimal targetUserId, OptionalProperties options)
        {
            return Show(tokens, 0, targetUserId, options);
        }

        /// <summary>
        /// Returns detailed information about the relationship between two users.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="sourceUseId">The source user id.</param>
        /// <param name="targetUserId">The target user id.</param>
        /// <returns>A <see cref="TwitterRelationship"/> instance.</returns>
        public static TwitterResponse<TwitterRelationship> Show(OAuthTokens tokens, decimal sourceUseId, decimal targetUserId)
        {
            return Show(tokens, sourceUseId, targetUserId, null);
        }

        /// <summary>
        /// Returns detailed information about the relationship between two users.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="sourceUseId">The source user id.</param>
        /// <param name="targetUserId">The target user id.</param>
        /// <param name="options">The options.</param>
        /// <returns>A <see cref="TwitterRelationship"/> instance.</returns>
        public static TwitterResponse<TwitterRelationship> Show(OAuthTokens tokens, decimal sourceUseId, decimal targetUserId, OptionalProperties options)
        {
            Commands.ShowFriendshipCommand command = new Twitterizer.Commands.ShowFriendshipCommand(
                tokens, 
                sourceUseId, 
                string.Empty, 
                targetUserId, 
                string.Empty, 
                options);

            return Core.CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// Returns detailed information about the relationship between two users.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="targetUserName">The target user name.</param>
        /// <returns>A <see cref="TwitterRelationship"/> instance.</returns>
        public static TwitterResponse<TwitterRelationship> Show(OAuthTokens tokens, string targetUserName)
        {
            return Show(tokens, string.Empty, targetUserName, null);
        }

        /// <summary>
        /// Returns detailed information about the relationship between two users.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="targetUserName">The target user name.</param>
        /// <param name="options">The options.</param>
        /// <returns>A <see cref="TwitterRelationship"/> instance.</returns>
        public static TwitterResponse<TwitterRelationship> Show(OAuthTokens tokens, string targetUserName, OptionalProperties options)
        {
            return Show(tokens, string.Empty, targetUserName, options);
        }

        /// <summary>
        /// Returns detailed information about the relationship between two users.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="sourceUserName">The source user name.</param>
        /// <param name="targetUserName">The target user name.</param>
        /// <returns>A <see cref="TwitterRelationship"/> instance.</returns>
        public static TwitterResponse<TwitterRelationship> Show(OAuthTokens tokens, string sourceUserName, string targetUserName)
        {
            return Show(tokens, sourceUserName, targetUserName, null);
        }

        /// <summary>
        /// Returns detailed information about the relationship between two users.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="sourceUserName">The source user name.</param>
        /// <param name="targetUserName">The target user name.</param>
        /// <param name="options">The options.</param>
        /// <returns>A <see cref="TwitterRelationship"/> instance.</returns>
        public static TwitterResponse<TwitterRelationship> Show(OAuthTokens tokens, string sourceUserName, string targetUserName, OptionalProperties options)
        {
            Commands.ShowFriendshipCommand command = new Twitterizer.Commands.ShowFriendshipCommand(tokens, 0, sourceUserName, 0, targetUserName, options);

            return Core.CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// Returns detailed information about the relationship between two users.
        /// </summary>
        /// <param name="sourceUseId">The first user id.</param>
        /// <param name="targetUserId">The second user id.</param>
        /// <returns>
        /// A <see cref="TwitterRelationship"/> instance.
        /// </returns>
        public static TwitterResponse<TwitterRelationship> Show(decimal sourceUseId, decimal targetUserId)
        {
            return Show(null, sourceUseId, targetUserId, null);
        }

        /// <summary>
        /// Returns detailed information about the relationship between two users.
        /// </summary>
        /// <param name="sourceUserName">The first username.</param>
        /// <param name="targetUserName">The second username.</param>
        /// <returns>
        /// A <see cref="TwitterRelationship"/> instance.
        /// </returns>
        public static TwitterResponse<TwitterRelationship> Show(string sourceUserName, string targetUserName)
        {
            return Show(null, sourceUserName, targetUserName, null);
        }

        #endregion

        #region User IDs lists
        
        /// <summary>
        /// Returns the numeric IDs for every user the specified user is friends with.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// A <see cref="TwitterListCollection"/> instance.
        /// </returns>
        public static TwitterResponse<UserIdCollection> FriendsIds(OAuthTokens tokens, UsersIdsOptions options)
        {
            Commands.FriendsIdsCommand command = new Commands.FriendsIdsCommand(tokens, options);
            return Core.CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// Returns the numeric IDs for every user the specified user is friends with.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <returns>
        /// A <see cref="TwitterListCollection"/> instance.
        /// </returns>
        public static TwitterResponse<UserIdCollection> FriendsIds(OAuthTokens tokens)
        {
            return FriendsIds(tokens, null);
        }

        /// <summary>
        /// Returns the numeric IDs for every user the specified user is following.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// A <see cref="TwitterListCollection"/> instance.
        /// </returns>
        public static TwitterResponse<UserIdCollection> FollowersIds(OAuthTokens tokens, UsersIdsOptions options)
        {
            Commands.FollowersIdsCommand command = new Commands.FollowersIdsCommand(tokens, options);
            return Core.CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// Returns the numeric IDs for every user the specified user is following.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <returns>
        /// A <see cref="TwitterListCollection"/> instance.
        /// </returns>
        public static TwitterResponse<UserIdCollection> FollowersIds(OAuthTokens tokens)
        {
            return FollowersIds(tokens, null);
        }

        #endregion

        /// <summary>
        /// Returns a collection of IDs for every user who has a pending request to follow the authenticating user.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public static TwitterResponse<TwitterCursorPagedIdCollection> IncomingRequests(OAuthTokens tokens, IncomingFriendshipsOptions options)
        {
            Commands.IncomingFriendshipsCommand command = new Commands.IncomingFriendshipsCommand(tokens, options);
            return Core.CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// Returns a collection of IDs for every user who has a pending request to follow the authenticating user.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <returns></returns>
        public static TwitterResponse<TwitterCursorPagedIdCollection> IncomingRequests(OAuthTokens tokens)
        {
            return IncomingRequests(tokens, null);
        }

        /// <summary>
        /// Returns a collection of IDs for every protected user for whom the authenticating user has a pending follow request.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public static TwitterResponse<TwitterCursorPagedIdCollection> OutgoingRequests(OAuthTokens tokens, OutgoingFriendshipsOptions options)
        {
            Commands.OutgoingFriendshipsCommand command = new Commands.OutgoingFriendshipsCommand(tokens, options);
            return Core.CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// Returns a collection of IDs for every protected user for whom the authenticating user has a pending follow request.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <returns></returns>
        public static TwitterResponse<TwitterCursorPagedIdCollection> OutgoingRequests(OAuthTokens tokens)
        {
            return OutgoingRequests(tokens, null);
        }

        /// <summary>
        /// Returns a collection of IDs that the user does not want to see retweets from.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public static TwitterResponse<UserIdCollection> NoRetweetIDs(OAuthTokens tokens, OptionalProperties options)
        {
            Commands.NoRetweetIDsCommand command = new Commands.NoRetweetIDsCommand(tokens, options);
            return Core.CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// Returns a collection of IDs that the user does not want to see retweets from.
        /// </summary>
        /// <param name="tokens">The tokens.</param>        
        /// <returns></returns>
        public static TwitterResponse<UserIdCollection> NoRetweetIDs(OAuthTokens tokens)
        {
            return NoRetweetIDs(tokens, null);
        }

        /// <summary>
        /// Updates a friendship for a user.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="userid">The userid.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public static TwitterResponse<TwitterRelationship> Update(OAuthTokens tokens, decimal userid, UpdateFriendshipOptions options)
        {
            Commands.UpdateFriendshipCommand command = new Commands.UpdateFriendshipCommand(tokens, userid, options);
            return Core.CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// Updates a friendship for a user.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="userid">The userid.</param>
        /// <returns></returns>
        public static TwitterResponse<TwitterRelationship> Update(OAuthTokens tokens, decimal userid)
        {
            return Update(tokens, userid, null);
        }

        /// <summary>
        /// Updates a friendship for a user.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="screenname">The screenname.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public static TwitterResponse<TwitterRelationship> Update(OAuthTokens tokens, string screenname, UpdateFriendshipOptions options)
        {
            Commands.UpdateFriendshipCommand command = new Commands.UpdateFriendshipCommand(tokens, screenname, options);
            return Core.CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// Updates a friendship for a user.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="screenname">The screenname.</param>
        /// <returns></returns>
        public static TwitterResponse<TwitterRelationship> Update(OAuthTokens tokens, string screenname)
        {
            return Update(tokens, screenname, null);
        }

    }
}
