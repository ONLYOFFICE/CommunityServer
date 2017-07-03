using Ical.Net.Interfaces.DataTypes;

namespace Ical.Net.DataTypes
{
    /// <summary>
    /// An abstract class from which all iCalendar data types inherit.
    /// </summary>
    public class EncodableDataType : CalendarDataType, IEncodableDataType
    {
        public virtual string Encoding
        {
            get { return Parameters.Get("ENCODING"); }
            set { Parameters.Set("ENCODING", value); }
        }
    }
}