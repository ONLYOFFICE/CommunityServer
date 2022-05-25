using System;
using Newtonsoft.Json;

namespace OpenStack.Serialization
{
    /// <summary>
    /// Converter which uses the default serialization/deserialization.
    /// </summary>
    /// <exclude />
    public abstract class DefaultJsonConverter : JsonConverter
    {
        /// These are on by default so that the converter is picked up and used. 
        /// Once we have wrapped/unwrapped, we set to false so that the default serialization/deserialization logic is used.
        private static bool _canRead = true;
        private static bool _canWrite = true;
        private static readonly object ReadLock = new object();
        private static readonly object WriteLock = new object();
        
        /// <inheritdoc/>
        public override bool CanRead
        {
            get
            {
                lock (ReadLock)
                {
                    return _canRead;
                }
            }
        }

        /// <inheritdoc/>
        public override bool CanWrite
        {
            get
            {
                lock (WriteLock)
                {
                    return _canWrite;
                }
            }
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            lock (WriteLock)
            {
                // Regular Serialization
                _canWrite = false;
                serializer.Serialize(writer, value);
                _canWrite = true;
            }
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            object result;

            lock (ReadLock)
            {
                _canRead = false;
                result = serializer.Deserialize(reader, objectType);
                _canRead = true;
            }

            return result;
        }

        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return true;
        }
    }
}