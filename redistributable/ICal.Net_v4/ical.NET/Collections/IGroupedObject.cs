namespace Ical.Net.Collections
{
    public interface IGroupedObject<TGroup>
    {
        TGroup Group { get; set; }
    }
}
