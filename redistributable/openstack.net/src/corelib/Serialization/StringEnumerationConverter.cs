using System;
using Newtonsoft.Json;

namespace OpenStack.Serialization
{
    /// <summary>
    /// Attempts to convert a string to a <see cref="StringEnumeration"/>.
    /// </summary>
    /// <exclude />
    public class StringEnumerationConverter : JsonConverter
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsSubclassOf(typeof(StringEnumeration));
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string displayName = serializer.Deserialize<string>(reader);
            return StringEnumeration.FromDisplayName(objectType, displayName);
        }

        /// <summary />
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var enumeration = (StringEnumeration)value;
            serializer.Serialize(writer, enumeration.DisplayName);
        }
    }
}