//-----------------------------------------------------------------------
// <copyright file="RetweetsOfMeOptions.cs" company="Patrick 'Ricky' Smith">
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
// <summary>The retweets of me options class.</summary>
//-----------------------------------------------------------------------

namespace Twitterizer
{
    using Twitterizer.Core;

    /// <summary>
    /// The Retweets of Me options class. Provides optional parameters for the RetweetsOfMeCommand class.
    /// </summary>
#if !SILVERLIGHT
    [System.Serializable]
#endif
    public class RetweetsOfMeOptions : OptionalProperties
    {
        /// <summary>
        /// Gets or sets the since status id.
        /// </summary>
        /// <value>The since status id.</value>
        public decimal SinceStatusId { get; set; }

        /// <summary>
        /// Gets or sets the max status id.
        /// </summary>
        /// <value>The max status id.</value>
        public decimal MaxStatusId { get; set; }

        /// <summary>
        /// Gets or sets the count of tweets to return.
        /// </summary>
        /// <value>The count of tweets.</value>
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the page number to return.
        /// </summary>
        /// <value>The page number.</value>
        public int Page { get; set; }
    }
}
