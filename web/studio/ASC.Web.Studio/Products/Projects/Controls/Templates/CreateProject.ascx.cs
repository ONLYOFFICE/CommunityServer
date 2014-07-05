/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using ASC.Core.Users;
using ASC.MessagingSystem;
using ASC.Web.Studio.Controls.Users;
using ASC.Web.Studio.Utility;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Resources;
using Newtonsoft.Json;

namespace ASC.Web.Projects.Controls.Templates
{
    public partial class CreateProject : BaseUserControl
    {
        protected bool IsAdmin
        {
            get { return Page.Participant.IsAdmin; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            _attantion.Options.IsPopup = true;

            if (Request["project"] != null)
            {
                var newProjectId = Create();
                Response.Clear();
                Response.ContentType = "text/html; charset=utf-8";
                Response.ContentEncoding = Encoding.UTF8;
                Response.Charset = Encoding.UTF8.WebName;
                Response.Write(newProjectId);
                Response.End();
            }

            LoadProjectManagerSelector();
            LoadProjectTeamSelector();

            Page.Title = HeaderStringHelper.GetPageTitle(ProjectTemplatesResource.CreateProjFromTmpl);
        }

        private void LoadProjectManagerSelector()
        {
            var projectManagerSelector = new AdvancedUserSelector
                {
                    ID = "projectManagerSelector",
                    DefaultGroupText = CustomResourceHelper.GetResource("EmployeeAllDepartments"),
                    EmployeeType = EmployeeType.User
                };
            projectManagerPlaceHolder.Controls.Add(projectManagerSelector);
        }

        private void LoadProjectTeamSelector()
        {
            var projectTeamSelector = (Studio.UserControls.Users.UserSelector)LoadControl(Studio.UserControls.Users.UserSelector.Location);
            projectTeamSelector.BehaviorID = "projectTeamSelector";
            projectTeamSelector.DisabledUsers.Add(new Guid());
            projectTeamSelector.Title = ProjectResource.ManagmentTeam;
            projectTeamSelector.SelectedUserListTitle = ProjectResource.Team;

            projectTeamPlaceHolder.Controls.Add(projectTeamSelector);
        }

        public int Create()
        {
            var newProject = Parser<Project>(Request["project"]);
            var team = new List<Guid>();
            var listMilestones = new List<Milestone>();
            var listTasks = new List<Task>();
            var notifyManager = Convert.ToBoolean(Request["notifyManager"]);
            var notifyResponsibles = Convert.ToBoolean(Request["notifyResponsibles"]);

            var projectEngine = Global.EngineFactory.GetProjectEngine();
            var participantEngine = Global.EngineFactory.GetParticipantEngine();
            var taskEngine = Global.EngineFactory.GetTaskEngine();
            var milestoneEngine = Global.EngineFactory.GetMilestoneEngine();

            if (Request["team"] != null)
            {
                team = Parser<List<Guid>>(Request["team"]);
            }

            if (Request["milestones"] != null)
            {
                listMilestones = Parser<List<Milestone>>(Request["milestones"]);
            }

            if (Request["noAssignTasks"] != null)
            {
                listTasks = Parser<List<Task>>(Request["noAssignTasks"]);
            }

            if (ProjectSecurity.CanCreateProject())
            {
                if (newProject != null)
                {
                    projectEngine.SaveOrUpdate(newProject, notifyManager);
                    projectEngine.AddToTeam(newProject, participantEngine.GetByID(newProject.Responsible), true);

                    //add team
                    foreach (var participant in team.Where(participant => participant != Guid.Empty))
                    {
                        projectEngine.AddToTeam(newProject, participantEngine.GetByID(participant), true);
                    }

                    foreach (var milestone in listMilestones)
                    {
                        var milestoneTasks = milestone.Tasks;
                        milestone.Description = string.Empty;
                        milestone.Project = newProject;
                        milestoneEngine.SaveOrUpdate(milestone, notifyResponsibles);

                        foreach (var task in milestoneTasks)
                        {
                            task.Description = string.Empty;
                            task.Status = TaskStatus.Open;
                            task.Milestone = milestone.ID;
                            task.Project = newProject;
                            taskEngine.SaveOrUpdate(task, null, notifyResponsibles);
                        }
                    }

                    //add no assign tasks

                    foreach (var task in listTasks)
                    {
                        task.Description = string.Empty;
                        task.Project = newProject;
                        task.Status = TaskStatus.Open;
                        taskEngine.SaveOrUpdate(task, null, notifyResponsibles);
                    }


                    var templateId = Request.QueryString["id"];
                    if (string.IsNullOrEmpty(templateId)) throw new ApplicationException("must be a template id in the page query string");

                    var id = Convert.ToInt32(templateId);
                    var template = Global.EngineFactory.GetTemplateEngine().GetByID(id);
                    MessageService.Send(HttpContext.Current.Request, MessageAction.ProjectCreatedFromTemplate, template.Title, newProject.Title);

                    return newProject.ID;
                }
            }

            return 0;
        }

        private static T Parser<T>(string data)
        {
            return JsonConvert.DeserializeObject<T>(data);
        }
    }
}