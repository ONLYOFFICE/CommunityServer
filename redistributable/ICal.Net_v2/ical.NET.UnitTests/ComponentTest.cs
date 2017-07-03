using NUnit.Framework;

namespace Ical.Net.UnitTests
{
    [TestFixture]
    public class ComponentTest
    {
        [Test, Category("Component")]
        public void UniqueComponent1()
        {
            var iCal = new Calendar();
            var evt = iCal.Create<Event>();

            Assert.IsNotNull(evt.Uid);
            Assert.IsNull(evt.Created); // We don't want this to be set automatically
            Assert.IsNotNull(evt.DtStamp);
        }
    }
}
