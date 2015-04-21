//-----------------------------------------------------------------------
// <copyright file="TwitterGeo.cs" company="Patrick 'Ricky' Smith">
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
// <summary>The geo location class.</summary>
//-----------------------------------------------------------------------

namespace Twitterizer
{
    using System;
    using System.Collections.ObjectModel;
    using Newtonsoft.Json;

    /// <summary>
    /// Lists the possible types of geographic boundaries.
    /// </summary>
    public enum TwitterGeoShapeType
    {
        /// <summary>
        /// A single point. Expect one coordinate.
        /// </summary>
        Point,

        /// <summary>
        /// A line, or multiple lines joined end-to-end.
        /// </summary>
        LineString,

        /// <summary>
        /// A polygon-shaped area.
        /// </summary>
        Polygon,

        /// <summary>
        /// A circle represented by a single point (the center) and the radius.
        /// </summary>
        CircleByCenterPoint
    }

    /// <summary>
    /// Represents a geological area
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    public class TwitterGeo
    {
        /// <summary>
        /// Gets or sets the type of the shape.
        /// </summary>
        /// <value>The type of the shape.</value>
        [JsonProperty(PropertyName = "type")]
        public TwitterGeoShapeType ShapeType { get; set; }

        /// <summary>
        /// Gets or sets the coordinates.
        /// </summary>
        /// <value>The coordinates.</value>
        [JsonProperty(PropertyName = "coordinates")]
        [JsonConverter(typeof(Coordinate.Converter))]
        public Collection<Coordinate> Coordinates { get; set; }
    }
}
