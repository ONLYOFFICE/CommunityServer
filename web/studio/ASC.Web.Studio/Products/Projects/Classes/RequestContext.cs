/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
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
