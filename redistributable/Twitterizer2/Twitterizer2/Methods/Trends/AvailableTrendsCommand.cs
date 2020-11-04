//-----------------------------------------------------------------------
// <copyright file="AvailableTrendsCommand.cs" company="Patrick 'Ricky' Smith">
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
// <summary>The available trends command class</summary>
//-----------------------------------------------------------------------

namespace Twitterizer.Commands
{
    using System;
    using Twitterizer;
    using Twitterizer.Core;
    using System.Globalization;

    /// <summary>
    /// The create list command class
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    internal sealed class AvailableTrendsCommand : TwitterCommand<TwitterTrendLocationCollection>
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="AvailableTrendsCommand"/> class.
        /// </summary>
        /// <param name="tokens">The request tokens.</param>
        /// <param name="options">The options.</param>
        public AvailableTrendsCommand(OAuthTokens tokens, AvailableTrendsOptions options)
            : base(
                HTTPVerb.GET,
                "trends/available.json",
                tokens,
                options)
        {
        }
        #endregion


        public override void Init()
        {
            AvailableTrendsOptions options = this.OptionalProperties as AvailableTrendsOptions;
            if (options == null)
            {
                return;
            }
            
            if (options.Lat != null)
                this.RequestParameters.Add("lat", options.Lat.ToString());

            if (options.Long != null)
                this.RequestParameters.Add("long", options.Long.ToString());        
        }
    }
}
