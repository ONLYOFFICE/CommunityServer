using System.Diagnostics;
using Ical.Net.CalendarComponents;
using Ical.Net.Serialization.DataTypes;

namespace Ical.Net.DataTypes
{
    /// <summary>
    /// A class that represents the geographical location of an
    /// <see cref="Components.Event"/> or <see cref="Todo"/> item.
    /// </summary>
    [DebuggerDisplay("{Latitude};{Longitude}")]
    public class GeographicLocation : EncodableDataType
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

        public override string ToString() => Latitude.ToString("0.000000") + ";" + Longitude.ToString("0.000000");

        protected bool Equals(GeographicLocation other) => Latitude.Equals(other.Latitude) && Longitude.Equals(other.Longitude);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((GeographicLocation)obj);
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