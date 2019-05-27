using System.Collections.Generic;
using Ical.Net.DataTypes;

namespace Ical.Net.CalendarComponents
{
    public interface IRecurringComponent : IUniqueComponent, IRecurrable
    {
        IList<Attachment> Attachments { get; set; }
        IList<string> Categories { get; set; }
        string Class { get; set; }
        IList<string> Contacts { get; set; }
        IDateTime Created { get; set; }
        string Description { get; set; }
        IDateTime LastModified { get; set; }
        int Priority { get; set; }
        IList<string> RelatedComponents { get; set; }
        int Sequence { get; set; }
        string Summary { get; set; }
    }
}