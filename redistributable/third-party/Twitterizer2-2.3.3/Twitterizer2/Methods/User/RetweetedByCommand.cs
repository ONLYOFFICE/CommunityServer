//-----------------------------------------------------------------------
// <copyright file="RetweetedByCommand.cs" company="Patrick 'Ricky' Smith">
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
// <summary>The retweeted by command class.</summary>
//-----------------------------------------------------------------------
namespace Twitterizer.Commands
{
    using System;
    using Twitterizer;
    using Twitterizer.Core;

    /// <summary>
    /// The retweeted by command class.
    /// </summary>
    /// <remarks>http://dev.twitter.com/doc/get/statuses/:id/retweeted_by</remarks>
    [AuthorizedCommandAttribute]
    internal class RetweetedByCommand : TwitterCommand<TwitterUserCollection>
    {
        public RetweetedByCommand(OAuthTokens tokens, decimal statusId, RetweetedByOptions options)
            : base(HTTPVerb.GET, string.Format("statuses/{0}/retweeted_by.json", statusId), tokens, options)
        {
            if (tokens == null)
            {
                throw new ArgumentNullException("tokens");
            }

            if (statusId <= 0)
            {
                throw new ArgumentNullException("statusId", "Status ID is required.");
            }
        }

        /// <summary>
        /// Inits this instance.
        /// </summary>
        public override void Init()
        {
            RetweetedByOptions options = this.OptionalProperties as RetweetedByOptions;

            if (options == null)
            {
                this.RequestParameters.Add("page", "1");
                return;
            }

            if (options.Count > 1)
            {
                this.RequestParameters.Add("count", options.Count.ToString());
            }

            if (options.IncludeEntities)
            {
                this.RequestParameters.Add("include_entities", "true");
            }

            if (options.TrimUser)
            {
                this.RequestParameters.Add("trim_user", "true");
            }

            if (options.Page > 0)
            {
                this.RequestParameters.Add("page", options.Page.ToString());
            }
        }
    }
}
