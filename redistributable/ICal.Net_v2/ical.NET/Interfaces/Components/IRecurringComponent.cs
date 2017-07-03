using System.Collections.Generic;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Interfaces.Evaluation;

namespace Ical.Net.Interfaces.Components
{
    public interface IRecurringComponent : IUniqueComponent, IRecurrable, IAlarmContainer
    {
        IList<IAttachment> Attachments { get; set; }
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