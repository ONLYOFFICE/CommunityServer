using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OpenStack.Networking.v2.Serialization
{
    /// <summary>
    /// Handles serialization/deserialization for <see cref="PortCreateDefinition.DHCPOptions"/>.
    /// </summary>
    /// <seealso href="http://specs.openstack.org/openstack/neutron-specs/specs/api/extra_dhcp_options__extra-dhcp-opt_.html"/>
    /// <exclude />
    public class DHCPOptionsConverter : JsonConverter
    {
        /// <summary>
        /// Writes the json.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The serializer.</param>
        /// <exception cref="NotImplementedException"></exception>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
                return;

            writer.WriteStartArray();
            var options = (Dictionary<string, string>) value;
            foreach (var option in options)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("opt_name");
                writer.WriteValue(option.Key);
                writer.WritePropertyName("opt_value");
                writer.WriteValue(option.Value);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }

        /// <summary>
        /// Reads the json.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value.</param>
        /// <param name="serializer">The serializer.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var dhcpOptions = new Dictionary<string, string>();

            if (reader.TokenType == JsonToken.StartArray)
                ReadAndAssert(reader);

            while (reader.TokenType != JsonToken.EndArray)
            {
                var dhcpOption = ReadItem(reader, serializer);
                dhcpOptions.Add(dhcpOption.Key, dhcpOption.Value);
                ReadAndAssert(reader);
            }

            return dhcpOptions;
        }

        private static KeyValuePair<string, string> ReadItem(JsonReader reader, JsonSerializer serializer)
        {
            string key = null;
            string value = null;

            ReadAndAssert(reader);

            while (reader.TokenType == JsonToken.PropertyName)
            {
                string propertyName = reader.Value.ToString();
                if (string.Equals(propertyName, "opt_name", StringComparison.OrdinalIgnoreCase))
                {
                    ReadAndAssert(reader);
                    key = serializer.Deserialize<string>(reader);
                }
                else if (string.Equals(propertyName, "opt_value", StringComparison.OrdinalIgnoreCase))
                {
                    ReadAndAssert(reader);
                    value = serializer.Deserialize<string>(reader);
                }
                else
                {
                    reader.Skip();
                }

                ReadAndAssert(reader);
            }

            return new KeyValuePair<string, string>(key, value);
        } 

        private static void ReadAndAssert(JsonReader reader)
        {
            if (!reader.Read())
                throw new JsonSerializationException("Unexpected end when reading DHCPOptions.");
        }

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns></returns>
        public override bool CanConvert(Type objectType)
        {
            return typeof (Dictionary<string, string>).IsAssignableFrom(objectType);
        }
    }
}