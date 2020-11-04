//-----------------------------------------------------------------------
// <copyright file="TwitterPlace.cs" company="Patrick 'Ricky' Smith">
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
// <summary>The twitter place class</summary>
//-----------------------------------------------------------------------

namespace Twitterizer
{
    using System;
    using Newtonsoft.Json;
    using Twitterizer.Core;

    /// <summary>
    /// The twitter place class. Represents a place or area.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
#if !SILVERLIGHT
    [Serializable]
#endif
    [System.Diagnostics.DebuggerDisplay("{FullName} ({Id})")]
    public sealed class TwitterPlace : TwitterObject
    {
        /// <summary>
        /// Gets or sets the country code.
        /// </summary>
        /// <value>The country code.</value>
        [JsonProperty(PropertyName = "country_code")]
        public string CountryCode { get; set; }

        /// <summary>
        /// Gets or sets the type of the place.
        /// </summary>
        /// <value>The type of the place.</value>
        [JsonProperty(PropertyName = "place_type")]
        public string PlaceType { get; set; }

        /// <summary>
        /// Gets or sets the address of the data.
        /// </summary>
        /// <value>The address of the data.</value>
        [JsonProperty(PropertyName = "url")]
        public string DataAddress { get; set; }

        /// <summary>
        /// Gets or sets the country.
        /// </summary>
        /// <value>The country.</value>
        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets the address of the street.
        /// </summary>
        /// <value>The address of the street.</value>
        [JsonProperty(PropertyName = "street_address")]
        public string StreetAddress { get; set; }

        /// <summary>
        /// Gets or sets the postal code.
        /// </summary>
        /// <value>The postal code.</value>
        /// <remarks></remarks>
        [JsonProperty(PropertyName = "postal_code")]
        public string PostalCode { get; set; }

        /// <summary>
        /// Gets or sets the phone number in the preferred local format for the place, include long distance code.
        /// </summary>
        /// <value>The phone number.</value>
        /// <remarks></remarks>
        [JsonProperty(PropertyName = "phone")]
        public string Phone { get; set; }

        /// <summary>
        /// Gets or sets the locality.
        /// </summary>
        /// <value>The locality.</value>
        /// <remarks></remarks>
        [JsonProperty(PropertyName = "locality")]
        public string Locality { get; set; }

        /// <summary>
        /// Gets or sets the region.
        /// </summary>
        /// <value>The region.</value>
        /// <remarks></remarks>
        [JsonProperty(PropertyName = "region")]
        public string Region { get; set; }

        /// <summary>
        /// Gets or sets the iso3 country code.
        /// </summary>
        /// <value>The iso3 country code.</value>
        /// <remarks></remarks>
        [JsonProperty(PropertyName = "iso3")]
        public string Iso3CountryCode { get; set; }

        /// <summary>
        /// Gets or sets the full name.
        /// </summary>
        /// <value>The full name.</value>
        [JsonProperty(PropertyName = "full_name")]
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the name of the place.
        /// </summary>
        /// <value>The name of the place.</value>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the place id.
        /// </summary>
        /// <value>The place id.</value>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets an ID or comma separated list of IDs representing the place in the applications place database.
        /// </summary>
        /// <value>The app ids.</value>
        [JsonProperty(PropertyName = "app:id")]
        public string AppIds { get; set; }

        /// <summary>
        /// Gets or sets the bounding box.
        /// </summary>
        /// <value>The bounding box.</value>
        [JsonProperty(PropertyName = "bounding_box")]
        public TwitterBoundingBox BoundingBox { get; set; }

        /// <summary>
        /// Retrieves a place based on the specified coordinates.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="options">The options.</param>
        /// <returns>A collection of matched <see cref="Twitterizer.TwitterPlace"/> items.</returns>
        public static TwitterResponse<TwitterPlaceCollection> Lookup(double latitude, double longitude, TwitterPlaceLookupOptions options)
        {
            Commands.ReverseGeocodeCommand command = new Twitterizer.Commands.ReverseGeocodeCommand(latitude, longitude, options);

            return CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// Retrieves a place based on the specified coordinates.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <returns>A collection of matched <see cref="Twitterizer.TwitterPlace"/> items.</returns>
        public static TwitterResponse<TwitterPlaceCollection> Lookup(double latitude, double longitude)
        {
            return Lookup(latitude, longitude, null);
        }
    }
}
