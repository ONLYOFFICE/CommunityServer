//-----------------------------------------------------------------------
// <copyright file="TwitterRelationship.cs" company="Patrick 'Ricky' Smith">
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
// <summary>The twitter relationship entity class</summary>
//-----------------------------------------------------------------------

namespace Twitterizer
{
    using System;
    using System.Diagnostics;
    using Newtonsoft.Json;
    using Twitterizer.Core;

    /// <summary>
    /// The Twitter Relationship entity class
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [DebuggerDisplay("TwitterRelationship = {Source} -> {Target}")]
#if !SILVERLIGHT
    [Serializable]
#endif
    public class TwitterRelationship : TwitterObject
    {
        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        /// <value>The source.</value>
        [JsonProperty(PropertyName = "source")]
        public TwitterRelationshipUser Source { get; set; }

        /// <summary>
        /// Gets or sets the target.
        /// </summary>
        /// <value>The target.</value>
        [JsonProperty(PropertyName = "target")]
        public TwitterRelationshipUser Target { get; set; }

        /// <summary>
        /// Gets or sets the relationship.
        /// </summary>
        /// <value>The relationship.</value>
        [JsonProperty(PropertyName = "relationship")]
        public TwitterRelationship Relationship
        {
            get
            {
                return this;
            }

            set
            {
                if (value != null)
                {
                    this.Target = value.Target;
                    this.Source = value.Source;
                }
            }
        }

        /// <summary>
        /// Allows the authenticating users to unfollow the user specified.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <returns>
        /// Returns the unfollowed user in the requested format when successful. Returns a string describing the failure condition when unsuccessful.
        /// </returns>
        public TwitterResponse<TwitterUser> Delete(OAuthTokens tokens)
        {
            Commands.DeleteFriendshipCommand command = new Twitterizer.Commands.DeleteFriendshipCommand(tokens, this.Target.Id, string.Empty, null);

            return CommandPerformer.PerformAction(command);
        }
    }
}
