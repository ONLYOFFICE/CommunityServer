using System;
using Newtonsoft.Json;

namespace OpenStack.Serialization
{
    /// <summary>
    /// Converts a json number representing seconds to a <see cref="TimeSpan"/>.
    /// </summary>
    /// <exclude />
    public class TimeSpanInSecondsConverter : JsonConverter
    {
        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var timeSpan = (TimeSpan) value;
            writer.WriteValue(timeSpan.TotalSeconds);
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            double seconds = double.Parse(reader.Value.ToString());
            return TimeSpan.FromSeconds(seconds);
        }

        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof (TimeSpan);
        }
    }
}