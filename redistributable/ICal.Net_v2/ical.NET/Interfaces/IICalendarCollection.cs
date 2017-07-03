using System.Collections.Generic;
using Ical.Net.Interfaces.Evaluation;

namespace Ical.Net.Interfaces
{
    public interface IICalendarCollection : IGetOccurrencesTyped, IList<ICalendar> {}
}