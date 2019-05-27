using System;
using System.Text;

namespace Ical.Net.Serialization
{
    internal class EncodingProvider : IEncodingProvider
    {
        public delegate string EncoderDelegate(byte[] data);

        public delegate byte[] DecoderDelegate(string value);

        private readonly SerializationContext _mSerializationContext;

        public EncodingProvider(SerializationContext ctx)
        {
            _mSerializationContext = ctx;
        }

        protected byte[] Decode7Bit(string value)
        {
            try
            {
                var utf7 = new UTF7Encoding();
                return utf7.GetBytes(value);
            }
            catch
            {
                return null;
            }
        }

        protected byte[] Decode8Bit(string value)
        {
            try
            {
                var utf8 = new UTF8Encoding();
                return utf8.GetBytes(value);
            }
            catch
            {
                return null;
            }
        }

        protected byte[] DecodeBase64(string value)
        {
            try
            {
                return Convert.FromBase64String(value);
            }
            catch
            {
                return null;
            }
        }

        protected virtual DecoderDelegate GetDecoderFor(string encoding)
        {
            if (encoding == null)
            {
                return null;
            }

            switch (encoding.ToUpper())
            {
                case "7BIT":
                    return Decode7Bit;
                case "8BIT":
                    return Decode8Bit;
                case "BASE64":
                    return DecodeBase64;
                default:
                    return null;
            }
        }

        protected string Encode7Bit(byte[] data)
        {
            try
            {
                var utf7 = new UTF7Encoding();
                return utf7.GetString(data);
            }
            catch
            {
                return null;
            }
        }

        protected string Encode8Bit(byte[] data)
        {
            try
            {
                var utf8 = new UTF8Encoding();
                return utf8.GetString(data);
            }
            catch
            {
                return null;
            }
        }

        protected string EncodeBase64(byte[] data)
        {
            try
            {
                return Convert.ToBase64String(data);
            }
            catch
            {
                return null;
            }
        }

        protected virtual EncoderDelegate GetEncoderFor(string encoding)
        {
            if (encoding == null)
            {
                return null;
            }

            switch (encoding.ToUpper())
            {
                case "7BIT":
                    return Encode7Bit;
                case "8BIT":
                    return Encode8Bit;
                case "BASE64":
                    return EncodeBase64;
                default:
                    return null;
            }
        }

        public string Encode(string encoding, byte[] data)
        {
            if (encoding == null || data == null)
            {
                return null;
            }

            var encoder = GetEncoderFor(encoding);
            //var wrapped = TextUtil.FoldLines(encoder?.Invoke(data));
            //return wrapped;
            return encoder?.Invoke(data);
        }

        public string DecodeString(string encoding, string value)
        {
            if (encoding == null || value == null)
            {
                return null;
            }

            var data = DecodeData(encoding, value);
            if (data == null)
            {
                return null;
            }

            // Decode the string into the current encoding
            var encodingStack = _mSerializationContext.GetService(typeof (EncodingStack)) as EncodingStack;
            return encodingStack.Current.GetString(data);
        }

        public byte[] DecodeData(string encoding, string value)
        {
            if (encoding == null || value == null)
            {
                return null;
            }

            var decoder = GetDecoderFor(encoding);
            return decoder?.Invoke(value);
        }
    }
}