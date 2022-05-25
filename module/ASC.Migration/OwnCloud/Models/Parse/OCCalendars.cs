using System.Collections.Generic;

namespace ASC.Migration.OwnCloud.Models
{
    public class OCCalendars
    {
        public int Id { get; set; }
        public List<OCCalendarObjects> CalendarObject { get; set; }
        public string DisplayName { get; set; }
    }

    public class OCCalendarObjects
    {
        public int Id { get; set; }
        public byte[] CalendarData { get; set; }
    }
}
