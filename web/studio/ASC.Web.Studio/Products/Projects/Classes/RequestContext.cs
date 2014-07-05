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
using System.Linq;
using System.Web;
using System.Collections;
using System.Collections.Generic;

using ASC.Core;
using ASC.Projects.Engine;
using ASC.Projects.Core.Domain;

namespace ASC.Web.Projects.Classes
{
    public class RequestContext
    {
        static Project Projectctx { get { return Hash["_projectctx"] as Project; } set { Hash["_projectctx"] = value; } }
        static int? ProjectId { get { return Hash["_projectId"] as int?; } set { Hash["_projectId"] = value; } }
        static int? ProjectsCount { get { return Hash["_projectsCount"] as int?; } set { Hash["_projectsCount"] = value; } }

        static List<Project> UserProjects { get { return Hash["_userProjects"] as List<Project>; } set { Hash["_userProjects"] = value; } }

        #region Project

        public static bool IsInConcreteProject
        {
            get { return !String.IsNullOrEmpty(UrlParameters.ProjectID); }
        }

        public static bool IsInConcreteProjectModule
        {
            get { return IsInConcreteProject && !String.IsNullOrEmpty(UrlParameters.EntityID); }
        }

        public static Project GetCurrentProject(bool isthrow = true)
        {
            if (Projectctx == null)
            {
                var project = Global.EngineFactory.GetProjectEngine().GetByID(GetCurrentProjectId(isthrow));

                if (project == null)
                {
                    if (isthrow) throw new ApplicationException("ProjectFat not finded");
                }
                else
                    Projectctx = project;
            }

            return Projectctx;
        }

        public static int GetCurrentProjectId(bool isthrow = true)
        {
            if (!ProjectId.HasValue)
            {
                int pid;
                if (!Int32.TryParse(UrlParameters.ProjectID, out pid))
                {
                    if (isthrow)
                        throw new ApplicationException("ProjectFat Id parameter invalid");
                }
                else
                    ProjectId = pid;
            }
            return ProjectId.HasValue ? ProjectId.Value : -1;
        }

        #endregion

        #region Projects

        public static int AllProjectsCount
        {
            get
            {
                if (!ProjectsCount.HasValue)
                    ProjectsCount = Global.EngineFactory.GetProjectEngine().GetAll().Count();
                return ProjectsCount.Value;
            }
        }

        public static List<Project> CurrentUserProjects
        {
            get
            {
                return UserProjects ??
                       (UserProjects =
                        Global.EngineFactory.GetProjectEngine().GetByParticipant(SecurityContext.CurrentAccount.ID));
            }
        }

        #endregion

        private static bool CanCreate(Func<Project, bool> canCreate, bool checkConreteProject)
        {
            if (checkConreteProject && IsInConcreteProject)
            {
                var project = GetCurrentProject();
                return project.Status != ProjectStatus.Closed && canCreate(project);
            }

            return ProjectSecurity.CurrentUserAdministrator
                       ? AllProjectsCount > 0
                       : CurrentUserProjects.Any(canCreate);
        }

        public static bool CanCreateTask(bool checkConreteProject = false)
        {
            return CanCreate(ProjectSecurity.CanCreateTask, checkConreteProject);
        }

        public static bool CanCreateMilestone(bool checkConreteProject = false)
        {
            return CanCreate(ProjectSecurity.CanCreateMilestone, checkConreteProject);   
        }

        public static bool CanCreateDiscussion(bool checkConreteProject = false)
        {
            return CanCreate(ProjectSecurity.CanCreateMessage, checkConreteProject);
        }

        public static bool CanCreateTime(bool checkConreteProject = false)
        {
            if (checkConreteProject && IsInConcreteProject)
            {
                var project = GetCurrentProject();
                var taskCount = Global.EngineFactory.GetProjectEngine().GetTaskCount(project.ID, null);
                return taskCount > 0 && ProjectSecurity.CanCreateTimeSpend(project);
            }

            return CanCreate(ProjectSecurity.CanCreateTimeSpend, false) && Global.EngineFactory.GetTaskEngine().GetByFilterCount(new TaskFilter()) > 0;
        }

        #region internal

        const string storageKey = "PROJECT_REQ_CTX";

        static Hashtable Hash
        {
            get
            {
                if (HttpContext.Current == null) throw new ApplicationException("Not in http request");

                var hash = (Hashtable)HttpContext.Current.Items[storageKey];
                if (hash == null)
                {
                    hash = new Hashtable();
                    HttpContext.Current.Items[storageKey] = hash;
                }
                return hash;
            }
        }

        #endregion
    }
}
