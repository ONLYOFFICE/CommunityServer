using System;
using System.IO;
using Ical.Net.Serialization.DataTypes;

namespace Ical.Net.Serialization
{
    public class DataMapSerializer : SerializerBase
    {
        public DataMapSerializer() {}

        public DataMapSerializer(SerializationContext ctx) : base(ctx) {}

        protected IStringSerializer GetMappedSerializer()
        {
            var sf = GetService<ISerializerFactory>();
            var mapper = GetService<DataTypeMapper>();
            if (sf == null || mapper == null)
            {
                return null;
            }

            var obj = SerializationContext.Peek();

            // Get the data type for this object
            var type = mapper.GetPropertyMapping(obj);

            return type == null
                ? new StringSerializer(SerializationContext)
                : sf.Build(type, SerializationContext) as IStringSerializer;
        }

        public override Type TargetType
        {
            get
            {
                ISerializer serializer = GetMappedSerializer();
                return serializer?.TargetType;
            }
        }

        public override string SerializeToString(object obj)
        {
            var serializer = GetMappedSerializer();
            return serializer?.SerializeToString(obj);
        }

        public override object Deserialize(TextReader tr)
        {
            var serializer = GetMappedSerializer();
            if (serializer == null)
            {
                return null;
            }

            var value = tr.ReadToEnd();
            var returnValue = serializer.Deserialize(new StringReader(value));

            // Default to returning the string representation of the value
            // if the value wasn't formatted correctly.
            // FIXME: should this be a try/catch?  Should serializers be throwing
            // an InvalidFormatException?  This may have some performance issues
            // as try/catch is much slower than other means.
            return returnValue ?? value;
        }
    }
}