//-----------------------------------------------------------------------
// <copyright file="TwitterTimeline.cs" company="Patrick 'Ricky' Smith">
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
// <summary>The TwitterTimeline class</summary>
//-----------------------------------------------------------------------

namespace Twitterizer
{
    using Twitterizer.Core;

    /// <summary>
    /// Provides interaction with timelines
    /// </summary>
    public static class TwitterTimeline
    {
        /// <overloads>
        /// Returns the 20 most recent statuses, including retweets, posted by the authenticating user and that user's friends. This is the equivalent of /timeline/home on the Web.
        /// </overloads>
        /// <param name="tokens">The tokens.</param>
        /// <param name="options">The options.</param>
        /// <returns>A collection of <see cref="TwitterStatus"/> items.</returns>
        public static TwitterResponse<TwitterStatusCollection> HomeTimeline(OAuthTokens tokens, TimelineOptions options)
        {
            Commands.HomeTimelineCommand command = new Commands.HomeTimelineCommand(tokens, options);

            return Core.CommandPerformer.PerformAction(command);
        }

        /// <param name="tokens">The tokens.</param>
        /// <returns>A collection of <see cref="TwitterStatus"/> items.</returns>
        public static TwitterResponse<TwitterStatusCollection> HomeTimeline(OAuthTokens tokens)
        {
            return HomeTimeline(tokens, null);
        }

        /// <param name="options">The options.</param>
        /// <returns>A collection of <see cref="TwitterStatus"/> items.</returns>
        public static TwitterResponse<TwitterStatusCollection> HomeTimeline(TimelineOptions options)
        {
            return HomeTimeline(null, options);
        }

        /// <summary>
        /// Returns the 20 most recent statuses posted by the authenticating user. It is also possible to request another user's timeline by using the screen_name or user_id parameter.
        /// </summary>
        /// <param name="tokens">The oauth tokens.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// A <see cref="TwitterStatusCollection"/> instance.
        /// </returns>
        public static TwitterResponse<TwitterStatusCollection> UserTimeline(
            OAuthTokens tokens,
            UserTimelineOptions options)
        {
            Commands.UserTimelineCommand command = new Commands.UserTimelineCommand(tokens, options);

            return Core.CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// Returns the 20 most recent statuses posted by the authenticating user. It is also possible to request another user's timeline by using the screen_name or user_id parameter.
        /// </summary>
        /// <param name="tokens">The oauth tokens.</param>
        /// <returns>
        /// A <see cref="TwitterStatusCollection"/> instance.
        /// </returns>
        public static TwitterResponse<TwitterStatusCollection> UserTimeline(
            OAuthTokens tokens)
        {
            return UserTimeline(tokens, null);
        }

        /// <summary>
        /// Returns the 20 most recent statuses posted by the authenticating user. It is also possible to request another user's timeline by using the screen_name or user_id parameter.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>
        /// A <see cref="TwitterStatusCollection"/> instance.
        /// </returns>
        public static TwitterResponse<TwitterStatusCollection> UserTimeline(
            UserTimelineOptions options)
        {
            return UserTimeline(null, options);
        }

        /// <summary>
        /// Gets the public timeline.
        /// </summary>
        /// <returns>A <see cref="TwitterStatusCollection"/>.</returns>
        public static TwitterResponse<TwitterStatusCollection> PublicTimeline()
        {
            return PublicTimeline((OAuthTokens)null);
        }

        /// <summary>
        /// Returns the 20 most recent statuses, including retweets if they exist, from non-protected users. The public timeline is cached for 60 seconds.
        /// </summary>
        /// <param name="tokens">The oauth tokens.</param>
        /// <returns>
        /// A <see cref="TwitterStatusCollection"/>.
        /// </returns>
        public static TwitterResponse<TwitterStatusCollection> PublicTimeline(OAuthTokens tokens)
        {
            return PublicTimeline(tokens, null);
        }

        /// <summary>
        /// Returns the 20 most recent statuses, including retweets if they exist, from non-protected users. The public timeline is cached for 60 seconds.
        /// </summary>
        /// <param name="options">The properties.</param>
        /// <returns>
        /// A <see cref="TwitterStatusCollection"/>.
        /// </returns>
        public static TwitterResponse<TwitterStatusCollection> PublicTimeline(OptionalProperties options)
        {
            return PublicTimeline(null, options);
        }

        /// <summary>
        /// Returns the 20 most recent statuses, including retweets if they exist, from non-protected users. The public timeline is cached for 60 seconds.
        /// </summary>
        /// <param name="tokens">The oauth tokens.</param>
        /// <param name="options">The options.</param>
        /// <returns>A <see cref="TwitterStatusCollection"/>.</returns>
        /// <remarks></remarks>
        public static TwitterResponse<TwitterStatusCollection> PublicTimeline(OAuthTokens tokens, OptionalProperties options)
        {
            Commands.PublicTimelineCommand command = new Commands.PublicTimelineCommand(tokens, options);
            TwitterResponse<TwitterStatusCollection> result = CommandPerformer.PerformAction(command);

            return result;
        }

        /// <summary>
        /// Obtains the authorized user's friends timeline.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <returns>A <see cref="TwitterStatusCollection"/>.</returns>
        [System.Obsolete("This method is deprecated and has been replaced by the HomeTimeline method.")]
        public static TwitterResponse<TwitterStatusCollection> FriendTimeline(OAuthTokens tokens)
        {
            return FriendTimeline(tokens, null);
        }

        /// <summary>
        /// Obtains the authorized user's friends timeline.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="options">The options.</param>
        /// <returns>A <see cref="TwitterStatusCollection"/>.</returns>
        [System.Obsolete("This method is deprecated and has been replaced by the HomeTimeline method.")]
        public static TwitterResponse<TwitterStatusCollection> FriendTimeline(OAuthTokens tokens, TimelineOptions options)
        {
            Commands.FriendsTimelineCommand command = new Commands.FriendsTimelineCommand(tokens, options);

            return CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// Returns the 20 most recent tweets of the authenticated user that have been retweeted by others.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="options">The options.</param>
        /// <returns>A <see cref="TwitterStatusCollection"/> instance.</returns>
        public static TwitterResponse<TwitterStatusCollection> RetweetsOfMe(OAuthTokens tokens, RetweetsOfMeOptions options)
        {
            return CommandPerformer.PerformAction(
                new Commands.RetweetsOfMeCommand(tokens, options));
        }

        /// <summary>
        /// Returns the 20 most recent tweets of the authenticated user that have been retweeted by others.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <returns>
        /// A <see cref="TwitterStatusCollection"/> instance.
        /// </returns>
        public static TwitterResponse<TwitterStatusCollection> RetweetsOfMe(OAuthTokens tokens)
        {
            return RetweetsOfMe(tokens, null);
        }

        /// <summary>
        /// Returns the 20 most recent retweets posted by the authenticating user.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="options">The options.</param>
        /// <returns>A <see cref="TwitterStatusCollection"/> instance.</returns>
        public static TwitterResponse<TwitterStatusCollection> RetweetedByMe(OAuthTokens tokens, TimelineOptions options)
        {
            return CommandPerformer.PerformAction(
                new Commands.RetweetedByMeCommand(tokens, options));
        }

        /// <summary>
        /// Returns the 20 most recent retweets posted by the authenticating user.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <returns>
        /// A <see cref="TwitterStatusCollection"/> instance.
        /// </returns>
        public static TwitterResponse<TwitterStatusCollection> RetweetedByMe(OAuthTokens tokens)
        {
            return RetweetedByMe(tokens, null);
        }

        /// <summary>
        /// Returns the 20 most recent retweets posted by the authenticating user's friends.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="options">The options.</param>
        /// <returns>A <see cref="TwitterStatusCollection"/> instance.</returns>
        public static TwitterResponse<TwitterStatusCollection> RetweetedToMe(OAuthTokens tokens, TimelineOptions options)
        {
            return CommandPerformer.PerformAction(
                new Commands.RetweetedToMeCommand(tokens, options));
        }

        /// <summary>
        /// Returns the 20 most recent retweets posted by the authenticating user's friends.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <returns>
        /// A <see cref="TwitterStatusCollection"/> instance.
        /// </returns>
        public static TwitterResponse<TwitterStatusCollection> RetweetedToMe(OAuthTokens tokens)
        {
            return RetweetedToMe(tokens, null);
        }

        /// <summary>
        /// Returns the 20 most recent mentions (status containing @username) for the authenticating user.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="options">The options.</param>
        /// <returns>A <see cref="TwitterStatusCollection"/> instance.</returns>
        public static TwitterResponse<TwitterStatusCollection> Mentions(OAuthTokens tokens, TimelineOptions options)
        {
            Commands.MentionsCommand command = new Commands.MentionsCommand(tokens, options);
            return CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// Returns the 20 most recent mentions (status containing @username) for the authenticating user.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <returns>
        /// A <see cref="TwitterStatusCollection"/> instance.
        /// </returns>
        public static TwitterResponse<TwitterStatusCollection> Mentions(OAuthTokens tokens)
        {
            return Mentions(tokens, null);
        }
    }
}
