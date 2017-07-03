using System.Diagnostics;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Interfaces.General;
using Ical.Net.Serialization.iCalendar.Serializers.DataTypes;

namespace Ical.Net.DataTypes
{
    /// <summary>
    /// A class that represents the geographical location of an
    /// <see cref="Components.Event"/> or <see cref="Components.Todo"/> item.
    /// </summary>
    [DebuggerDisplay("{Latitude};{Longitude}")]
    public class GeographicLocation : EncodableDataType, IGeographicLocation
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public GeographicLocation() {}

        public GeographicLocation(string value) : this()
        {
            var serializer = new GeographicLocationSerializer();
            serializer.Deserialize(value);
        }

        public GeographicLocation(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public override void CopyFrom(ICopyable obj) {}

        public override string ToString()
        {
            return Latitude.ToString("0.000000") + ";" + Longitude.ToString("0.000000");
        }

        protected bool Equals(GeographicLocation other)
        {
            return Latitude.Equals(other.Latitude) && Longitude.Equals(other.Longitude);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((GeographicLocation)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Latitude.GetHashCode() * 397) ^ Longitude.GetHashCode();
            }
        }
    }
}