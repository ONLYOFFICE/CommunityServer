//-----------------------------------------------------------------------
// <copyright file="SearchCommand.cs" company="Patrick 'Ricky' Smith">
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
// <summary>The search command class</summary>
//-----------------------------------------------------------------------

namespace Twitterizer.Commands
{
    using System;
    using System.Globalization;
    using Twitterizer;
    using Core;

#if !SILVERLIGHT
    [Serializable]
#endif
    [AuthorizedCommandAttribute]
    internal sealed class SearchCommand : TwitterCommand<TwitterSearchResultCollection>
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="SearchCommand"/> class.
        /// </summary>
        /// <param name="requestTokens">The request tokens.</param>
        /// <param name="query">The query.</param>
        /// <param name="options">The options.</param>
        public SearchCommand(OAuthTokens requestTokens, string query, SearchOptions options)
            : base(HTTPVerb.GET, "search/tweets.json", requestTokens, options)
        {
            if (string.IsNullOrEmpty(query))
            {
                throw new ArgumentNullException("query");
            }

            this.Query = query;

            this.DeserializationHandler = TwitterSearchResultCollection.Deserialize;
        }
        #endregion

        /// <summary>
        /// Gets or sets the query.
        /// </summary>
        /// <value>The query.</value>
        public string Query { get; set; }

        /// <summary>
        /// Initializes the command.
        /// </summary>
        public override void Init()
        {
#if !SILVERLIGHT
            CultureInfo unitedStatesEnglishCulture = CultureInfo.GetCultureInfo("en-us");
#else
            CultureInfo unitedStatesEnglishCulture = CultureInfo.InvariantCulture;
#endif

            this.RequestParameters.Add("q", this.Query);

            SearchOptions options = this.OptionalProperties as SearchOptions;

            if (options == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(options.Language))
            {
                this.RequestParameters.Add("lang", options.Language);
            }

            if (!string.IsNullOrEmpty(options.Locale))
            {
                this.RequestParameters.Add("locale", options.Locale);
            }

            if (options.MaxId > 0)
            {
                this.RequestParameters.Add("max_id", options.MaxId.ToString("#"));
            }

            if (options.Count > 0)
            {
                this.RequestParameters.Add("count", options.Count.ToString(unitedStatesEnglishCulture));
            }

            if (options.SinceId > 0)
            {
                this.RequestParameters.Add("since_id", options.SinceId.ToString("#"));
            }

            if (!string.IsNullOrEmpty(options.GeoCode))
            {
                this.RequestParameters.Add("geocode", options.GeoCode);
            }

            if (options.UntilDate > new DateTime())
            {
                this.RequestParameters.Add("until", options.UntilDate.ToString("{0:yyyy-MM-dd}", unitedStatesEnglishCulture));
            }

            switch (options.ResultType)
            {
                case SearchOptionsResultType.Mixed:
                    this.RequestParameters.Add("result_type", "mixed");
                    break;
                case SearchOptionsResultType.Recent:
                    this.RequestParameters.Add("result_type", "recent");
                    break;
                case SearchOptionsResultType.Popular:
                    this.RequestParameters.Add("result_type", "popular");
                    break;
            }

            if (options.IncludeEntities)
                this.RequestParameters.Add("include_entities", "true");
        }
    }
}
