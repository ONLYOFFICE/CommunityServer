using System;
using System.IO;
using Ical.Net.DataTypes;

namespace Ical.Net.Serialization.DataTypes
{
    public class TriggerSerializer : StringSerializer
    {
        public TriggerSerializer() { }

        public TriggerSerializer(SerializationContext ctx) : base(ctx) { }

        public override Type TargetType => typeof (Trigger);

        public override string SerializeToString(object obj)
        {
            try
            {
                if (!(obj is Trigger t))
                {
                    return null;
                }

                // Push the trigger onto the serialization stack
                SerializationContext.Push(t);
                try
                {
                    var factory = GetService<ISerializerFactory>();
                    if (factory == null)
                    {
                        return null;
                    }

                    var valueType = t.GetValueType() ?? typeof(TimeSpan);
                    if (!(factory.Build(valueType, SerializationContext) is IStringSerializer serializer))
                    {
                        return null;
                    }

                    var value = valueType == typeof(IDateTime)
                        ? t.DateTime
                        : (object) t.Duration;
                    return serializer.SerializeToString(value);
                }
                finally
                {
                    // Pop the trigger off the serialization stack
                    SerializationContext.Pop();
                }
            }
            catch
            {
                return null;
            }
        }

        public override object Deserialize(TextReader tr)
        {
            var value = tr.ReadToEnd();

            if (!(CreateAndAssociate() is Trigger t))
            {
                return null;
            }

            // Push the trigger onto the serialization stack
            SerializationContext.Push(t);
            try
            {
                // Decode the value as needed
                value = Decode(t, value);

                // Set the trigger relation
                if (t.Parameters.ContainsKey("RELATED") && t.Parameters.Get("RELATED").Equals("END"))
                {
                    t.Related = TriggerRelation.End;
                }

                var factory = GetService<ISerializerFactory>();
                if (factory == null)
                {
                    return null;
                }

                var valueType = t.GetValueType() ?? typeof(TimeSpan);
                var serializer = factory.Build(valueType, SerializationContext) as IStringSerializer;
                var obj = serializer?.Deserialize(new StringReader(value));
                switch (obj)
                {
                    case null:
                        return null;
                    case IDateTime _:
                        t.DateTime = (IDateTime) obj;
                        break;
                    default:
                        t.Duration = (TimeSpan) obj;
                        break;
                }

                return t;
            }
            finally
            {
                // Pop the trigger off the serialization stack
                SerializationContext.Pop();
            }
        }
    }
}