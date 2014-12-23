/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

#region Import

using System;
using System.Collections.Generic;
using System.Reflection;
using ASC.Core;
using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Core.Domain;
using ASC.Projects.Core.Domain.Reports;
using ASC.Projects.Data;
using log4net;
using NUnit.Framework;

#endregion

namespace ASC.Projects.Tests.Data
{
    [TestFixture]
    public class Projects : TestBase
    {

        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void SaveOrUpdateTest()
        {
            IDaoFactory daoFactory = new DaoFactory("projects", 0);

            Project project = daoFactory.GetProjectDao().GetById(10);

            Console.WriteLine(daoFactory.GetProjectDao().GetTeam(project.ID).Length);

            daoFactory.GetProjectDao().AddToTeam(project.ID, new Guid("777fc1c2-b444-4303-9d71-f8766796e4b4"));

            Console.WriteLine(daoFactory.GetProjectDao().GetTeam(project.ID).Length);

        }

        /// <summary>
        ///    project_id, project_title, project_leader, project_status,milestone_count, task_count, participian_count
        /// </summary>
        /// <param name="projectStatus"></param>
        /// <returns></returns>
        public void BuildProjectListReport()
        {
            ASC.Core.CoreContext.TenantManager.SetCurrentTenant(0);
            IDaoFactory daoFactory = new DaoFactory("projects", 0);

            ///  IList queryResult = daoFactory.GetProjectDao().BuildProjectListReport(null);

            // Console.WriteLine(queryResult.Count);


        }

        public void BuildProjectWithoutOpenMilestone()
        {


        }

        [Test]
        public void BuildProjectWithoutOpenTask()
        {
            ASC.Core.CoreContext.TenantManager.SetCurrentTenant(0);
            IDaoFactory daoFactory = new DaoFactory("projects", 0);

            Console.WriteLine(daoFactory.GetReportDao().BuildProjectWithoutOpenMilestone(new ReportFilter()).Count);

        }

        [Test]
        public void SaveOrUpdateTest123()
        {
            IDaoFactory daoFactory = new DaoFactory("projects", 0);

            Project newProject = new Project();

            newProject.Title = "Test project 2";
            newProject.Description = "Description";
            newProject.Responsible = SecurityContext.CurrentAccount.ID;

            daoFactory.GetProjectDao().Save(newProject);
            daoFactory.GetProjectDao().AddToTeam(newProject.ID, SecurityContext.CurrentAccount.ID);
            Console.WriteLine(newProject.ID);

        }

        [Test]
        public void LoadProject()
        {
            IDaoFactory daoFactory = new DaoFactory("projects", 0);

            Console.WriteLine(daoFactory.GetProjectDao().GetTeam(15).Length);


        }

        [Test]
        public void GetTaskCount()
        {
            IDaoFactory daoFactory = new DaoFactory("projects", 0);

            Console.WriteLine(daoFactory.GetProjectDao().GetTaskCount(new List<int>(new[] { 1 }), TaskStatus.Open, TaskStatus.NotAccept, TaskStatus.Closed));
        }

        [Test]
        public void AddProjectTags()
        {
            IDaoFactory daoFactory = new DaoFactory("projects", 0);

            Project project = daoFactory.GetProjectDao().GetById(11);

            var tags = daoFactory.GetTagDao().GetProjectTags(project.ID);

            Console.WriteLine(tags.Length);
        }


        [Test]
        public void GetProjectTags()
        {
            IDaoFactory daoFactory = new DaoFactory("projects", 0);

            Project project = daoFactory.GetProjectDao().GetById(8);

            var tags = daoFactory.GetTagDao().GetProjectTags(project.ID);
            Console.WriteLine(tags.Length);
        }
    }
}
