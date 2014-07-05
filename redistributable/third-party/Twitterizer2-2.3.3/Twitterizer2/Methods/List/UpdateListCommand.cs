//-----------------------------------------------------------------------
// <copyright file="UpdateListCommand.cs" company="Patrick 'Ricky' Smith">
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
// <summary>The update list command class</summary>
//-----------------------------------------------------------------------

namespace Twitterizer.Commands
{
    using System;
    using System.Globalization;
    using Twitterizer;
    using Twitterizer.Core;

    /// <summary>
    /// The update list command class
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    internal sealed class UpdateListCommand : TwitterCommand<TwitterList>
    {
        private readonly string id;
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateListCommand"/> class.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="id">The id.</param>
        /// <param name="options">The options.</param>
        /// <remarks></remarks>
        public UpdateListCommand(OAuthTokens tokens, string id, UpdateListOptions options)
            : base(
                HTTPVerb.POST,
                "lists/update.json", 
                tokens, 
                options)
        {
            if (tokens == null)
            {
                throw new ArgumentNullException("tokens");
            }

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            this.id = id;
        }
        #endregion

        /// <summary>
        /// Initializes the command.
        /// </summary>
        public override void Init()
        {
            if (!string.IsNullOrEmpty(id))
                this.RequestParameters.Add("list_id", id);

            UpdateListOptions options = this.OptionalProperties as UpdateListOptions;

            if (options == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(options.Name))
            {
                this.RequestParameters.Add("name", options.Name);
            }

            if (options.IsPublic != null)
            {
                this.RequestParameters.Add("mode", options.IsPublic.Value ? "public" : "private");
            }

            if (!string.IsNullOrEmpty(options.Description))
            {
                this.RequestParameters.Add("description", options.Description);
            }
        }
    }
}
