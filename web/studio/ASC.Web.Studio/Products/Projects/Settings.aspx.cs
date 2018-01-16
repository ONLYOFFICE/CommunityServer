/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
using System.Runtime.Serialization;
using ASC.Core;
using ASC.Core.Common.Settings;
using ASC.Projects.Engine;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Projects
{
    public partial class Settings : BasePage
    {
        protected override bool CheckSecurity { get { return ProjectSecurity.IsProjectsEnabled(); } }

        protected override void PageLoad()
        {
            Title = HeaderStringHelper.GetPageTitle(ProjectsCommonResource.CommonSettings);
        }
    }

    [Serializable]
    [DataContract]
    public class ProjectsCommonSettings : BaseSettings<ProjectsCommonSettings>
    {
        [DataMember(Name = "EverebodyCanCreate")]
        public bool EverebodyCanCreate { get; set; }

        [DataMember(Name = "HideEntitiesInPausedProjects")]
        public bool HideEntitiesInPausedProjects { get; set; }

        [DataMember]
        public StartModuleType StartModuleType { get; set; }

        public override Guid ID
        {
            get { return new Guid("{F833803D-0A84-4156-A73F-7680F522FE07}"); }
        }

        public override ISettings GetDefault()
        {
            return new ProjectsCommonSettings
            {
                EverebodyCanCreate = false,
                StartModuleType =  StartModuleType.Tasks,
                HideEntitiesInPausedProjects = true
            };
        }
    }

    public enum StartModuleType
    {
        Projects,
        Tasks,
        Discussions,
        TimeTracking
    }

    public class StartModule
    {
        public StartModuleType StartModuleType { get; private set; }
        public string Title { get; private set; }
        public string Page { get; private set; }

        public static StartModule ProjectsModule = new StartModule(StartModuleType.Projects, ProjectResource.Projects, "projects.aspx");
        public static StartModule TaskModule = new StartModule(StartModuleType.Tasks, TaskResource.Tasks, "tasks.aspx");
        public static StartModule DiscussionModule = new StartModule(StartModuleType.Discussions, MessageResource.Messages, "messages.aspx");
        public static StartModule TimeTrackingModule = new StartModule(StartModuleType.TimeTracking, ProjectsCommonResource.TimeTracking, "timetracking.aspx");
        

        private StartModule(StartModuleType startModuleType, string title, string page)
        {
            StartModuleType = startModuleType;
            Title = title;
            Page = page;
        }

        public static StartModule GetInstance(StartModuleType startModuleType)
        {
            switch (startModuleType)
            {
                case StartModuleType.Projects: return ProjectsModule;
                case StartModuleType.Tasks: return TaskModule;
                case StartModuleType.Discussions: return DiscussionModule;
                case StartModuleType.TimeTracking: return TimeTrackingModule;
            }
            return null;
        }
    }
}