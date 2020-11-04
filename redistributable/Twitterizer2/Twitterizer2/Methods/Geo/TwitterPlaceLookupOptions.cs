//-----------------------------------------------------------------------
// <copyright file="TwitterPlaceLookupOptions.cs" company="Patrick 'Ricky' Smith">
//  This file is part of the Twitterizer library (http://www.twitterizer.net)
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
// <summary>The twitter place lookup options class.</summary>
//-----------------------------------------------------------------------

namespace Twitterizer
{
    /// <summary>
    /// Provides a payload for optional parameters for the <see cref="Twitterizer.TwitterPlace.Lookup(double, double, TwitterPlaceLookupOptions)"/> method.
    /// </summary>
    public class TwitterPlaceLookupOptions : OptionalProperties
    {
        /// <summary>
        /// A hint on the "region" in which to search. If a number, then this is a radius in meters, but it can also take a string that is suffixed with ft to specify feet. If this is not passed in, then it is assumed to be 0m. If coming from a device, in practice, this value is whatever accuracy the device has measuring its location (whether it be coming from a GPS, WiFi triangulation, etc.).
        /// </summary>
        public string Accuracy { get; set; }

        /// <summary>
        /// The minimal granularity of data to return. If this is not passed in, then neighborhood is assumed. city can also be passed.
        /// </summary>
        public string Granularity { get; set; }

        /// <summary>
        /// A hint as to the number of results to return. This does not guarantee that the number of results returned will equal max_results, but instead informs how many "nearby" results to return. Ideally, only pass in the number of places you intend to display to the user here.
        /// </summary>
        public int? MaxResults { get; set; }
    }
}
