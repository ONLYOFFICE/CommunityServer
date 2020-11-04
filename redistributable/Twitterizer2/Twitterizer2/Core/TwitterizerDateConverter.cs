//-----------------------------------------------------------------------
// <copyright file="TwitterizerDateConverter.cs" company="Patrick 'Ricky' Smith">
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
// <summary>The date converter for Twitter API dates</summary>
//-----------------------------------------------------------------------

namespace Twitterizer
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Converts date strings returned by the Twitter API into <see cref="System.DateTime"/>
    /// </summary>
    public class TwitterizerDateConverter : Newtonsoft.Json.Converters.DateTimeConverterBase
    {
        /// <summary>
        /// The date pattern for most dates returned by the API
        /// </summary>
        protected const string DateFormat = "ddd MMM dd HH:mm:ss zz00 yyyy";

        /// <summary>
        /// Reads the json.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value.</param>
        /// <param name="serializer">The serializer.</param>
        /// <returns>The parsed value as a DateTime, or null.</returns>
        public override object ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            if (reader.Value == null || reader.Value.GetType() != typeof(string))
                return new DateTime();

            DateTime parsedDate;

            return DateTime.TryParseExact(
                (string)reader.Value,
                DateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out parsedDate) ? parsedDate : new DateTime();
        }

        /// <summary>
        /// Writes the json.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The serializer.</param>
        public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            if (value.GetType() != typeof(DateTime))
                throw new ArgumentOutOfRangeException("value", "The value provided was not the expected data type.");

            writer.WriteValue(((DateTime)value).ToString(DateFormat, CultureInfo.InvariantCulture));
        }
    }
}
