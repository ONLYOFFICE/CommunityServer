//-----------------------------------------------------------------------
// <copyright file="TwitterAccount.cs" company="Patrick 'Ricky' Smith">
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
// <summary>The TwitterAccount class.</summary>
//-----------------------------------------------------------------------
namespace Twitterizer.Commands
{
    using System.Globalization;
    using Twitterizer.Core;
    using System;

    internal class ReportSpamCommand : TwitterCommand<TwitterUser>
    {
        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        /// <value>The user id.</value>
        public decimal UserId { get; set; }

        /// <summary>
        /// Gets or sets the name of the screen.
        /// </summary>
        /// <value>The name of the screen.</value>
        public string ScreenName { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportSpamCommand"/> class.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="screenName">Name of the screen.</param>
        /// <param name="options">The options.</param>
        public ReportSpamCommand(OAuthTokens tokens, decimal userId, string screenName, OptionalProperties options)
            : base(HTTPVerb.POST, "report_spam.json", tokens, options)
        {
            if (string.IsNullOrEmpty(screenName) && userId <= 0)
            {
                throw new ArgumentException("A screen name or user id is required.");
            }
            this.ScreenName = screenName;
            this.UserId = userId;
        }

        /// <summary>
        /// Inits this instance.
        /// </summary>
        public override void Init()
        {
            if (this.UserId > 0)
            {
                this.RequestParameters.Add("user_id", this.UserId.ToString("#"));
            }
            else if (!string.IsNullOrEmpty(this.ScreenName))
            {
                this.RequestParameters.Add("screen_name", this.ScreenName);
            }
        }
    }
}
