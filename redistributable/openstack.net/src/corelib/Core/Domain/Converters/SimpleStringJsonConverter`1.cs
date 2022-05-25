namespace net.openstack.Core.Domain.Converters
{
    using System;
    using System.Globalization;
    using Newtonsoft.Json;

    /// <summary>
    /// Provides a base class for JSON converters that represent serialized objects
    /// as a simple string.
    /// </summary>
    /// <typeparam name="T">The deserialized object type.</typeparam>
    /// <threadsafety static="true" instance="false"/>
    public abstract class SimpleStringJsonConverter<T> : JsonConverter
    {
        /// <remarks>
        /// Serialization is performed by calling <see cref="ConvertToString"/> and writing
        /// the result as a string value.
        /// </remarks>
        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            if (!(value is T))
                throw new JsonSerializationException(string.Format(CultureInfo.InvariantCulture, "Unexpected type when converting to JSON. Expected {0}, found {1}.", typeof(T), value.GetType()));

            T entity = (T)value;
            serializer.Serialize(writer, ConvertToString(entity));
        }

        /// <remarks>
        /// Deserialization is performed by reading the raw value as a string and using
        /// <see cref="ConvertToObject"/> to convert it to an object of type
        /// <typeparamref name="T"/>.
        /// </remarks>
        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (objectType != typeof(T))
                throw new JsonSerializationException(string.Format(CultureInfo.InvariantCulture, "Expected target type {0}, found {1}.", typeof(T), objectType));

            string value = serializer.Deserialize<string>(reader);
            if (value == null)
                return null;

            return ConvertToObject(value);
        }

        /// <returns><see langword="true"/> if <paramref name="objectType"/> equals <typeparamref name="T"/>; otherwise, <see langword="false"/>.</returns>
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(T);
        }

        /// <summary>
        /// Serializes an object of type <typeparamref name="T"/> to a string value.
        /// </summary>
        /// <remarks>
        /// The default implementation returns the result of calling <see cref="Object.ToString()"/>.
        /// </remarks>
        /// <param name="obj">The object to serialize.</param>
        /// <returns>A string representation of the object.</returns>
        protected virtual string ConvertToString(T obj)
        {
            return obj.ToString();
        }

        /// <summary>
        /// Deserializes a string value to an object of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="str">The string to deserialize.</param>
        /// <returns>The deserialized object.</returns>
        protected abstract T ConvertToObject(string str);
    }
}
