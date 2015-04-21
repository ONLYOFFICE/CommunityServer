//-----------------------------------------------------------------------
// <copyright file="TwitterRelationshipUser.cs" company="Patrick 'Ricky' Smith">
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
// <author>David Golden</author>
// <summary>The twitter relationship user class</summary>
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
#if !SILVERLIGHT
    [Serializable]
#endif
    public class TwitterRelationshipUser : TwitterObject
    {  

        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        /// <value>The ID.</value>
        [JsonProperty(PropertyName = "id")]
        public decimal Id { get; set; }

        /// <summary>
        /// Gets or sets if Following.
        /// </summary>
        /// <value>Is the user following.</value>
        [JsonProperty(PropertyName = "following")]
        public bool Following { get; set; }

        /// <summary>
        /// Gets or sets the ScreenName.
        /// </summary>
        /// <value>The users ScreenName.</value>
        [JsonProperty(PropertyName = "screen_name")]
        public string ScreenName { get; set; }

        /// <summary>
        /// Gets or sets if followed by.
        /// </summary>
        /// <value>Is the user being followed by.</value>
        [JsonProperty(PropertyName = "followed_by")]
        public bool FollowedBy { get; set; }

        /// <summary>
        /// Gets or sets if notifications are enabled.
        /// </summary>
        /// <value>Notifications enabled for this user.</value>
        [JsonProperty(PropertyName = "notifications_enabled")]
        public bool? NotificationsEnabled { get; set; }

        /// <summary>
        /// Gets or sets if Can DM.
        /// </summary>
        /// <value>Can the user be DM.</value>
        [JsonProperty(PropertyName = "can_dm")]
        public bool CanDM { get; set; }

        /// <summary>
        /// Gets or sets if wants retweets.
        /// </summary>
        /// <value>The user wants to see retweets.</value>
        [JsonProperty(PropertyName = "want_retweets")]
        public bool? WantRetweets { get; set; }

        /// <summary>
        /// Gets or sets if marked as spam.
        /// </summary>
        /// <value>If the user is marked as spam.</value>
        [JsonProperty(PropertyName = "marked_spam")]
        public bool? MarkedSpam { get; set; }

        /// <summary>
        /// Gets or sets if all replies.
        /// </summary>
        /// <value>If the user wants All Replies.</value>
        [JsonProperty(PropertyName = "all_replies")]
        public bool? AllReplies { get; set; }

        /// <summary>
        /// Gets or sets if blocking.
        /// </summary>
        /// <value>If the user is blocked.</value>
        [JsonProperty(PropertyName = "blocking")]
        public bool? Blocking { get; set; }      
           
    }
}
