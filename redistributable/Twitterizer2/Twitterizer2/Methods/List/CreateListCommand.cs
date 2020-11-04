//-----------------------------------------------------------------------
// <copyright file="CreateListCommand.cs" company="Patrick 'Ricky' Smith">
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
// <summary>The create list command class</summary>
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
    [AuthorizedCommandAttribute]
#if !SILVERLIGHT
    [Serializable]
#endif
    internal sealed class CreateListCommand : TwitterCommand<TwitterList>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateListCommand"/> class.
        /// </summary>
        /// <param name="requestTokens">The request tokens.</param>
        /// <param name="name">The name.</param>
        /// <param name="options">The options.</param>
        /// <remarks></remarks>
        public CreateListCommand(OAuthTokens requestTokens, string name, OptionalProperties options)
            : base(HTTPVerb.POST, "lists/create.json", requestTokens, options)
        {
            if (Tokens == null)
            {
                throw new ArgumentNullException("requestTokens");
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            this.Name = name;
        }
        
        #region API Properties
        /// <summary>
        /// Gets or sets the name of the list.
        /// </summary>
        /// <value>The name of the list.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is public.
        /// </summary>
        /// <value><c>true</c> if this instance is public; otherwise, <c>false</c>.</value>
        public bool IsPublic { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        public string Description { get; set; }
        #endregion

        /// <summary>
        /// Initializes the command.
        /// </summary>
        public override void Init()
        {
            this.RequestParameters.Add("name", this.Name);
            this.RequestParameters.Add("mode", this.IsPublic ? "public" : "private");

            if (!string.IsNullOrEmpty(this.Description))
            {
                this.RequestParameters.Add("description", this.Description);
            }
        }
    }
}
