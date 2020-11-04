//-----------------------------------------------------------------------
// <copyright file="TwitterList.cs" company="Patrick 'Ricky' Smith">
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
// <summary>The twitter list entity class</summary>
//-----------------------------------------------------------------------

namespace Twitterizer
{
    using System;
    using System.Diagnostics;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Twitterizer.Core;

    /// <summary>
    /// The twitter list entity class
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [DebuggerDisplay("TwitterList = {FullName}")]
#if !SILVERLIGHT
    [Serializable]
#endif
    [DataContract]
    public class TwitterList : TwitterObject
    {
        #region API properties
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The list id.</value>
        [JsonProperty(PropertyName = "id")]
        [DataMember]
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The list name.</value>
        [JsonProperty(PropertyName = "name")]
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the full name.
        /// </summary>
        /// <value>The full name.</value>
        [JsonProperty(PropertyName = "full_name")]
        [DataMember]
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the slug.
        /// </summary>
        /// <value>The list slug.</value>
        [JsonProperty(PropertyName = "slug")]
        [DataMember]
        public string Slug { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        [JsonProperty(PropertyName = "description")]
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the number of subscribers.
        /// </summary>
        /// <value>The number of subscribers.</value>
        [JsonProperty(PropertyName = "subscriber_count")]
        [DataMember]
        public int NumberOfSubscribers { get; set; }

        /// <summary>
        /// Gets or sets the number of members.
        /// </summary>
        /// <value>The number of members.</value>
        [JsonProperty(PropertyName = "member_count")]
        [DataMember]
        public int NumberOfMembers { get; set; }

        /// <summary>
        /// Gets or sets the absolute path.
        /// </summary>
        /// <value>The absolute path.</value>
        [JsonProperty(PropertyName = "uri")]
        [DataMember]
        public string AbsolutePath { get; set; }

        /// <summary>
        /// Gets or sets the mode.
        /// </summary>
        /// <value>The list mode.</value>
        [JsonProperty(PropertyName = "mode")]
        [DataMember]
        public string Mode { get; set; }

        /// <summary>
        /// Gets or sets the user that owns the list.
        /// </summary>
        /// <value>The owning user.</value>
        [JsonProperty(PropertyName = "user")]
        [DataMember]
        public TwitterUser User { get; set; }
        #endregion

        #region Calculated Properties
        /// <summary>
        /// Gets a value indicating whether this instance is public.
        /// </summary>
        /// <value><c>true</c> if this instance is public; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool IsPublic
        {
            get
            {
                return this.Mode == "public";
            }
        }
        #endregion

        /// <summary>
        /// Creates a new list for the authenticated user. Accounts are limited to 20 lists.
        /// </summary>
        /// <param name="tokens">The oauth tokens.</param>
        /// <param name="username">The username.</param>
        /// <param name="name">The list name.</param>
        /// <param name="isPublic">if set to <c>true</c> creates a public list.</param>
        /// <param name="description">The description.</param>
        /// <param name="options">The options.</param>
        /// <returns>A <see cref="TwitterList"/> instance.</returns>
        [Obsolete("The username parameter is no longer required.")]
        public static TwitterResponse<TwitterList> New(OAuthTokens tokens, string username, string name, bool isPublic, string description, OptionalProperties options)
        {
            return New(tokens, name, isPublic, description, options);
        }

        /// <summary>
        /// Creates a new list for the authenticated user. Accounts are limited to 20 lists.
        /// </summary>
        /// <param name="tokens">The oauth tokens.</param>
        /// <param name="name">The list name.</param>
        /// <param name="isPublic">if set to <c>true</c> creates a public list.</param>
        /// <param name="description">The description.</param>
        /// <param name="options">The options.</param>
        /// <returns>A <see cref="TwitterList"/> instance.</returns>
        public static TwitterResponse<TwitterList> New(OAuthTokens tokens, string name, bool isPublic, string description, OptionalProperties options)
        {
            Commands.CreateListCommand command = new Twitterizer.Commands.CreateListCommand(tokens, name, options)
            {
                IsPublic = isPublic,
                Description = description
            };

            return Core.CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// Creates a new list for the authenticated user. Accounts are limited to 20 lists.
        /// </summary>
        /// <param name="tokens">The oauth tokens.</param>
        /// <param name="username">The username.</param>
        /// <param name="name">The list name.</param>
        /// <param name="isPublic">if set to <c>true</c> creates a public list.</param>
        /// <param name="description">The description.</param>
        /// <returns>A <see cref="TwitterList"/> instance.</returns>
        /// <remarks></remarks>
        public static TwitterResponse<TwitterList> New(OAuthTokens tokens, string username, string name, bool isPublic, string description)
        {
            return New(tokens, name, isPublic, description, null);
        }

        /// <summary>
        /// Updates the specified list.
        /// </summary>
        /// <param name="tokens">The oauth tokens.</param>
        /// <param name="username">The username.</param>
        /// <param name="listId">The list id.</param>
        /// <param name="options">The options.</param>
        /// <returns>A <see cref="TwitterList"/> instance.</returns>
        [Obsolete("The username parameter is no longer required.")]
        public static TwitterResponse<TwitterList> Update(OAuthTokens tokens, string username, string listId, UpdateListOptions options)
        {
            return Update(tokens, listId, options);
        }

        /// <summary>
        /// Updates the specified list.
        /// </summary>
        /// <param name="tokens">The oauth tokens.</param>
        /// <param name="listId">The list id.</param>
        /// <param name="options">The options.</param>
        /// <returns>A <see cref="TwitterList"/> instance.</returns>
        /// <remarks></remarks>
        public static TwitterResponse<TwitterList> Update(OAuthTokens tokens, string listId, UpdateListOptions options)
        {
            Commands.UpdateListCommand command = new Twitterizer.Commands.UpdateListCommand(tokens, listId, options);

            return Core.CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// List the lists of the specified user. Private lists will be included if the authenticated users is the same as the user who's lists are being returned.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// A <see cref="TwitterListCollection"/> instance.
        /// </returns>
        public static TwitterResponse<TwitterListCollection> GetLists(OAuthTokens tokens, GetListsOptions options = null)
        {
            Commands.GetListsCommand command = new Twitterizer.Commands.GetListsCommand(tokens, options);

            return Core.CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// Returns the specified list. Private lists will only be shown if the authenticated user owns the specified list.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="slug">The slug.</param>
        /// <returns>A <see cref="TwitterList"/> instance.</returns>
        public static TwitterResponse<TwitterList> Show(OAuthTokens tokens, string slug)
        {
            return Show(tokens, slug, null);
        }

        /// <summary>
        /// Returns the specified list. Private lists will only be shown if the authenticated user owns the specified list.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="slug">The slug.</param>
        /// <param name="options">The options.</param>
        /// <returns>A <see cref="TwitterList"/> instance.</returns>
        public static TwitterResponse<TwitterList> Show(OAuthTokens tokens, string slug, OptionalProperties options)
        {
            Commands.GetListCommand command = new Twitterizer.Commands.GetListCommand(tokens, slug, -1, options);

            return Core.CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// Returns the specified list. Private lists will only be shown if the authenticated user owns the specified list.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="listId">The list id.</param>
        /// <returns>A <see cref="TwitterList"/> instance.</returns>
        public static TwitterResponse<TwitterList> Show(OAuthTokens tokens, decimal listId)
        {
            return Show(tokens, listId, null);
        }

        /// <summary>
        /// Returns the specified list. Private lists will only be shown if the authenticated user owns the specified list.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="listId">The list id.</param>
        /// <param name="options">The options.</param>
        /// <returns>A <see cref="TwitterList"/> instance.</returns>
        public static TwitterResponse<TwitterList> Show(OAuthTokens tokens, decimal listId, OptionalProperties options)
        {
            Commands.GetListCommand command = new Twitterizer.Commands.GetListCommand(tokens, string.Empty, listId, options);

            return Core.CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// Deletes the specified list. Must be owned by the authenticated user.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="username">The username.</param>
        /// <param name="listIdOrSlug">The list id or slug.</param>
        /// <param name="options">The options.</param>
        /// <returns>A <see cref="TwitterList"/> instance.</returns>
        public static TwitterResponse<TwitterList> Delete(OAuthTokens tokens, string username, string listIdOrSlug, OptionalProperties options)
        {
            Commands.DeleteListCommand command = new Twitterizer.Commands.DeleteListCommand(tokens, username, listIdOrSlug, options);

            return Core.CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// Show tweet timeline for members of the specified list.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="username">The username.</param>
        /// <param name="listIdOrSlug">The list id or slug.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// A <see cref="TwitterStatusCollection"/> instance.
        /// </returns>
        public static TwitterResponse<TwitterStatusCollection> GetStatuses(OAuthTokens tokens, string username, string listIdOrSlug, ListStatusesOptions options)
        {
            Commands.ListStatusesCommand command = new Twitterizer.Commands.ListStatusesCommand(tokens, username, listIdOrSlug, options);

            return Core.CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// List the lists the specified user has been added to.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="screenname">The screenname.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// A <see cref="TwitterListCollection"/> instance.
        /// </returns>
        public static TwitterResponse<TwitterListCollection> GetMemberships(OAuthTokens tokens, string screenname, ListMembershipsOptions options)
        {
            Commands.ListMembershipsCommand command = new Twitterizer.Commands.ListMembershipsCommand(tokens, screenname, options);
            return Core.CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// List the lists the specified user has been added to.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="screenname">The screenname.</param>
        /// <returns>
        /// A <see cref="TwitterListCollection"/> instance.
        /// </returns>
        public static TwitterResponse<TwitterListCollection> GetMemberships(OAuthTokens tokens, string screenname)
        {
            return GetMemberships(tokens, screenname, null);
        }

        /// <summary>
        /// List the lists the specified user has been added to.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="userid">The userid.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// A <see cref="TwitterListCollection"/> instance.
        /// </returns>
        public static TwitterResponse<TwitterListCollection> GetMemberships(OAuthTokens tokens, decimal userid, ListMembershipsOptions options)
        {
            Commands.ListMembershipsCommand command = new Twitterizer.Commands.ListMembershipsCommand(tokens, userid, options);
            return Core.CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// List the lists the specified user has been added to.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="userid">The userid.</param>
        /// <returns>
        /// A <see cref="TwitterListCollection"/> instance.
        /// </returns>
        public static TwitterResponse<TwitterListCollection> GetMemberships(OAuthTokens tokens, decimal userid)
        {
            return GetMemberships(tokens, userid, null);
        }

        /// <summary>
        /// List the lists the specified user follows.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// A <see cref="TwitterListCollection"/> instance.
        /// </returns>
        public static TwitterResponse<TwitterListCollection> GetSubscriptions(OAuthTokens tokens, string userName, GetListSubscriptionsOptions options)
        {
            Commands.GetListSubscriptionsCommand command = new Twitterizer.Commands.GetListSubscriptionsCommand(tokens, userName, options);

            return Core.CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// List the lists the specified user follows.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="userName">Name of the user.</param>
        /// <returns>
        /// A <see cref="TwitterListCollection"/> instance.
        /// </returns>
        public static TwitterResponse<TwitterListCollection> GetSubscriptions(OAuthTokens tokens, string userName)
        {
            return GetSubscriptions(tokens, userName, null);
        }


        /// <summary>
        /// Returns the members of the specified list.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="username">The username.</param>
        /// <param name="listIdOrSlug">The list id or slug.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// A collection of users as <see cref="TwitterUserCollection"/>.
        /// </returns>
        public static TwitterResponse<TwitterUserCollection> GetMembers(OAuthTokens tokens, string username, string listIdOrSlug, GetListMembersOptions options)
        {
            Commands.GetListMembersCommand command = new Twitterizer.Commands.GetListMembersCommand(tokens, username, listIdOrSlug, options);

            return CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// Returns the members of the specified list.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="username">The username.</param>
        /// <param name="listIdOrSlug">The list id or slug.</param>
        /// <returns>A collection of users as <see cref="TwitterUserCollection"/>.</returns>
        /// <remarks></remarks>
        public static TwitterResponse<TwitterUserCollection> GetMembers(OAuthTokens tokens, string username, string listIdOrSlug)
        {
            return GetMembers(tokens, username, listIdOrSlug, null);
        }

        /// <summary>
        /// Add a member to a list. The authenticated user must own the list to be able to add members to it. Lists are limited to having 500 members.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="ownerUsername">The username of the list owner.</param>
        /// <param name="listId">The list id.</param>
        /// <param name="userIdToAdd">The user id to add.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// A <see cref="TwitterList"/> representing the list the user was added to, or <c>null</c>.
        /// </returns>
        public static TwitterResponse<TwitterList> AddMember(OAuthTokens tokens, string ownerUsername, string listId, decimal userIdToAdd, OptionalProperties options)
        {
            Commands.AddListMemberCommand command = new Twitterizer.Commands.AddListMemberCommand(tokens, ownerUsername, listId, userIdToAdd, options);

            return CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// Add a member to a list. The authenticated user must own the list to be able to add members to it. Lists are limited to having 500 members.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="ownerUsername">The username of the list owner.</param>
        /// <param name="listId">The list id.</param>
        /// <param name="userIdToAdd">The user id to add.</param>
        /// <returns>
        /// A <see cref="TwitterList"/> representing the list the user was added to, or <c>null</c>.
        /// </returns>
        public static TwitterResponse<TwitterList> AddMember(OAuthTokens tokens, string ownerUsername, string listId, decimal userIdToAdd)
        {
            return AddMember(tokens, ownerUsername, listId, userIdToAdd, null);
        }

        /// <summary>
        /// Removes the specified member from the list. The authenticated user must be the list's owner to remove members from the list.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="ownerUsername">The username of the list owner.</param>
        /// <param name="listId">The list id.</param>
        /// <param name="userIdToAdd">The user id to add.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// A <see cref="TwitterList"/> representing the list the user was added to, or <c>null</c>.
        /// </returns>
        public static TwitterResponse<TwitterList> RemoveMember(OAuthTokens tokens, string ownerUsername, string listId, decimal userIdToAdd, OptionalProperties options)
        {
            Commands.RemoveListMemberCommand command = new Twitterizer.Commands.RemoveListMemberCommand(tokens, ownerUsername, listId, userIdToAdd, options);

            return CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// Removes the specified member from the list. The authenticated user must be the list's owner to remove members from the list.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="ownerUsername">The username of the list owner.</param>
        /// <param name="listId">The list id.</param>
        /// <param name="userIdToAdd">The user id to add.</param>
        /// <returns>
        /// A <see cref="TwitterList"/> representing the list the user was added to, or <c>null</c>.
        /// </returns>
        public static TwitterResponse<TwitterList> RemoveMember(OAuthTokens tokens, string ownerUsername, string listId, decimal userIdToAdd)
        {
            return RemoveMember(tokens, ownerUsername, listId, userIdToAdd, null);
        }

        /// <summary>
        /// Check if a user is a member of the specified list.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="ownerUsername">The username of the list owner.</param>
        /// <param name="listId">The list id.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// The user's details, if they are a member of the list, otherwise <c>null</c>.
        /// </returns>
        public static TwitterResponse<TwitterUser> CheckMembership(OAuthTokens tokens, string ownerUsername, string listId, decimal userId, OptionalProperties options)
        {
            Commands.CheckListMembershipCommand command = new Twitterizer.Commands.CheckListMembershipCommand(
                tokens,
                ownerUsername,
                listId,
                userId,
                options);

            return CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// Check if a user is a member of the specified list.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="ownerUsername">The username of the list owner.</param>
        /// <param name="listId">The list id.</param>
        /// <param name="userId">The user id.</param>
        /// <returns>
        /// The user's details, if they are a member of the list, otherwise <c>null</c>.
        /// </returns>
        public static TwitterResponse<TwitterUser> CheckMembership(OAuthTokens tokens, string ownerUsername, string listId, decimal userId)
        {
            return CheckMembership(tokens, ownerUsername, listId, userId, null);
        }

        /// <summary>
        /// Subscribes the specified tokens.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="listId">The list id.</param>
        /// <returns></returns>
        public static TwitterResponse<TwitterList> Subscribe(OAuthTokens tokens, decimal listId)
        {
            return Subscribe(tokens, listId, null);
        }

        /// <summary>
        /// Subscribes the specified tokens.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="listId">The list id.</param>
        /// <param name="optionalProperties">The optional properties.</param>
        /// <returns></returns>
        public static TwitterResponse<TwitterList> Subscribe(OAuthTokens tokens, decimal listId, OptionalProperties optionalProperties)
        {
            Commands.CreateListMembershipCommand command = new Commands.CreateListMembershipCommand(tokens, listId, optionalProperties);

            return CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// Unsubscribes the authenticated user from the specified list.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="listId">The list id.</param>
        /// <param name="optionalProperties">The optional properties.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static TwitterResponse<TwitterList> UnSubscribe(OAuthTokens tokens, decimal listId, OptionalProperties optionalProperties)
        {
            Commands.DestroyListSubscriber command = new Commands.DestroyListSubscriber(tokens, listId, optionalProperties);

            return CommandPerformer.PerformAction(command);
        }
    }
}
