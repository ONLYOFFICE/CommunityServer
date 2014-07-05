using System.Threading;
using NUnit.Framework;

namespace BasecampRestAPI.Test
{
    [TestFixture]
    public class Test
    {
        private BaseCamp _baseCamp;

        [SetUp]
        public void Setup()
        {
            _baseCamp = BaseCamp.GetInstance(@"https://omts.basecamphq.com", "d5d07ef81cc0225521a73075edefb32933f15071", "X");
        }


        [Test]
        public void ProjectsTest()
        {
            Assert.AreNotEqual(_baseCamp.Projects.Length, 0);
        }

        [Test]
        public void GeneretateRateLimiting()
        {
            for (int i = 0; i < 600; i++)
            {
                ThreadPool.QueueUserWorkItem(x =>
                                                 {
                                                     Assert.AreNotEqual(_baseCamp.Projects.Length, 0);
                                                     Assert.AreNotEqual(_baseCamp.People.Length, 0);
                                                 });
            }
            //wait
            Thread.Sleep(300000);//200 sec. it will be enough
        }


    }
}