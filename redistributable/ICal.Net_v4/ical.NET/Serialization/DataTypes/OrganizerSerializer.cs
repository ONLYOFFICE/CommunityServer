using System;
using System.IO;
using Ical.Net.DataTypes;

namespace Ical.Net.Serialization.DataTypes
{
    public class OrganizerSerializer : StringSerializer
    {
        public OrganizerSerializer() { }

        public OrganizerSerializer(SerializationContext ctx) : base(ctx) { }

        public override Type TargetType => typeof (Organizer);

        public override string SerializeToString(object obj)
        {
            try
            {
                var o = obj as Organizer;
                return o?.Value == null
                    ? null
                    : Encode(o, Escape(o.Value.OriginalString));
            }
            catch
            {
                return null;
            }
        }

        public override object Deserialize(TextReader tr)
        {
            var value = tr.ReadToEnd();

            Organizer o = null;
            try
            {
                o = CreateAndAssociate() as Organizer;
                if (o != null)
                {
                    var uriString = Unescape(Decode(o, value));

                    // Prepend "mailto:" if necessary
                    if (!uriString.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
                    {
                        uriString = "mailto:" + uriString;
                    }

                    o.Value = new Uri(uriString);
                }
            }
            catch {}

            return o;
        }
    }
}