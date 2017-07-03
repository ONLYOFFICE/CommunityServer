namespace Ical.Net.Interfaces.DataTypes
{
    public interface IFreeBusyEntry : IPeriod
    {
        FreeBusyStatus Status { get; set; }
    }
}