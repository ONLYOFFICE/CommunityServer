using System;
using System.Linq;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using NUnit.Framework;

namespace Ical.Net.FrameworkUnitTests
{
    [TestFixture]
    public class CalendarPropertiesTest
    {
        [Test]
        public void AddPropertyShouldNotIncludePropertyNameInValue()
        {
            const string propName = "X-WR-CALNAME";
            const string propValue = "Testname";

            var iCal = new Calendar();
            iCal.AddProperty(propName, propValue);

            var result = new CalendarSerializer().SerializeToString(iCal);

            var lines = result.Split(new [] { SerializationConstants.LineBreak }, StringSplitOptions.None);
            var propLine = lines.FirstOrDefault(x => x.StartsWith("X-WR-CALNAME:"));
            Assert.AreEqual($"{propName}:{propValue}", propLine);
        }

        [Test]
        [Ignore("Calendar properties aren't being properly serialized")]
        public void PropertySerialization_Tests()
        {
            const string formatted =
@"FMTTYPE=text/html:<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 3.2//EN"">\n<HTML>\n<HEAD>\n<META NAME=""Generator"" CONTENT=""MS Exchange Server version rmj.rmm.rup.rpr"">\n<TITLE></TITLE>\n</HEAD>\n<BODY>\n<!-- Converted from text/rtf format -->\n\n<P DIR=LTR><SPAN LANG=""en-us""><FONT FACE=""Calibri"">This is some</FONT></SPAN><SPAN LANG=""en-us""><B> <FONT FACE=""Calibri"">HTML</FONT></B></SPAN><SPAN LANG=""en-us""><FONT FACE=""Calibri""></FONT></SPAN><SPAN LANG=""en-us""><U> <FONT FACE=""Calibri"">formatted</FONT></U></SPAN><SPAN LANG=""en-us""><FONT FACE=""Calibri""></FONT></SPAN><SPAN LANG=""en-us""><I> <FONT FACE=""Calibri"">text</FONT></I></SPAN><SPAN LANG=""en-us""><FONT FACE=""Calibri"">.</FONT></SPAN><SPAN LANG=""en-us""></SPAN></P>\n\n</BODY>\n</HTML>";

            var start = DateTime.Now;
            var end = start.AddHours(1);
            var @event = new CalendarEvent
            {
                Start = new CalDateTime(start),
                End = new CalDateTime(end),
                Description = "This is a description",
            };
            var property = new CalendarProperty("X-ALT-DESC", formatted);
            @event.AddProperty(property);
            var calendar = new Calendar();
            calendar.Events.Add(@event);

            var serialized = new CalendarSerializer().SerializeToString(calendar);
            Assert.IsTrue(serialized.Contains("X-ALT-DESC;"));
        }

        [Test]
        public void PropertySetValueMustAllowNull()
        {
            var property = new CalendarProperty();
            Assert.DoesNotThrow(() => property.SetValue(null));
        }
    }
}
