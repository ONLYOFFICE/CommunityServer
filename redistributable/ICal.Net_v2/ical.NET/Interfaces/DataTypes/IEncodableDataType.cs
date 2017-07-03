namespace Ical.Net.Interfaces.DataTypes
{
    public interface IEncodableDataType : ICalendarDataType
    {
        string Encoding { get; set; }
    }
}