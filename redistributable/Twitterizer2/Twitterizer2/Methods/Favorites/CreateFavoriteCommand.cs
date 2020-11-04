//-----------------------------------------------------------------------
// <copyright file="CreateFavoriteCommand.cs" company="Patrick 'Ricky' Smith">
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
// <summary>The create favorite command class.</summary>
//-----------------------------------------------------------------------

namespace Twitterizer.Commands
{
    using System;
    using System.Globalization;
    using Twitterizer.Core;

    /// <summary>
    /// The Create Favorite Command class. Favorites the status specified in the ID parameter as the authenticating user. Returns the favorite status when successful.
    /// </summary>
    [AuthorizedCommandAttribute]
#if !SILVERLIGHT
    [Serializable]
#endif
    internal sealed class CreateFavoriteCommand : TwitterCommand<TwitterStatus>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateFavoriteCommand"/> class.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="statusId">The status id.</param>
        /// <param name="options">The options.</param>
        public CreateFavoriteCommand(OAuthTokens tokens, decimal statusId, OptionalProperties options) :
            base(HTTPVerb.POST, string.Format(CultureInfo.InvariantCulture.NumberFormat, "favorites/create.json?id={0}", statusId), tokens, options)
        {
            if (tokens == null)
            {
                throw new ArgumentNullException("tokens");
            }

            if (statusId <= 0)
            {
                throw new ArgumentException("Status Id is required.");
            }
        }

        /// <summary>
        /// Initializes the command.
        /// </summary>
        public override void Init()
        {
        }
    }
}
