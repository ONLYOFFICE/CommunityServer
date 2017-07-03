namespace Ical.Net.Interfaces.Components
{
    public interface IJournal : IRecurringComponent
    {
        JournalStatus Status { get; set; }
    }
}