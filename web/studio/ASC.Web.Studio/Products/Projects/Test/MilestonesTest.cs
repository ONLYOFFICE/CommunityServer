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