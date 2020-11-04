//-----------------------------------------------------------------------
// <copyright file="SearchOptions.cs" company="Patrick 'Ricky' Smith">
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
// <summary>The search options class</summary>
//-----------------------------------------------------------------------
namespace Twitterizer
{
    using System;

    /// <summary>
    /// The available search result type filter options.
    /// </summary>
    public enum SearchOptionsResultType
    {
        /// <summary>
        /// Use Twitter's default
        /// </summary>
        Default,

        /// <summary>
        /// Include both popular and real time results in the response.
        /// </summary>
        Mixed,
        
        /// <summary>
        /// Return only the most recent results in the response.
        /// </summary>
        Recent,
        
        /// <summary>
        /// Return only the most popular results in the response.
        /// </summary>
        Popular
    }

    /// <summary>
    /// The search options class. Provides a payload for optional parameters for the SearchCommand class.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    public class SearchOptions : OptionalProperties
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SearchOptions"/> class.
        /// </summary>
        public SearchOptions()
        {
            UseSSL = true;
        }

        /// <summary>
        /// Gets or sets the language.
        /// </summary>
        /// <value>The language.</value>
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets the locale.
        /// </summary>
        /// <value>The locale.</value>
        public string Locale { get; set; }

        /// <summary>
        /// Gets or sets the max id.
        /// </summary>
        /// <value>The max id.</value>
        public decimal MaxId { get; set; }

        /// <summary>
        /// Gets or sets the number per page.
        /// </summary>
        /// <value>The number per page.</value>
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the since id.
        /// </summary>
        /// <value>The since id.</value>
        public decimal SinceId { get; set; }

        /// <summary>
        /// Gets or sets the geo code string. 
        /// The parameter value is specified by "latitude,longitude,radius", where radius units must be specified as either "mi" (miles) or "km" (kilometers). Note that you cannot use the near operator via the API to geocode arbitrary locations; however you can use this geocode parameter to search near geocodes directly.
        /// </summary>
        /// <value>The geo code.</value>
        public string GeoCode { get; set; }

        /// <summary>
        /// Gets or sets the until date.
        /// </summary>
        /// <value>The until date.</value>
        public DateTime UntilDate { get; set; }

        /// <summary>
        /// Gets or sets the type of the result.
        /// </summary>
        /// <value>The type of the result.</value>
        public SearchOptionsResultType ResultType { get; set; }

        /// <summary>
        /// Gets or sets whether to include some entities in the result.
        /// </summary>
        /// <value>The type of the result.</value>
        public bool IncludeEntities { get; set; }
    }
}
