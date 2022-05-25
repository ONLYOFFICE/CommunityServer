using System;
using Newtonsoft.Json;
using OpenStack.Serialization;

namespace OpenStack.Compute.v2_1.Serialization
{
    /// <summary>
    /// Handles serialization for Compute KeyPairs, which don't follow the usual convention of not wrapping the object when contained in a list.
    /// </summary>
    /// <exclude />
    public class KeyPairConverter : DefaultJsonConverter
    {
        private readonly string _name = "keypair";

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // Wrap
            writer.WriteStartObject();
            writer.WritePropertyName(_name);

            // Default serialization
            base.WriteJson(writer, value, serializer);

            writer.WriteEndObject();
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // Skip to the desired property
            while (reader.Read())
            {
                if (reader.TokenType != JsonToken.PropertyName || reader.Value.ToString() != _name)
                    continue;

                // Advance to the contained value
                reader.Read();
                break;
            }

            // Default Deserialization
            object result = base.ReadJson(reader, objectType, existingValue, serializer);

            while (reader.Read() && reader.TokenType != JsonToken.EndObject) { } // Advance to end of object

            return result;
        }
    }
}