namespace Ical.Net.Collections.Interfaces
{
    public interface IGroupedObject<TGroup>
    {
        TGroup Group { get; set; }
    }
}
