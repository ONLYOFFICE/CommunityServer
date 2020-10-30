/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


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