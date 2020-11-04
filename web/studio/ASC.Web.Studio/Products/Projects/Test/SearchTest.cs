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


using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ASC.ElasticSearch;
using ASC.Projects.Core.Domain;
using ASC.Web.Projects.Core.Search;

namespace ASC.Web.Projects.Test
{
    using ASC.Core;
    using NUnit.Framework;

    [TestFixture]
    [TestOf(typeof(SearchTest))]
    public class SearchTest : BaseTest
    {
        private List<Project> Projects { get; set; }

        [OneTimeSetUp]
        public void Setup()
        {
            Projects = new List<Project>
            {
                CreateProject("Project for testing search фёдор", "Description"),
                CreateProject("Another Project for testing search фёдор", "Another Description")
            };
        }

        [TestCaseSource(typeof(SearchCaseData), "TestCases", new object[] { "Search Project all by " })]
        public void ProjectAll(string searchText)
        {
            List<int> result;
            FactoryIndexer<ProjectsWrapper>.TrySelectIds(s => s.MatchAll(searchText), out result);

            foreach (var prj in Projects)
            {
                CollectionAssert.Contains(result, prj.ID);
            }
        }

        //[Test]
        //public void ProjectSpecial()
        //{
        //    var projectsSpecial = new List<Project>
        //    {
        //        CreateProject("test@gmail.com", "abc"),
        //        CreateProject("test@mail.ru", "abc")
        //    };

        //    List<int> result;
        //    FactoryIndexer<ProjectsWrapper>.TrySelectIds(s => s.MatchAll("test@gmail.com"), out result);

        //    CollectionAssert.IsNotEmpty(result);
        //    Assert.AreEqual(result.Count, 1);
        //    Assert.AreEqual(result[0], projectsSpecial[0].ID);

        //    FactoryIndexer<ProjectsWrapper>.TrySelectIds(s => s.MatchAll("test"), out result);

        //    foreach (var prj in projectsSpecial)
        //    {
        //        CollectionAssert.Contains(result, prj.ID);
        //    }

        //    projectsSpecial.ForEach(DeleteProject);
        //}

        [TestCaseSource(typeof(SearchCaseData), "TestCases", new object[] { "Search Project all in " })]
        public void ProjectAllIn(string searchText)
        {
            List<int> result;
            var prj = Projects[0].ID;
            FactoryIndexer<ProjectsWrapper>.TrySelectIds(s => s.MatchAll(searchText).In(r => r.Id, new[] { prj }), out result);

            Assert.AreEqual(result[0], prj);

            var prj2 = Projects[1].ID;
            FactoryIndexer<ProjectsWrapper>.TrySelectIds(s => s.MatchAll(searchText).In(r => r.Id, new[] { prj, prj2 }), out result);

            Assert.AreEqual(result.Count, 2);
        }

        [TestCaseSource(typeof(SearchCaseData), "TestCases", new object[] { "Search Project multi by " })]
        public void ProjectMulti(string searchText)
        {
            List<int> result;
            FactoryIndexer<ProjectsWrapper>.TrySelectIds(s => s.Match(r => new object[] { r.Title, r.Description }, searchText), out result);

            foreach (var prj in Projects)
            {
                CollectionAssert.Contains(result, prj.ID);
            }
        }

        [TestCaseSource(typeof(SearchCaseData), "TestCases", new object[] { "Search Project or by " })]
        public void ProjectOr(string searchText)
        {
            List<int> result;
            FactoryIndexer<ProjectsWrapper>.TrySelectIds(s => s.Or(r => r.Match(k => k.Title, searchText), r => r.Match(k => k.Description, searchText)), out result);

            foreach (var prj in Projects)
            {
                CollectionAssert.Contains(result, prj.ID);
            }
        }

        [TestCaseSource(typeof(SearchCaseData), "TestCases", new object[] { "Search Project or by " })]
        public void ProjectNot(string searchText)
        {
            List<int> result;
            FactoryIndexer<ProjectsWrapper>.TrySelectIds(s => s.Not(r => r.Match(k => k.Title, searchText + "1")).MatchAll(searchText), out result);

            foreach (var prj in Projects)
            {
                CollectionAssert.Contains(result, prj.ID);
            }
        }

        [Test]
        public void ProjectMultiProps()
        {
            List<int> result;
            FactoryIndexer<ProjectsWrapper>.TrySelectIds(s => s.Match(r => r.Title, "Project").Match(r=> r.Description, "Description"), out result);

            foreach (var prj in Projects)
            {
                CollectionAssert.Contains(result, prj.ID);
            }
        }

        [TestCaseSource(typeof(SearchCaseData), "WildCardTestCases", new object[] { "Search Project wildcard by " })]
        public void ProjectWildCard(string searchText)
        {
            List<int> result;
            FactoryIndexer<ProjectsWrapper>.TrySelectIds(s => s.MatchAll(searchText), out result);

            foreach (var prj in Projects)
            {
                CollectionAssert.Contains(result, prj.ID);
            }

            FactoryIndexer<ProjectsWrapper>.TrySelectIds(s => s.Or(r => r.Match(a => a.Title, searchText), r => r.Match(a => a.Description, searchText)), out result);

            foreach (var prj in Projects)
            {
                CollectionAssert.Contains(result, prj.ID);
            }

        }

        [TestCaseSource(typeof(SearchCaseData), "TestCases", new object[] { "Search Project exactly by " })]
        public void ProjectExactly(string searchText)
        {
            List<int> result;
            FactoryIndexer<ProjectsWrapper>.TrySelectIds(s => s.Or(r => r.Match(a => a.Title, "\"" + searchText + "\""), r => r.Match(a => a.Description, "\"" + searchText + "\"")), out result);

            foreach (var prj in Projects)
            {
                CollectionAssert.Contains(result, prj.ID);
            }

            FactoryIndexer<ProjectsWrapper>.TrySelectIds(s => s.MatchAll("\"" + searchText + "\""), out result);

            foreach (var prj in Projects)
            {
                CollectionAssert.Contains(result, prj.ID);
            }
        }

        [Test]
        public void ProjectPhrase()
        {
            List<int> result;
            FactoryIndexer<ProjectsWrapper>.TrySelectIds(s => s.Match(r => r.Title, "\"Project for\""), out result);

            foreach (var prj in Projects)
            {
                CollectionAssert.Contains(result, prj.ID);
            }

            FactoryIndexer<ProjectsWrapper>.TrySelectIds(s => s.Match(r => r.Title, "\"for search\""), out result);
            CollectionAssert.IsEmpty(result);

            FactoryIndexer<ProjectsWrapper>.TrySelectIds(s => s.Match(r => r.Title, "for search"), out result);
            foreach (var prj in Projects)
            {
                CollectionAssert.Contains(result, prj.ID);
            }
        }

        //[Test]
        //public void ProjectTag()
        //{
        //    var proj = CreateProject("TQDAZCBGT", "");

        //    TagEngine.SetProjectTags(proj.ID, "tag123,tag1234");

        //    var tag = TagEngine.GetProjectTags(proj.ID);

        //    FactoryIndexer<ProjectsWrapper>.Update(new ProjectsWrapper
        //    {
        //        Id = proj.ID,
        //        TagsWrapper = tag.Keys.Select(r => new TagsWrapper
        //        {
        //            TagId = r
        //        }).ToList()
        //    }, true, r => r.TagsWrapper);

        //    var proj1 = CreateProject("UYTRHGF", "");

        //    TagEngine.SetProjectTags(proj1.ID, "tag123");

        //    tag = TagEngine.GetProjectTags(proj1.ID);

        //    FactoryIndexer<ProjectsWrapper>.Update(new ProjectsWrapper
        //    {
        //        Id = proj1.ID,
        //        TagsWrapper = tag.Keys.Select(r => new TagsWrapper
        //        {
        //            TagId = r
        //        }).ToList()
        //    }, true, r => r.TagsWrapper);

        //    var proj2 = CreateProject("RFVTGB", "");

        //    TagEngine.SetProjectTags(proj2.ID, "tag5432");

        //    tag = TagEngine.GetProjectTags(proj2.ID);

        //    FactoryIndexer<ProjectsWrapper>.Update(new ProjectsWrapper
        //    {
        //        Id = proj2.ID,
        //        TagsWrapper = tag.Keys.Select(r => new TagsWrapper
        //        {
        //            TagId = r
        //        }).ToList()
        //    }, true, r => r.TagsWrapper);

        //    FactoryIndexer<ProjectsWrapper>.Update(new ProjectsWrapper
        //    {
        //        TagsWrapper = tag.Keys.Select(r => new TagsWrapper
        //        {
        //            TagId = r
        //        }).ToList()
        //    },
        //    r => r.In(a => a.Id, new[] { proj.ID, proj1.ID }),
        //    UpdateAction.Add,
        //    r => r.TagsWrapper);

        //    List<int> result;
        //    //FactoryIndexer<ProjectsWrapper>.TrySelectIds(s => s.Nested(w=> w.In(r => r.TagsWrapper.Select(t=> t.TagId), new[] {tag.Keys.First() })), out result);
        //    FactoryIndexer<ProjectsWrapper>.TrySelectIds(s => s.In(r => r.TagsWrapper.Select(t => t.TagId), new[] { tag.Keys.First() }), out result);

        //    CollectionAssert.IsNotEmpty(result);
        //    Assert.AreEqual(result.Count, 3);

        //    tag = TagEngine.GetProjectTags(proj.ID);

        //    FactoryIndexer<ProjectsWrapper>.TrySelectIds(s => s.InAll(r => r.TagsWrapper.Select(b => b.TagId), tag.Select(r => r.Key).ToArray()), out result);

        //    CollectionAssert.IsNotEmpty(result);
        //    Assert.AreEqual(result.Count, 1);
        //    Assert.AreEqual(proj.ID, result[0]);

        //    DeleteProject(proj);
        //    DeleteProject(proj1);
        //    DeleteProject(proj2);
        //}

        [TestCaseSource(typeof(SearchCaseData), "TestCases", new object[] { "Search Project sort " })]
        public void ProjectSort(string searchText)
        {
            List<int> result;
            FactoryIndexer<ProjectsWrapper>.TrySelectIds(s => s.MatchAll(searchText).Sort(a => a.Id, false), out result);

            Assert.Greater(result[0], result[1]);

            FactoryIndexer<ProjectsWrapper>.TrySelectIds(s => s.MatchAll(searchText).Sort(a => a.LastModifiedOn, true), out result);

            Assert.Greater(result[1], result[0]);
        }

        [Test(Description = "Search Project update ")]
        public void ProjectUpdate()
        {
            var searchText = "ABCDEF";
            var proj = CreateProject(searchText, "Description");

            List<int> result;
            FactoryIndexer<ProjectsWrapper>.TrySelectIds(s => s.MatchAll(searchText), out result);

            Assert.AreEqual(result[0], proj.ID);

            FactoryIndexer<ProjectsWrapper>.Update(new ProjectsWrapper { Id = proj.ID, Title = "QWERTY" }, true, r => r.Title);
            FactoryIndexer<ProjectsWrapper>.Update(new ProjectsWrapper { Id = proj.ID, LastModifiedOn = DateTime.UtcNow }, true, r => r.LastModifiedOn);
            FactoryIndexer<ProjectsWrapper>.Update(new ProjectsWrapper { Id = proj.ID, TenantId = 10}, true, r => r.TenantId);
            FactoryIndexer<ProjectsWrapper>.Update(new ProjectsWrapper { Id = proj.ID, TenantId = 5, Title = "Tenant" }, true, r => r.TenantId, r => r.Title);

            FactoryIndexer<ProjectsWrapper>.TrySelectIds(s => s.MatchAll(searchText), out result);
            CollectionAssert.IsEmpty(result);

            DeleteProject(proj);
        }

        [Test(Description = "Search Project update by query")]
        public void ProjectUpdateByQuery()
        {
            var searchText = "ABCDEF";
            var proj1 = CreateProject(searchText, "Description");
            var proj2 = CreateProject(searchText, "Description");
            var proj3 = CreateProject(searchText, "Description");

            List<int> result;
            FactoryIndexer<ProjectsWrapper>.TrySelectIds(s => s.MatchAll(searchText), out result);

            Assert.AreEqual(result.Count, 3);

            var newTitle = "QWERTY";
            FactoryIndexer<ProjectsWrapper>.Update(new ProjectsWrapper { Title = "QWERTY" }, r=> r.In(t => t.Id, new [] { proj1.ID, proj2.ID}),true, r => r.Title);

            FactoryIndexer<ProjectsWrapper>.TrySelectIds(s => s.MatchAll(newTitle), out result);
            Assert.AreEqual(result.Count, 2);

            FactoryIndexer<ProjectsWrapper>.TrySelectIds(s => s.MatchAll(searchText), out result);
            Assert.AreEqual(result[0], proj3.ID);

            DeleteProject(proj1);
            DeleteProject(proj2);
            DeleteProject(proj3);
        }

        //[Test(Description = "Delete by id ")]
        //public async System.Threading.Tasks.Task ProjectDeleteById()
        //{
        //    var searchText = "ABCDEF";
        //    var proj = CreateProject(searchText, "Description");

        //    List<int> result;
        //    var tenant = CoreContext.TenantManager.GetCurrentTenant();
        //    await FactoryIndexer<ProjectsWrapper>.DeleteAsync(proj.ID);
        //    FactoryIndexer<ProjectsWrapper>.Flush();
        //    await System.Threading.Tasks.Task.Delay(1000);

        //    CoreContext.TenantManager.SetCurrentTenant(tenant.TenantId);
        //    SecurityContext.AuthenticateMe(tenant.OwnerId);

        //    FactoryIndexer<ProjectsWrapper>.TrySelectIds(s => s.MatchAll(searchText), out result);
        //    CollectionAssert.IsEmpty(result);

        //    Delete(proj);

        //    FactoryIndexer<ProjectsWrapper>.TrySelectIds(s => s.MatchAll("Another"), out result);
        //    CollectionAssert.IsNotEmpty(result);
        //}

        [Test(Description = "Delete by query ")]
        public void ProjectDeleteByQuery()
        {
            var searchText = "QAZWSX";
            var proj = CreateProject(searchText, "Description");

            List<int> result;
            var tenant = CoreContext.TenantManager.GetCurrentTenant();
            FactoryIndexer<ProjectsWrapper>.Delete(r => r.Match(a => a.Title, searchText));

            CoreContext.TenantManager.SetCurrentTenant(tenant.TenantId);
            SecurityContext.AuthenticateMe(tenant.OwnerId);

            FactoryIndexer<ProjectsWrapper>.TrySelectIds(s => s.MatchAll(searchText), out result);
            CollectionAssert.IsEmpty(result);

            Delete(proj);

            FactoryIndexer<ProjectsWrapper>.TrySelectIds(s => s.MatchAll("Another"), out result);
            CollectionAssert.IsNotEmpty(result);
        }

        [Test(Description = "Delete ")]
        public void ProjectDelete()
        {
            var searchText = "WERTYU";
            var proj = CreateProject(searchText, "Description");

            List<int> result;

            FactoryIndexer<ProjectsWrapper>.TrySelectIds(s => s.MatchAll(searchText), out result);
            CollectionAssert.IsNotEmpty(result);

            DeleteProject(proj);

            FactoryIndexer<ProjectsWrapper>.TrySelectIds(s => s.MatchAll(searchText), out result);
            CollectionAssert.IsEmpty(result);
        }

        [TestCaseSource(typeof(SearchCaseData), "SpecTestCases", new object[] { "Search Project spec " })]
        public void ProjectSpec(string spec)
        {
            var searchText = "ABC" + spec + "BCD";
            var proj = CreateProject(searchText, "Description");

            List<int> result;

            FactoryIndexer<ProjectsWrapper>.TrySelectIds(s => s.MatchAll(searchText), out result);
            CollectionAssert.IsNotEmpty(result);
            Assert.AreEqual(proj.ID, result[0]);

            DeleteProject(proj);
        }

        //[Test(Description = "Subtask ")]
        //public void TaskWithSubstasks()
        //{
        //    var searchText = "qazwsx";
        //    var task = CreateTask(searchText, "Description");

        //    CreateTask("subtask", "Description");
        //    task.SubTasks = new List<Subtask>();
        //    for (var i = 0; i < 3; i++)
        //    {
        //        task.SubTasks.Add(SubtaskEngine.SaveOrUpdate(new Subtask {Title = "subtask" + i}, task));
        //    }

        //    FactoryIndexer<TasksWrapper>.Update(task, true, r=> r.Subtasks);

        //    List<int> result;

        //    FactoryIndexer<TasksWrapper>.TrySelectIds(s => s.Match(r=> r.Subtasks.Select(q=> q.Title) ,"subtask"), out result);
        //    CollectionAssert.IsNotEmpty(result);


        //    FactoryIndexer<TasksWrapper>.TrySelectIds(s => s.MatchAll("subtask"), out result);
        //    CollectionAssert.IsNotEmpty(result);
        //}

        [OneTimeTearDown]
        public void TearDown()
        {
            foreach (var project in Projects)
            {
                DeleteProject(project);
            }
        }

        private Project CreateProject(string title, string description)
        {
            var newProject = GenerateProject(SecurityContext.CurrentAccount.ID);

            newProject.Title = title;
            newProject.Description = description;

            SaveOrUpdate(newProject);

            FactoryIndexer<ProjectsWrapper>.Index(newProject);

            return newProject;
        }

        private Task CreateTask(string title, string description)
        {
            var newTask = GenerateTask();

            newTask.Title = title;
            newTask.Description = description;

            SaveOrUpdate(newTask);

            FactoryIndexer<TasksWrapper>.Index(newTask);

            return newTask;
        }

        private void DeleteProject(Project project)
        {
            Delete(project);
            FactoryIndexer<ProjectsWrapper>.Delete(project);
        }
    }

    public class SearchCaseData
    {
        public static IEnumerable TestCases(string action)
        {
            yield return new TestCaseData("Project").SetName(action + " Project");
            yield return new TestCaseData("for").SetName(action + " for");
            yield return new TestCaseData("testing").SetName(action + " testing");
            yield return new TestCaseData("search").SetName(action + " search");
            yield return new TestCaseData("Description").SetName(action + " Description");
            yield return new TestCaseData("pRoJeCt").SetName(action + " pRoJeCt");
            yield return new TestCaseData("фёдор").SetName(action + " фёдор");
            yield return new TestCaseData("федор").SetName(action + " федор");
        }

        public static IEnumerable WildCardTestCases(string action)
        {
            yield return new TestCaseData("Pr*").SetName(action + " Project");
            yield return new TestCaseData("*Pr*").SetName(action + " *Pr*");
            yield return new TestCaseData("fo*").SetName(action + " for");
            yield return new TestCaseData("*esting").SetName(action + " testing");
            yield return new TestCaseData("*rch").SetName(action + " search");
            yield return new TestCaseData("Desc?iption").SetName(action + " Description");
        }
        public static IEnumerable SpecTestCases(string action)
        {
            yield return new TestCaseData("-").SetName(action + " -");
            yield return new TestCaseData("=").SetName(action + " =");
            yield return new TestCaseData("+").SetName(action + " +");
            yield return new TestCaseData(";").SetName(action + " ;");
            yield return new TestCaseData("/").SetName(action + " /");
            yield return new TestCaseData("\\").SetName(action + " \\");
            yield return new TestCaseData("|").SetName(action + " |");
            yield return new TestCaseData("№").SetName(action + " №");
            yield return new TestCaseData("&").SetName(action + " &");
            yield return new TestCaseData("#").SetName(action + " #");
            yield return new TestCaseData("^").SetName(action + " ^");
            yield return new TestCaseData("<>").SetName(action + " <>");
            yield return new TestCaseData("()").SetName(action + " ()");
            yield return new TestCaseData("[]").SetName(action + " []");
            yield return new TestCaseData("{}").SetName(action + " {}");
            yield return new TestCaseData("$").SetName(action + " $");
            yield return new TestCaseData("%").SetName(action + " %");
        }
    }
}