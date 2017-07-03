using System;
using System.IO;
using Ical.Net.DataTypes;
using Ical.Net.Interfaces.DataTypes;
using System.Globalization;
using Ical.Net.Interfaces.Serialization;

namespace Ical.Net.Serialization.iCalendar.Serializers.DataTypes
{
    public class GeographicLocationSerializer : EncodableDataTypeSerializer
    {
        public GeographicLocationSerializer() { }

        public GeographicLocationSerializer(ISerializationContext ctx) : base(ctx) { }

        public override Type TargetType => typeof (GeographicLocation);

        public override string SerializeToString(object obj)
        {
            var g = obj as IGeographicLocation;
            if (g == null)
            {
                return null;
            }

            var value = g.Latitude.ToString("0.000000", CultureInfo.InvariantCulture.NumberFormat) + ";"
                + g.Longitude.ToString("0.000000", CultureInfo.InvariantCulture.NumberFormat);
            return Encode(g, value);
        }

        public GeographicLocation Deserialize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var g = CreateAndAssociate() as GeographicLocation;
            if (g == null)
            {
                return null;
            }

            // Decode the value, if necessary!
            value = Decode(g, value);

            var values = value.Split(new [] {';'}, StringSplitOptions.RemoveEmptyEntries);
            if (values.Length != 2)
            {
                return null;
            }

            double lat;
            double lon;
            double.TryParse(values[0], NumberStyles.Any, CultureInfo.InvariantCulture, out lat);
            double.TryParse(values[1], NumberStyles.Any, CultureInfo.InvariantCulture, out lon);
            g.Latitude = lat;
            g.Longitude = lon;

            return g;
        }

        public override object Deserialize(TextReader tr) => Deserialize(tr.ReadToEnd());
    }
}