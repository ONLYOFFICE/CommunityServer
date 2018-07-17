namespace ASC.Web.Projects.Test
{
    using NUnit.Framework;

    [TestFixture]
    public class TimeTrackingTest : BaseTest
    {
        [Test]
        public void TimeTracking()
        {
            var newTime = GenerateTimeTracking();

            SaveOrUpdate(newTime);

            Assert.AreNotEqual(newTime.ID, 0);


            var result = Get(newTime);

            Assert.AreEqual(newTime.ID, result.ID);


            newTime.Note = "NewTitle";

            SaveOrUpdate(newTime);

            var updatedTime = Get(newTime);

            Assert.AreEqual(updatedTime.Note, newTime.Note);


            Delete(newTime);

            var deletedTime = Get(newTime);

            Assert.IsNull(deletedTime);
        }
    }
}