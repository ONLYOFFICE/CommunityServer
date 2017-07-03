using Ical.Net.DataTypes;
using Ical.Net.Interfaces;
using Ical.Net.Interfaces.Components;
using NUnit.Framework;

namespace Ical.Net.UnitTests
{
    [TestFixture]
    public class FreeBusyTest
    {
        /// <summary>
        /// Ensures that GetFreeBusyStatus() return the correct status.
        /// </summary>
        [Test, Category("FreeBusy")]
        public void GetFreeBusyStatus1()
        {
            ICalendar cal = new Calendar();

            IEvent evt = cal.Create<Event>();
            evt.Summary = "Test event";
            evt.Start = new CalDateTime(2010, 10, 1, 8, 0, 0);
            evt.End = new CalDateTime(2010, 10, 1, 9, 0, 0);

            var freeBusy = cal.GetFreeBusy(new CalDateTime(2010, 10, 1, 0, 0, 0), new CalDateTime(2010, 10, 7, 11, 59, 59));
            Assert.AreEqual(FreeBusyStatus.Free, freeBusy.GetFreeBusyStatus(new CalDateTime(2010, 10, 1, 7, 59, 59)));
            Assert.AreEqual(FreeBusyStatus.Busy, freeBusy.GetFreeBusyStatus(new CalDateTime(2010, 10, 1, 8, 0, 0)));
            Assert.AreEqual(FreeBusyStatus.Busy, freeBusy.GetFreeBusyStatus(new CalDateTime(2010, 10, 1, 8, 59, 59)));
            Assert.AreEqual(FreeBusyStatus.Free, freeBusy.GetFreeBusyStatus(new CalDateTime(2010, 10, 1, 9, 0, 0)));
        }
    }
}
