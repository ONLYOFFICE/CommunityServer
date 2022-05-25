using System.Collections.Generic;

namespace ASC.Migration.NextcloudWorkspace.Models
{
    public class NCCalendars
    {
        public int Id { get; set; }
        public List<NCCalendarObjects> CalendarObject { get; set; }
        public string DisplayName { get; set; }
    }

    public class NCCalendarObjects
    {
        public int Id { get; set; }
        public byte[] CalendarData { get; set; }
    }
}
