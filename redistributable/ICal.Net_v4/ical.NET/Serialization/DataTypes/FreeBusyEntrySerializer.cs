using System;
using System.IO;
using Ical.Net.DataTypes;

namespace Ical.Net.Serialization.DataTypes
{
    public class FreeBusyEntrySerializer : PeriodSerializer
    {
        public FreeBusyEntrySerializer() { }

        public FreeBusyEntrySerializer(SerializationContext ctx) : base(ctx) { }

        public override Type TargetType => typeof (FreeBusyEntry);

        public override string SerializeToString(object obj)
        {
            var entry = obj as FreeBusyEntry;
            if (entry == null)
            {
                return base.SerializeToString(obj);
            }

            switch (entry.Status)
            {
                case FreeBusyStatus.Busy:
                    entry.Parameters.Remove("FBTYPE");
                    break;
                case FreeBusyStatus.BusyTentative:
                    entry.Parameters.Set("FBTYPE", "BUSY-TENTATIVE");
                    break;
                case FreeBusyStatus.BusyUnavailable:
                    entry.Parameters.Set("FBTYPE", "BUSY-UNAVAILABLE");
                    break;
                case FreeBusyStatus.Free:
                    entry.Parameters.Set("FBTYPE", "FREE");
                    break;
            }

            return base.SerializeToString(obj);
        }

        public override object Deserialize(TextReader tr)
        {
            var entry = base.Deserialize(tr) as FreeBusyEntry;
            if (entry == null)
            {
                return entry;
            }

            if (!entry.Parameters.ContainsKey("FBTYPE"))
            {
                return entry;
            }

            var value = entry.Parameters.Get("FBTYPE");
            if (value == null)
            {
                return entry;
            }

            switch (value.ToUpperInvariant())
            {
                case "FREE":
                    entry.Status = FreeBusyStatus.Free;
                    break;
                case "BUSY":
                    entry.Status = FreeBusyStatus.Busy;
                    break;
                case "BUSY-UNAVAILABLE":
                    entry.Status = FreeBusyStatus.BusyUnavailable;
                    break;
                case "BUSY-TENTATIVE":
                    entry.Status = FreeBusyStatus.BusyTentative;
                    break;
            }

            return entry;
        }
    }
}