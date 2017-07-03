using System;
using System.Collections.Generic;
using Ical.Net.Interfaces.DataTypes;

namespace Ical.Net.Interfaces.Components
{
    public interface IUniqueComponent : ICalendarComponent
    {
        string Uid { get; set; }

        IList<IAttendee> Attendees { get; set; }
        IList<string> Comments { get; set; }
        IDateTime DtStamp { get; set; }
        IOrganizer Organizer { get; set; }
        IList<IRequestStatus> RequestStatuses { get; set; }
        Uri Url { get; set; }
    }
}