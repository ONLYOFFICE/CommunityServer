//-----------------------------------------------------------------------
// <copyright file="RetweetedByMeCommand.cs" company="Patrick 'Ricky' Smith">
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
// <summary>The retweeted by me command.</summary>
//-----------------------------------------------------------------------

namespace Twitterizer.Commands
{
    using System;
    using System.Globalization;
    using Twitterizer.Core;

    /// <summary>
    /// The Retweeted By Me Command.
    /// </summary>
    [AuthorizedCommandAttribute]
    internal sealed class RetweetedByMeCommand : TwitterCommand<TwitterStatusCollection>
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="RetweetedByMeCommand"/> class.
        /// </summary>
        /// <param name="tokens">The request tokens.</param>
        /// <param name="options">The options.</param>
        public RetweetedByMeCommand(OAuthTokens tokens, TimelineOptions options)
            : base(HTTPVerb.GET, "statuses/retweeted_by_me.json", tokens, options)
        {
            if (tokens == null)
            {
                throw new ArgumentNullException("tokens");
            }
        }
        #endregion

        /// <summary>
        /// Initializes the command.
        /// </summary>
        public override void Init()
        {
            TimelineOptions options = this.OptionalProperties as TimelineOptions;
            if (options == null)
            {
                this.RequestParameters.Add("page", "1");

                return;
            }

            if (options.SinceStatusId > 0)
                this.RequestParameters.Add("since_id", options.SinceStatusId.ToString("#"));

            if (options.MaxStatusId > 0)
                this.RequestParameters.Add("max_id", options.MaxStatusId.ToString("#"));

            if (options.Count > 0)
                this.RequestParameters.Add("count", options.Count.ToString(CultureInfo.InvariantCulture));

            this.RequestParameters.Add("page", options.Page > 0 ? options.Page.ToString(CultureInfo.InvariantCulture) : "1");

            this.RequestParameters.Add("include_entities", "true");
        }
    }
}
