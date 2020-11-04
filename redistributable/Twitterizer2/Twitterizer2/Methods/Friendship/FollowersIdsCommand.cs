//-----------------------------------------------------------------------
// <copyright file="FollowersIdsCommand.cs" company="Patrick 'Ricky' Smith">
//  This file is part of the Twitterizer library (http://www.twitterizer.net)
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
// <author>Edgardo Vega</author>
// <summary>The Friendship Resources members command class.</summary>
//-----------------------------------------------------------------------

namespace Twitterizer.Commands
{
    using System;
    using System.Globalization;
    using Twitterizer.Core;


    /// <summary>
    /// Returns the members of the specified list.
    /// </summary>
    [AuthorizedCommand]
    internal class FollowersIdsCommand : TwitterCommand<UserIdCollection>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetListsCommand"/> class.
        /// </summary>
        /// <param name="requestTokens">The request tokens.</param>
        /// <param name="options">The options.</param>
        public FollowersIdsCommand(OAuthTokens requestTokens, UsersIdsOptions options)
            : base(HTTPVerb.GET, string.Format(CultureInfo.CurrentCulture, "followers/ids.json"), requestTokens, options)
        {

            if (requestTokens == null)
            {
                throw new ArgumentNullException("requestTokens");
            }


            this.DeserializationHandler = UserIdCollection.DeserializeWrapper;
        }
               
        /// <summary>
        /// Initializes the command.
        /// </summary>
        public override void Init()
        {
            UsersIdsOptions options = this.OptionalProperties as UsersIdsOptions;

            if (options == null)
            {
                this.RequestParameters.Add("cursor", "-1");
                return;
            }

            if (options.UserId > 0)
                this.RequestParameters.Add("user_id", options.UserId.ToString("#"));

            if (!string.IsNullOrEmpty(options.ScreenName))
                this.RequestParameters.Add("screen_name", options.ScreenName);

            this.RequestParameters.Add("cursor", options.Cursor > 0 ? options.Cursor.ToString(CultureInfo.InvariantCulture) : "-1");
        }
    }
}
