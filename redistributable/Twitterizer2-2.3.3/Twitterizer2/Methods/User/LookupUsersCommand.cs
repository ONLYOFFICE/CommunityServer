//-----------------------------------------------------------------------
// <copyright file="UserLookupCommand.cs" company="Patrick 'Ricky' Smith">
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
// <summary>The user lookup command class.</summary>
//-----------------------------------------------------------------------
namespace Twitterizer.Commands
{
    using System;
    using System.Linq;

    /// <summary>
    /// The Lookup Users command class.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    internal sealed class LookupUsersCommand : Core.TwitterCommand<TwitterUserCollection>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LookupUsersCommand"/> class.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="options">The options.</param>
        public LookupUsersCommand(OAuthTokens tokens, LookupUsersOptions options)
            : base(HTTPVerb.GET, "users/lookup.json", tokens, options)
        {
            if (tokens == null)
            {
                throw new ArgumentNullException("tokens");
            }

            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            if (options.ScreenNames.Count == 0 && options.UserIds.Count == 0)
            {
                throw new ArgumentException("At least one screen name or user id must be specified.");
            }
        }

        /// <summary>
        /// Inits this instance.
        /// </summary>
        public override void Init()
        {
            LookupUsersOptions options = this.OptionalProperties as LookupUsersOptions;
            if (options == null)
            {
                throw new NullReferenceException("The optional parameters class is not valid for this command.");
            }

            if (options.UserIds.Count > 0)
                this.RequestParameters.Add("user_id",
                                           string.Join(",", options.UserIds.Where(id => id > 0).Select(id => id.ToString()).ToArray()));

            if (options.ScreenNames.Count > 0)
                this.RequestParameters.Add("screen_name", string.Join(",", options.ScreenNames.ToArray()));

            if (options.IncludeEntities)
                this.RequestParameters.Add("include_entities", "true");
        }
    }
}
