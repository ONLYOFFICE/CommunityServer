namespace ASC.Web.Projects.Test
{
    using NUnit.Framework;

    [TestFixture]
    public class TasksTest : BaseTest
    {
        [Test]
        public void Task()
        {
            var newTask = GenerateTask();

            SaveOrUpdate(newTask);

            Assert.AreNotEqual(newTask.ID, 0);


            var result = Get(newTask);

            Assert.AreEqual(newTask.ID, result.ID);


            newTask.Title = "NewTitle";

            SaveOrUpdate(newTask);

            var updatedTask = Get(newTask);

            Assert.AreEqual(updatedTask.Title, newTask.Title);


            Delete(newTask);

            var deletedTask = Get(newTask);

            Assert.IsNull(deletedTask);
        }
    }
}