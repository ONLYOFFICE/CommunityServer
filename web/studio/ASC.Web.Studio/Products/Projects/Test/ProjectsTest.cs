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
    using System;
    using System.Collections.Generic;
    using ASC.Core;
    using NUnit.Framework;

    [TestFixture]
    public class ProjectsTest : BaseTest
    {
        [Test]
        public void Project()
        {
            var newProject = GenerateProject(SecurityContext.CurrentAccount.ID);

            SaveOrUpdate(newProject);

            Assert.AreNotEqual(newProject.ID, 0);

            var result = Get(newProject);

            Assert.AreEqual(newProject.ID, result.ID);

            newProject.Title = "NewTitle";
            newProject.Private = true;

            SaveOrUpdate(newProject);

            var updatedProject = Get(newProject);

            Assert.AreEqual(updatedProject.Title, newProject.Title);
            Assert.AreEqual(updatedProject.Private, newProject.Private);

            var team = new List<Guid>(4)
            {
                Owner,
                Admin,
                UserInTeam,
                Guest
            };

            AddTeamToProject(newProject, team);

            var getTeam = GetTeam(newProject.ID);
            CollectionAssert.AreEquivalent(team, getTeam);

            Delete(newProject);
            var deletedProject = Get(newProject);

            Assert.IsNull(deletedProject);
        }
    }
}