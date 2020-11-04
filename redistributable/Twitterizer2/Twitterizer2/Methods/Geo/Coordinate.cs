//-----------------------------------------------------------------------
// <copyright file="Coordinate.cs" company="Patrick 'Ricky' Smith">
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
// <summary>The coordinate class</summary>
//-----------------------------------------------------------------------
namespace Twitterizer
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a single point on planet earth.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    [DataContract]
    public class Coordinate
    {
        /// <summary>
        /// Gets or sets the latitude.
        /// </summary>
        /// <value>The latitude.</value>
        public double Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude.
        /// </summary>
        /// <value>The longitude.</value>
        public double Longitude { get; set; }

        /// <summary>
        /// Reads a json array of coordinates and converts it into a collection of coordinate objects.
        /// </summary>
#if !SILVERLIGHT         
        internal class Converter : JsonConverter
#else
        public class Converter : JsonConverter
#endif
        {
            /// <summary>
            /// Determines whether this instance can convert the specified object type.
            /// </summary>
            /// <param name="objectType">Type of the object.</param>
            /// <returns>
            /// <c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
            /// </returns>
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(Collection<Coordinate>);
            }

            /// <summary>
            /// Reads the json.
            /// </summary>
            /// <param name="reader">The reader.</param>
            /// <param name="objectType">Type of the object.</param>
            /// <param name="existingValue">The existing value.</param>
            /// <param name="serializer">The serializer.</param>
            /// <returns>A deserialized <see cref="System.Collections.ObjectModel.Collection{Coordinate}"/></returns>
            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                try
                {
                    Collection<Coordinate> result = existingValue as Collection<Coordinate>;

                    if (result == null)
                        result = new Collection<Coordinate>();

                    int startDepth = reader.Depth;

                    if (reader.TokenType != JsonToken.StartArray)
                    {
                        return null;
                    }
                    
                    //int depth = reader.Depth + 1;
                    double count = 1;

                    while (reader.Read() && reader.Depth > startDepth)
                    {
                        if (new[] { JsonToken.StartArray, JsonToken.EndArray }.Contains(reader.TokenType))
                            continue;

                        int itemIndex = Convert.ToInt32(Math.Ceiling(count / 2) - 1);

                        if (count % 2 > 0)
                        {
                            result.Add(new Coordinate());
                            result[itemIndex].Latitude = Convert.ToDouble(reader.Value);
                        }
                        else
                        {
                            result[itemIndex].Longitude = Convert.ToDouble(reader.Value);
                        }

                        count++;
                    }                    
                    return result;
                }
                catch
                {
                    return null;
                }
            }

            /// <summary>
            /// Writes the json.
            /// </summary>
            /// <param name="writer">The writer.</param>
            /// <param name="value">The value.</param>
            /// <param name="serializer">The serializer.</param>
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                // TODO: Implement this.
                writer.WriteNull();
            }
        }
    }
}
