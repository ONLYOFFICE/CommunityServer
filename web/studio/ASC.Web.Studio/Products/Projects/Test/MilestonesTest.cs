namespace ASC.Web.Projects.Test
{
    using NUnit.Framework;

    [TestFixture]
    public class MilestonesTest : BaseTest
    {
        [Test]
        public void Milestone()
        {
            var newMilestone = GenerateMilestone();

            SaveOrUpdate(newMilestone);

            Assert.AreNotEqual(newMilestone.ID, 0);


            var result = Get(newMilestone);

            Assert.AreEqual(newMilestone.ID, result.ID);


            newMilestone.Title = "NewTitle";

            SaveOrUpdate(newMilestone);

            var updatedMilestone = Get(newMilestone);

            Assert.AreEqual(updatedMilestone.Title, newMilestone.Title);


            Delete(newMilestone);

            var deletedMilestone = Get(newMilestone);

            Assert.IsNull(deletedMilestone);
        }
    }
}