using Ical.Net.DataTypes;

namespace Ical.Net.Serialization.DataTypes
{
    public abstract class EncodableDataTypeSerializer : DataTypeSerializer
    {
        protected EncodableDataTypeSerializer() {}

        protected EncodableDataTypeSerializer(SerializationContext ctx) : base(ctx) {}

        protected string Encode(IEncodableDataType dt, string value)
        {
            if (value == null)
            {
                return null;
            }

            if (dt?.Encoding == null)
            {
                return value;
            }

            // Return the value in the current encoding
            var encodingStack = GetService<EncodingStack>();
            return Encode(dt, encodingStack.Current.GetBytes(value));
        }

        protected string Encode(IEncodableDataType dt, byte[] data)
        {
            if (data == null)
            {
                return null;
            }

            if (dt?.Encoding == null)
            {
                // Default to the current encoding
                var encodingStack = GetService<EncodingStack>();
                return encodingStack.Current.GetString(data);
            }

            var encodingProvider = GetService<IEncodingProvider>();
            return encodingProvider?.Encode(dt.Encoding, data);
        }

        protected string Decode(IEncodableDataType dt, string value)
        {
            if (dt?.Encoding == null)
            {
                return value;
            }

            var data = DecodeData(dt, value);
            if (data == null)
            {
                return null;
            }

            // Default to the current encoding
            var encodingStack = GetService<EncodingStack>();
            return encodingStack.Current.GetString(data);
        }

        protected byte[] DecodeData(IEncodableDataType dt, string value)
        {
            if (value == null)
            {
                return null;
            }

            if (dt?.Encoding == null)
            {
                // Default to the current encoding
                var encodingStack = GetService<EncodingStack>();
                return encodingStack.Current.GetBytes(value);
            }

            var encodingProvider = GetService<IEncodingProvider>();
            return encodingProvider?.DecodeData(dt.Encoding, value);
        }
    }
}