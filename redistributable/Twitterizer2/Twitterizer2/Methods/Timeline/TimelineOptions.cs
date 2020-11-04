//-----------------------------------------------------------------------
// <copyright file="TimelineOptions.cs" company="Patrick 'Ricky' Smith">
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
// <summary>The timeline options class.</summary>
//-----------------------------------------------------------------------

namespace Twitterizer
{
    /// <summary>
    /// The timeline options class. Provides optional parameters for timeline methods.
    /// </summary>
#if !SILVERLIGHT
    [System.Serializable]
#endif
    public class TimelineOptions : OptionalProperties
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimelineOptions"/> class.
        /// </summary>
        public TimelineOptions()
        {
            this.Page = 1;
        }

        /// <summary>
        /// Gets or sets the minimum (earliest) status id to request.
        /// </summary>
        /// <value>The since id.</value>
        public decimal SinceStatusId { get; set; }

        /// <summary>
        /// Gets or sets the max (latest) status id to request.
        /// </summary>
        /// <value>The max id.</value>
        public decimal MaxStatusId { get; set; }

        /// <summary>
        /// Gets or sets the number of messages to request.
        /// </summary>
        /// <value>The number of messages to request.</value>
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the page number to request.
        /// </summary>
        /// <value>The page number.</value>
        public int Page { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether user objects should contain only Id values.
        /// </summary>
        /// <value><c>true</c> if user objects should contain only Id values; otherwise, <c>false</c>.</value>
        public bool SkipUser { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [include retweets].
        /// </summary>
        /// <value><c>true</c> if [include retweets]; otherwise, <c>false</c>.</value>
        public bool IncludeRetweets { get; set; }

        /// <summary>
        /// Initializes the specified command.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command">The command.</param>
        /// <param name="options">The options.</param>
        internal static void Init<T>(Core.TwitterCommand<T> command, TimelineOptions options)
            where T : Core.ITwitterObject
        {
            command.RequestParameters.Add("include_entities", "true");

            if (options == null)
                options = new TimelineOptions();

            if (options.Count > 0)
                command.RequestParameters.Add("count", options.Count.ToString());

            if (options.IncludeRetweets)
                command.RequestParameters.Add("include_rts", "true");

            if (options.MaxStatusId > 0)
                command.RequestParameters.Add("max_id", options.MaxStatusId.ToString("#"));

            command.RequestParameters.Add("page", options.Page > 0 ? options.Page.ToString() : "1");

            if (options.SinceStatusId > 0)
                command.RequestParameters.Add("since_id", options.SinceStatusId.ToString("#"));

            if (options.SkipUser)
                command.RequestParameters.Add("trim_user", "true");
        }
    }
}
