using System;
using Newtonsoft.Json;

namespace OpenStack.Serialization
{
    /// <exclude />
    public class IdentifierConverter : JsonConverter
    {
        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
                return;

            Identifier id = (Identifier) value;
            writer.WriteValue(id.ToString());
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Null:
                    return null;
                case JsonToken.String:
                    var id = reader.Value.ToString();
                    return string.IsNullOrEmpty(id) ? null : new Identifier(id);
            }

            throw new JsonSerializationException(string.Format("Unexpected token when deserializing {0}", objectType.FullName));
        }

        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Identifier);
        }
    }
}
