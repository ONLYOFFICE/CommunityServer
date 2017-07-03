namespace ASC.Web.Projects.Test
{
    using NUnit.Framework;

    [TestFixture]
    public class DiscussionTest : BaseTest
    {
        [Test]
        public void Discussion()
        {
            var newMessage = GenerateMessage();

            SaveOrUpdate(newMessage);

            Assert.AreNotEqual(newMessage.ID, 0);


            var result = Get(newMessage);

            Assert.AreEqual(newMessage.ID, result.ID);


            newMessage.Title = "NewTitle";

            SaveOrUpdate(newMessage);

            var message = Get(newMessage);

            Assert.AreEqual(message.Title, newMessage.Title);


            Delete(newMessage);

            var deletedMessage = Get(newMessage);

            Assert.IsNull(deletedMessage);
        }
    }
}