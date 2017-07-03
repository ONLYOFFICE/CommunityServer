using System;

namespace Ical.Net.Interfaces.DataTypes
{
    public interface IOrganizer : IEncodableDataType
    {
        Uri SentBy { get; set; }
        string CommonName { get; set; }
        Uri DirectoryEntry { get; set; }
        Uri Value { get; set; }
    }
}