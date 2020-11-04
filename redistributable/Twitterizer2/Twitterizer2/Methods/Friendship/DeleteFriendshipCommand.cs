//-----------------------------------------------------------------------
// <copyright file="DeleteFriendshipCommand.cs" company="Patrick 'Ricky' Smith">
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
// <summary>The delete friendship command class</summary>
//-----------------------------------------------------------------------

namespace Twitterizer.Commands
{
    using System;
    using System.Globalization;
    using Twitterizer.Core;

    /// <summary>
    /// The delete friendship command class.
    /// </summary>
    [AuthorizedCommandAttribute]
#if !SILVERLIGHT
    [Serializable]
#endif
    internal sealed class DeleteFriendshipCommand : Core.TwitterCommand<TwitterUser>
    {
        /// <summary>
        /// The base address to the API method.
        /// </summary>
        private const string Path = "friendships/destroy.json";

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteFriendshipCommand"/> class.
        /// </summary>
        /// <param name="tokens">The request tokens.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="userName">The user name.</param>
        /// <param name="optionalProperties">The optional properties.</param>
        public DeleteFriendshipCommand(OAuthTokens tokens, decimal userId, string userName, OptionalProperties optionalProperties)
            : base(HTTPVerb.POST, Path, tokens, optionalProperties)
        {
            if (tokens == null)
            {
                throw new ArgumentNullException("tokens");
            }

            if (userId <= 0 && string.IsNullOrEmpty(userName))
            {
                throw new ArgumentException("User ID or screen name is required.");
            }

            this.UserName = userName;
            this.UserId = userId;
        }

        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        /// <value>The user id.</value>
        public decimal UserId { get; internal set; }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        /// <value>The username.</value>
        public string UserName { get; internal set; }

        /// <summary>
        /// Initializes the command.
        /// </summary>
        public override void Init()
        {
            if (this.UserId > 0)
            {
                this.RequestParameters.Add("user_id", this.UserId.ToString("#"));
            }
            else if (!string.IsNullOrEmpty(this.UserName))
            {
                this.RequestParameters.Add("screen_name", this.UserName);
            }
        }
    }
}
