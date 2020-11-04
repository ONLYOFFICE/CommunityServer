//-----------------------------------------------------------------------
// <copyright file="ListMembershipsCommand.cs" company="Patrick 'Ricky' Smith">
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
// <summary>The list membership command class</summary>
//-----------------------------------------------------------------------

namespace Twitterizer.Commands
{
    using System;
    using System.Globalization;
    using Twitterizer;
    using Twitterizer.Core;

    /// <summary>
    /// The list membership command class
    /// </summary>
    [AuthorizedCommandAttribute]
    internal sealed class ListMembershipsCommand : TwitterCommand<TwitterListCollection>
    {
        private readonly string screenname;
        private readonly decimal userid;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListMembershipsCommand"/> class.
        /// </summary>
        /// <param name="requestTokens">The request tokens.</param>
        /// <param name="screenname">The screenname.</param>
        /// <param name="options">The options.</param>
        public ListMembershipsCommand(OAuthTokens requestTokens, string screenname, ListMembershipsOptions options)
            : base(
                HTTPVerb.GET,
                "lists/memberships.json",
                requestTokens,
                options)
        {
            if (string.IsNullOrEmpty(screenname))
            {
                throw new ArgumentNullException("screenname");
            }

            if (Tokens == null)
            {
                throw new ArgumentNullException("requestTokens");
            }

            this.DeserializationHandler = TwitterListCollection.Deserialize;
            this.screenname = screenname;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ListMembershipsCommand"/> class.
        /// </summary>
        /// <param name="requestTokens">The request tokens.</param>
        /// <param name="userid">The screenname.</param>
        /// <param name="options">The options.</param>
        public ListMembershipsCommand(OAuthTokens requestTokens, decimal userid, ListMembershipsOptions options)
            : base(
                HTTPVerb.GET,
                "lists/memberships.json",
                requestTokens,
                options)
        {
            if (userid <= 0)
            {
                throw new ArgumentNullException("userid");
            }

            if (Tokens == null)
            {
                throw new ArgumentNullException("requestTokens");
            }

            this.DeserializationHandler = TwitterListCollection.Deserialize;
            this.userid = userid;
        }

        /// <summary>
        /// Initializes the command.
        /// </summary>
        public override void Init()
        {
            if (!String.IsNullOrEmpty(screenname))
                this.RequestParameters.Add("screen_name", screenname);

            if (userid > 0)
                this.RequestParameters.Add("user_id", userid.ToString("#"));

            ListMembershipsOptions options = this.OptionalProperties as ListMembershipsOptions;
            if (options != null)
            {
                if (options.Cursor <= 0)
                    this.RequestParameters.Add("cursor", "-1");
                else
                    this.RequestParameters.Add("cursor", options.Cursor.ToString(CultureInfo.CurrentCulture));

                if (options.FilterToOwnedLists)
                    this.RequestParameters.Add("filter_to_owned_lists", "true");
            }
        }
    }
}
