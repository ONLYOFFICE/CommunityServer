using System;
using System.Collections.Generic;
using System.IO;
using Ical.Net.DataTypes;

namespace Ical.Net.Serialization.DataTypes
{
    public class PeriodListSerializer : EncodableDataTypeSerializer
    {
        public PeriodListSerializer() { }

        public PeriodListSerializer(SerializationContext ctx) : base(ctx) { }

        public override Type TargetType => typeof (PeriodList);

        public override string SerializeToString(object obj)
        {
            var periodList = obj as PeriodList;
            var factory = GetService<ISerializerFactory>();
            if (periodList == null || factory == null)
            {
                return null;
            }

            var dtSerializer = factory.Build(typeof(IDateTime), SerializationContext) as IStringSerializer;
            var periodSerializer = factory.Build(typeof(Period), SerializationContext) as IStringSerializer;
            if (dtSerializer == null || periodSerializer == null)
            {
                return null;
            }

            var parts = new List<string>(periodList.Count);

            foreach (var p in periodList)
            {
                if (p.EndTime != null)
                {
                    parts.Add(periodSerializer.SerializeToString(p));
                }
                else if (p.StartTime != null)
                {
                    parts.Add(dtSerializer.SerializeToString(p.StartTime));
                }
            }

            return Encode(periodList, string.Join(",", parts));
        }

        public override object Deserialize(TextReader tr)
        {
            var value = tr.ReadToEnd();

            // Create the day specifier and associate it with a calendar object
            var rdt = CreateAndAssociate() as PeriodList;
            var factory = GetService<ISerializerFactory>();
            if (rdt == null || factory == null)
            {
                return null;
            }

            // Decode the value, if necessary
            value = Decode(rdt, value);

            var dtSerializer = factory.Build(typeof (IDateTime), SerializationContext) as IStringSerializer;
            var periodSerializer = factory.Build(typeof (Period), SerializationContext) as IStringSerializer;
            if (dtSerializer == null || periodSerializer == null)
            {
                return null;
            }

            var values = value.Split(',');
            foreach (var v in values)
            {
                var dt = dtSerializer.Deserialize(new StringReader(v)) as IDateTime;
                var p = periodSerializer.Deserialize(new StringReader(v)) as Period;

                if (dt != null)
                {
                    dt.AssociatedObject = rdt.AssociatedObject;
                    rdt.Add(dt);
                }
                else if (p != null)
                {
                    p.AssociatedObject = rdt.AssociatedObject;
                    rdt.Add(p);
                }
            }
            return rdt;
        }
    }
}