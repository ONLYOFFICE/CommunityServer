//-----------------------------------------------------------------------
// <copyright file="TwitterDirectMessage.cs" company="Patrick 'Ricky' Smith">
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
// <summary>The direct message entity class</summary>
//-----------------------------------------------------------------------

namespace Twitterizer
{
    using System;
    using Newtonsoft.Json;
    using Twitterizer.Core;

    /// <summary>
    /// The Direct Message Entity Class
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
#if !SILVERLIGHT
    [Serializable]
#endif
    public class TwitterDirectMessage : TwitterObject
    {
        #region Properties
        /// <summary>
        /// Gets or sets the direct message id.
        /// </summary>
        /// <value>The direct message id.</value>
        [JsonProperty(PropertyName = "id")]
        public decimal Id { get; set; }

        /// <summary>
        /// Gets or sets the sender id.
        /// </summary>
        /// <value>The sender id.</value>
        [JsonProperty(PropertyName = "sender_id")]
        public decimal SenderId { get; set; }

        /// <summary>
        /// Gets or sets the direct message text.
        /// </summary>
        /// <value>The direct message text.</value>
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the recipient id.
        /// </summary>
        /// <value>The recipient id.</value>
        [JsonProperty(PropertyName = "recipient_id")]
        public decimal RecipientId { get; set; }

        /// <summary>
        /// Gets or sets the created date.
        /// </summary>
        /// <value>The created date.</value>
        [JsonProperty(PropertyName = "created_at")]
        [JsonConverter(typeof(TwitterizerDateConverter))]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Gets or sets the name of the sender screen.
        /// </summary>
        /// <value>The name of the sender screen.</value>
        [JsonProperty(PropertyName = "sender_screen_name")]
        public string SenderScreenName { get; set; }

        /// <summary>
        /// Gets or sets the name of the recipient screen.
        /// </summary>
        /// <value>The name of the recipient screen.</value>
        [JsonProperty(PropertyName = "recipient_screen_name")]
        public string RecipientScreenName { get; set; }

        /// <summary>
        /// Gets or sets the sender.
        /// </summary>
        /// <value>The sender.</value>
        [JsonProperty(PropertyName = "sender")]
        public TwitterUser Sender { get; set; }

        /// <summary>
        /// Gets or sets the recipient.
        /// </summary>
        /// <value>The recipient.</value>
        [JsonProperty(PropertyName = "recipient")]
        public TwitterUser Recipient { get; set; }

        /// <summary>
        /// Gets or sets the entities.
        /// </summary>
        /// <value>The entities.</value>
        [JsonProperty(PropertyName = "entities")]
        [JsonConverter(typeof(Entities.TwitterEntityCollection.Converter))]
        public Entities.TwitterEntityCollection Entities { get; set; }
        #endregion

        /// <summary>
        /// Returns a list of the 20 most recent direct messages sent to the authenticating user.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <returns>A <see cref="TwitterDirectMessageCollection"/> instance.</returns>
        public static TwitterResponse<TwitterDirectMessageCollection> DirectMessages(OAuthTokens tokens)
        {
            return DirectMessages(tokens, null);
        }

        /// <summary>
        /// Returns a list of the 20 most recent direct messages sent to the authenticating user.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// A <see cref="TwitterDirectMessageCollection"/> instance.
        /// </returns>
        public static TwitterResponse<TwitterDirectMessageCollection> DirectMessages(OAuthTokens tokens, DirectMessagesOptions options)
        {
            return CommandPerformer.PerformAction(new Commands.DirectMessagesCommand(tokens, options));
        }

        /// <summary>
        /// Returns a list of the 20 most recent direct messages sent by the authenticating user.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <returns>
        /// A <see cref="TwitterDirectMessageCollection"/> instance.
        /// </returns>
        public static TwitterResponse<TwitterDirectMessageCollection> DirectMessagesSent(OAuthTokens tokens)
        {
            return DirectMessagesSent(tokens, null);
        }

        /// <summary>
        /// Sends a new direct message to the specified user from the authenticating user.
        /// </summary>
        /// <param name="tokens">The OAuth tokens.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="text">The text of your direct message.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// A <see cref="TwitterDirectMessage"/> instance.
        /// </returns>
        public static TwitterResponse<TwitterDirectMessage> Send(OAuthTokens tokens, decimal userId, string text, OptionalProperties options)
        {
            Commands.SendDirectMessageCommand command = new Commands.SendDirectMessageCommand(tokens, text, userId, options);

            TwitterResponse<TwitterDirectMessage> result = Core.CommandPerformer.PerformAction(command);

            return result;
        }

        /// <summary>
        /// Sends a new direct message to the specified user from the authenticating user.
        /// </summary>
        /// <param name="tokens">The OAuth tokens.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="text">The text of your direct message.</param>
        /// <returns>
        /// A <see cref="TwitterDirectMessage"/> instance.
        /// </returns>
        public static TwitterResponse<TwitterDirectMessage> Send(OAuthTokens tokens, decimal userId, string text)
        {
            return Send(tokens, userId, text, null);
        }

        /// <summary>
        /// Sends a new direct message to the specified user from the authenticating user.
        /// </summary>
        /// <param name="tokens">The OAuth tokens.</param>
        /// <param name="screenName">The user's screen name.</param>
        /// <param name="text">The message text.</param>
        /// <param name="options">The options.</param>
        /// <returns>A <see cref="TwitterDirectMessage"/> object of the created direct message.</returns>
        public static TwitterResponse<TwitterDirectMessage> Send(OAuthTokens tokens, string screenName, string text, OptionalProperties options)
        {
            Commands.SendDirectMessageCommand command = new Commands.SendDirectMessageCommand(tokens, text, screenName, options);

            TwitterResponse<TwitterDirectMessage> result = Core.CommandPerformer.PerformAction(command);

            return result;
        }

        /// <summary>
        /// Sends a new direct message to the specified user from the authenticating user.
        /// </summary>
        /// <param name="tokens">The OAuth tokens.</param>
        /// <param name="screenName">The user's screen name.</param>
        /// <param name="text">The message text.</param>
        /// <returns>A <see cref="TwitterDirectMessage"/> object of the created direct message.</returns>
        public static TwitterResponse<TwitterDirectMessage> Send(OAuthTokens tokens, string screenName, string text)
        {
            return Send(tokens, screenName, text, null);
        }

        /// <summary>
        /// Returns a list of the 20 most recent direct messages sent by the authenticating user.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// A <see cref="TwitterDirectMessageCollection"/> instance.
        /// </returns>
        public static TwitterResponse<TwitterDirectMessageCollection> DirectMessagesSent(OAuthTokens tokens, DirectMessagesSentOptions options)
        {
            return CommandPerformer.PerformAction(new Commands.DirectMessagesSentCommand(tokens, options));
        }

        /// <summary>
        /// Deletes this direct message.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// A <see cref="TwitterDirectMessage"/> instance.
        /// </returns>
        public TwitterResponse<TwitterDirectMessage> Delete(OAuthTokens tokens, OptionalProperties options)
        {
            Commands.DeleteDirectMessageCommand command = new Commands.DeleteDirectMessageCommand(tokens, this.Id, options);

            TwitterResponse<TwitterDirectMessage> result = Core.CommandPerformer.PerformAction(command);

            return result;
        }

        /// <summary>
        /// Deletes this direct message.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="id">The direct message id.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// A <see cref="TwitterDirectMessage"/> instance.
        /// </returns>
        public static TwitterResponse<TwitterDirectMessage> Delete(OAuthTokens tokens, decimal id, OptionalProperties options)
        {
            Commands.DeleteDirectMessageCommand command = new Commands.DeleteDirectMessageCommand(tokens, id, options);

            TwitterResponse<TwitterDirectMessage> result = Core.CommandPerformer.PerformAction(command);

            return result;
        }


        /// <summary>
        /// Returns a single direct message, specified by an id parameter. Like the /1/direct_messages.format request, this method will include the user objects of the sender and recipient.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="id">The id.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static TwitterResponse<TwitterDirectMessage> Show(OAuthTokens tokens, decimal id, OptionalProperties options)
        {
            Commands.ShowDirectMessageCommand command = new Commands.ShowDirectMessageCommand(tokens, id, options);

            return Core.CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// Returns the status text with HTML links to users, urls, and hashtags.
        /// </summary>
        /// <returns></returns>
        public string LinkifiedText()
        {
            return TwitterStatus.LinkifiedText(Entities, Text);
        }
    }
}
