namespace Ical.Net.Interfaces.DataTypes
{
    public interface IRequestStatus : IEncodableDataType
    {
        string Description { get; set; }
        string ExtraData { get; set; }
        IStatusCode StatusCode { get; set; }
    }
}