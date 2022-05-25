using System;
using Newtonsoft.Json;

namespace OpenStack.Serialization
{
    /// <summary>
    /// Some of the OpenStack API's like to return a wrapper around the real object requested. This will deal with that root level wrapper.
    /// <para>Note that it only affects the root, if there are nested objects that also use this converter, it is assumed that they don't have a wrapper.</para>
    /// </summary>
    /// <exclude />
    public class RootWrapperConverter : DefaultJsonConverter
    {
        private readonly string _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="RootWrapperConverter"/> class.
        /// </summary>
        /// <param name="name">The root json property wrapper.</param>
        public RootWrapperConverter(string name)
        {
            _name = name;
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            bool isRoot = writer.WriteState == WriteState.Start;

            if (isRoot)
            {
                // Wrap
                writer.WriteStartObject();
                writer.WritePropertyName(_name);
            }

            // Default serialization
            base.WriteJson(writer, value, serializer);

            if (isRoot)
            {
                writer.WriteEndObject();
            }
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            bool isRoot = reader.Depth == 0;

            if (isRoot)
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
            }

            // Default Deserialization
            object result = base.ReadJson(reader, objectType, existingValue, serializer);

            if (isRoot)
            {
                while (reader.Read()) { } // Advance to end
            }

            return result;
        }
    }
}
