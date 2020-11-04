//-----------------------------------------------------------------------
// <copyright file="GetListCommand.cs" company="Patrick 'Ricky' Smith">
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
// <summary>The get list command class</summary>
//-----------------------------------------------------------------------

namespace Twitterizer.Commands
{
    using System;
    using System.Globalization;
    using Twitterizer;
    using Twitterizer.Core;

    /// <summary>
    /// The create list command class
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    internal sealed class GetListCommand : TwitterCommand<TwitterList>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetListCommand"/> class.
        /// </summary>
        /// <param name="requestTokens">The request tokens.</param>
        /// <param name="slug">The slug.</param>
        /// <param name="listId">The list id.</param>
        /// <param name="options">The options.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentException"></exception>
        public GetListCommand(OAuthTokens requestTokens, string slug, decimal listId, OptionalProperties options)
            : base(
                HTTPVerb.GET, 
                "lists/show.json", 
                requestTokens, 
                options)
        {
            if (Tokens == null)
            {
                throw new ArgumentNullException("requestTokens");
            }

            if (!string.IsNullOrEmpty(slug) ^ listId > 0)
            {
                throw new ArgumentException("You must supply a list id number or slug, but not both.");
            }

            this.ListId = listId;
            this.Slug = slug;
        }

        /// <summary>
        /// Gets or sets the list id.
        /// </summary>
        /// <value>The list id.</value>
        public decimal ListId { get; set; }

        /// <summary>
        /// Gets or sets the slug.
        /// </summary>
        /// <value>The slug.</value>
        public string Slug { get; set; }

        /// <summary>
        /// Initializes the command.
        /// </summary>
        public override void Init()
        {
            if (ListId > 0)
            {
                this.RequestParameters.Add("list_id", this.ListId.ToString("#"));
            }

            if (!string.IsNullOrEmpty(this.Slug))
            {
                this.RequestParameters.Add("slug", this.Slug);
            }
        }
    }
}
