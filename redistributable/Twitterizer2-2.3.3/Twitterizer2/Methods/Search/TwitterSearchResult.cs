//-----------------------------------------------------------------------
// <copyright file="TwitterSearchResult.cs" company="Patrick 'Ricky' Smith">
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
// <summary>The twitter search result class.</summary>
//-----------------------------------------------------------------------

namespace Twitterizer
{
    using System;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;

    /// <summary>
    /// The Twitter Search Result class.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class TwitterSearchResult : Core.TwitterObject
    {
        /// <summary>
        /// Gets or sets the profile image URL.
        /// </summary>
        /// <value>The profile image URL.</value>
        [JsonProperty(PropertyName = "profile_image_url")]
        public string ProfileImageLocation { get; set; }

        /// <summary>
        /// Gets or sets the created date.
        /// </summary>
        /// <value>The created date.</value>
        [JsonProperty(PropertyName = "created_at")]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Gets or sets the name of from user screen.
        /// </summary>
        /// <value>The name of from user screen.</value>
        [JsonProperty(PropertyName = "from_user")]
        public string FromUserScreenName { get; set; }

        /// <summary>
        /// Gets or sets from user id.
        /// </summary>
        /// <value>From user id.</value>
        [JsonProperty(PropertyName = "from_user_id")]
        public long? FromUserId { get; set; }

        /// <summary>
        /// Gets or sets the name of to user screen.
        /// </summary>
        /// <value>The name of to user screen.</value>
        [JsonProperty(PropertyName = "to_user")]
        public string ToUserScreenName { get; set; }

        /// <summary>
        /// Gets or sets to user id.
        /// </summary>
        /// <value>To user id.</value>
        [JsonProperty(PropertyName = "to_user_id")]
        public long? ToUserId { get; set; }

        /// <summary>
        /// Gets or sets the status text.
        /// </summary>
        /// <value>The status text.</value>
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        /// <summary>
        /// Returns the status text with HTML links to users, urls, and hashtags.
        /// </summary>
        /// <remarks>This will only work if you specify <see cref="SearchOptions.IncludeEntities"/> = <c>true</c> when executing the search.</remarks>
        /// <returns></returns>
        public string LinkifiedText()
        {
            return TwitterStatus.LinkifiedText(Entities, Text);
        }

        /// <summary>
        /// Gets or sets the status id.
        /// </summary>
        /// <value>The status id.</value>
        [JsonProperty(PropertyName = "id")]
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        /// <value>The source.</value>
        [JsonProperty(PropertyName = "source")]
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the language.
        /// </summary>
        /// <value>The language.</value>
        [JsonProperty(PropertyName = "iso_language_code")]
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets the geo location associated with the result.
        /// </summary>
        /// <value>The geo location data.</value>
        [JsonProperty(PropertyName = "geo")]
        public TwitterGeo Geo { get; set; }

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>The location.</value>
        [JsonProperty(PropertyName = "location")]
        public string Location { get; set; }

        /// <summary>
        /// Gets or sets the status id the status is in reply to.
        /// </summary>
        /// <value>The status id.</value>
        [DataMember, JsonProperty(PropertyName = "in_reply_to_status_id")]
        public decimal? InReplyToStatusId { get; set; }

        /// <summary>
        /// Gets or sets the entities.
        /// </summary>
        /// <value>The entities.</value>
        [DataMember]
        [JsonProperty(PropertyName = "entities")]
        [JsonConverter(typeof(Entities.TwitterEntityCollection.Converter))]
        public Entities.TwitterEntityCollection Entities { get; set; }
    }
}
