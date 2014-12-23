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

using ASC.Core;
using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Core.Domain;
using ASC.Projects.Data;
using log4net;
using NUnit.Framework;

#endregion

namespace ASC.Projects.Tests.Data
{
    public class ProjectChangeRequestTest : TestBase
    {

        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void AcceptProjectChangeRequest()
        {
            IDaoFactory daoFactory = new DaoFactory("projects", 0);

            IProjectDao _projectDao = daoFactory.GetProjectDao();
            IProjectChangeRequestDao _projectChangeRequestDao = daoFactory.GetProjectChangeRequestDao();

            ProjectChangeRequest projectChangeRequest = daoFactory.GetProjectChangeRequestDao().GetById(48);

            Project project = projectChangeRequest.RequestType == ProjectRequestType.Edit ? _projectDao.GetById(projectChangeRequest.ProjectID) : new Project();

            project.Title = projectChangeRequest.Title;
            project.Description = projectChangeRequest.Description;
            project.Responsible = projectChangeRequest.Responsible;
            project.Status = projectChangeRequest.Status;


            project = _projectDao.Save(project);
            daoFactory.GetProjectDao().AddToTeam(project.ID, project.Responsible);
            _projectChangeRequestDao.Delete(projectChangeRequest.ID);
        }

        [Test]
        public void SaveOrUpdateTest()
        {
            IDaoFactory daoFactory = new DaoFactory("projects", 0);

            ProjectChangeRequest projectChangeRequest = new ProjectChangeRequest();

            projectChangeRequest.ProjectID = 10;
            projectChangeRequest.RequestType = ProjectRequestType.Create;
            projectChangeRequest.Status = ProjectStatus.Open;
            projectChangeRequest.Description = "asdf";
            projectChangeRequest.Title = "New Project 123";
            projectChangeRequest.Responsible = SecurityContext.CurrentAccount.ID;
            //  projectChangeRequest.CreateBy = new Participant(ASC.Core.SecurityContext.CurrentAccount.ID);

            daoFactory.GetProjectChangeRequestDao().Save(projectChangeRequest);
        }
    }
}
