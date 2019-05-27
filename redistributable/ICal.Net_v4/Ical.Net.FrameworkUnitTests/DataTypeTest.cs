using Ical.Net.DataTypes;
using NUnit.Framework;

namespace Ical.Net.FrameworkUnitTests
{
    [TestFixture]
    public class DataTypeTest
    {
        [Test, Category("DataType")]
        public void OrganizerConstructorMustAcceptNull()
        {
            Assert.DoesNotThrow(() => { var o = new Organizer(null); });
        }

        [Test, Category("DataType")]
        public void AttachmentConstructorMustAcceptNull()
        {
            Assert.DoesNotThrow(() => { var o = new Attachment((byte[])null); });
            Assert.DoesNotThrow(() => { var o = new Attachment((string)null); });
        }
    }
}