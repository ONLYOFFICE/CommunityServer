// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;

    using Newtonsoft.Json;

    /// <summary>
    /// Handles serialization and deserialization for TimeOfDay.
    /// </summary>
    public class TimeOfDayConverter : JsonConverter
    {
        /// <summary>
        /// Checks if the given type can be converted to a TimeOfDay.
        /// </summary>
        /// <param name="objectType">The object type.</param>
        /// <returns>True if the object is type match of TimeOfDay.</returns>
        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(TimeOfDay))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Deserialize the JSON data into a TimeOfDay object.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="existingValue">The original value.</param>
        /// <param name="serializer">The serializer to deserialize the object with.</param>
        /// <returns>A TimeOfDay object.</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            try
            {
                var dateTime = (DateTime)serializer.Deserialize(reader, typeof(DateTime));

                return new TimeOfDay(dateTime);
            }
            catch (JsonSerializationException serializationException)
            {
                throw new ServiceException(
                    new Error
                    {
                        Code = ErrorConstants.Codes.GeneralException,
                        Message = "Unable to deserialize time of day"
                    },
                    serializationException);
            }
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var timeOfDay = value as TimeOfDay;

            if (timeOfDay != null)
            {
                writer.WriteValue(timeOfDay.ToString());
            }
            else
            {
                throw new ServiceException(
                    new Error
                    {
                        Code = ErrorConstants.Codes.GeneralException,
                        Message = "Invalid type for time of day converter"
                    });
            }
        }
    }
}
