//-----------------------------------------------------------------------
// <copyright file="ListFavoritesOptions.cs" company="Patrick 'Ricky' Smith">
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
// <summary>The list favorites options class.</summary>
//-----------------------------------------------------------------------

namespace Twitterizer
{
    /// <summary>
    /// The list favorites options class. Provides a payload for optional parameters of the ListFavoritesCommand class.
    /// </summary>
#if !SILVERLIGHT
    [System.Serializable]
#endif
    public class ListFavoritesOptions : OptionalProperties
    {
        /// <summary>
        /// Gets or sets the number of favorites to return.
        /// </summary>
		  /// <value>The number of favorites to return per page.</value>
        public int Count { get;set; }

        /// <summary>
        /// Gets or sets the user name or id of the user for whom to return results for.
        /// </summary>
        /// <value>The user name or id of the user for whom to return results for.</value>
        public string UserNameOrId { get; set; }

        /// <summary>
        /// Gets or sets the page.
        /// </summary>
        /// <value>The page number.</value>
        public int Page { get; set; }

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
    }
}
