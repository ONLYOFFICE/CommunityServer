//-----------------------------------------------------------------------
// <copyright file="FollowersCommand.cs" company="Patrick 'Ricky' Smith">
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
// <summary>The command to obtain followers of a user.</summary>
//-----------------------------------------------------------------------
namespace Twitterizer.Commands
{
    using System;
    using System.Globalization;
    using Twitterizer;
    using Twitterizer.Core;

    /// <summary>
    /// The command to obtain followers of a user.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    internal sealed class FollowersCommand :
        Core.TwitterCommand<TwitterUserCollection>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FollowersCommand"/> class.
        /// </summary>
        /// <param name="tokens">The request tokens.</param>
        /// <param name="options">The options.</param>
        public FollowersCommand(OAuthTokens tokens, FollowersOptions options)
            : base(HTTPVerb.GET, "statuses/followers.json", tokens, options)
        {
            if (tokens == null && options == null)
            {
                throw new ArgumentException("You must provide an authorization token or specify a user."); 
            }

            if (tokens == null && options != null && options.UserId <= 0 && string.IsNullOrEmpty(options.ScreenName))
            {
                throw new ArgumentException("Either a user id or screen name (but not both) must be supplied (using the options parameter) when called unauthorized.");
            }

            this.DeserializationHandler = TwitterUserCollection.DeserializeWrapper;
        }
        
        /// <summary>
        /// Initializes the command.
        /// </summary>
        public override void Init()
        {
            // Default values
            this.RequestParameters.Add("cursor", "-1");

            // Handle optional parameters
            FollowersOptions options = this.OptionalProperties as FollowersOptions;

            if (options == null)
            {
                return;
            }

            if (options.UserId > 0)
                this.RequestParameters.Add("user_id", options.UserId.ToString("#"));

            if (!string.IsNullOrEmpty(options.ScreenName))
                this.RequestParameters.Add("screen_name", options.ScreenName);

            // Override the default
            if (options.Cursor != 0)
                this.RequestParameters["cursor"] = options.Cursor.ToString(CultureInfo.CurrentCulture);
        }
    }
}
