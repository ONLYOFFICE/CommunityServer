//-----------------------------------------------------------------------
// <copyright file="SendDirectMessageCommand.cs" company="Patrick 'Ricky' Smith">
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
// <summary>The Send Direct Message Command class</summary>
//-----------------------------------------------------------------------

namespace Twitterizer.Commands
{
    using System;
    using System.Globalization;
    using Twitterizer.Core;

    /// <summary>
    /// The Send Direct Message Command class
    /// </summary>
    [AuthorizedCommandAttribute]
#if !SILVERLIGHT
    [Serializable]
#endif
    internal sealed class SendDirectMessageCommand : TwitterCommand<TwitterDirectMessage>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SendDirectMessageCommand"/> class.
        /// </summary>
        /// <param name="tokens">The request tokens.</param>
        /// <param name="text">The message text.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="options">The options.</param>
        public SendDirectMessageCommand(OAuthTokens tokens, string text, decimal userId, OptionalProperties options)
            : this(tokens, text, options)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("User Id must be supplied", "userId");
            }

            this.RecipientUserId = userId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SendDirectMessageCommand"/> class.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="text">The message text.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="options">The options.</param>
        public SendDirectMessageCommand(OAuthTokens tokens, string text, string userName, OptionalProperties options)
            : this(tokens, text, options)
        {
            if (string.IsNullOrEmpty(userName))
            {
                throw new ArgumentNullException("userName");
            }

            this.RecipientUserName = userName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SendDirectMessageCommand"/> class.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="text">The message text.</param>
        /// <param name="options">The options.</param>
        private SendDirectMessageCommand(OAuthTokens tokens, string text, OptionalProperties options)
            : base(HTTPVerb.POST, "direct_messages/new.json", tokens, options)
        {
            if (tokens == null)
            {
                throw new ArgumentNullException("tokens");
            }

            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentNullException("text");
            }

            this.Text = text;
        }

        #region Properties
        /// <summary>
        /// Gets or sets the status text.
        /// </summary>
        /// <value>The status text.</value>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the recipient user id.
        /// </summary>
        /// <value>The recipient user id.</value>
        public decimal RecipientUserId { get; set; }

        /// <summary>
        /// Gets or sets the name of the recipient user.
        /// </summary>
        /// <value>The name of the recipient user.</value>
        public string RecipientUserName { get; set; }
        #endregion

        /// <summary>
        /// Initializes the command.
        /// </summary>
        public override void Init()
        {
            this.RequestParameters.Add("text", this.Text);

            if (this.RecipientUserId > 0)
                this.RequestParameters.Add("user_id", this.RecipientUserId.ToString("#"));

            if (!string.IsNullOrEmpty(this.RecipientUserName) && this.RecipientUserId <= 0)
                this.RequestParameters.Add("screen_name", this.RecipientUserName);
        }
    }
}
