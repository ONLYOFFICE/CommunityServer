using System;
using Ical.Net.Interfaces.General;
using IServiceProvider = Ical.Net.Interfaces.General.IServiceProvider;

namespace Ical.Net.Interfaces.DataTypes
{
    public interface ICalendarDataType : ICalendarParameterCollectionContainer, ICopyable, IServiceProvider
    {
        Type GetValueType();
        void SetValueType(string type);
        ICalendarObject AssociatedObject { get; set; }
        ICalendar Calendar { get; }

        string Language { get; set; }
    }
}