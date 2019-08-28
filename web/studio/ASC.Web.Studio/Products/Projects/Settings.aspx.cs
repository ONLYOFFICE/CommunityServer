/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using System.Web;
using ASC.Core.Common.Settings;
using ASC.Web.Projects.Classes;
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
            Page
                .RegisterStyle(
                PathProvider.GetFileStaticRelativePath("settings.less"),
                "~/Products/Files/Controls/FileSelector/fileselector.css",
                "~/Products/Files/Controls/ThirdParty/thirdparty.css",
                "~/Products/Files/Controls/ContentList/contentlist.css",
                "~/Products/Files/Controls/EmptyFolder/emptyfolder.css",
                "~/Products/Files/Controls/Tree/tree.css")
                .RegisterBodyScripts(
                PathProvider.GetFileStaticRelativePath("settings.js"),
                    "~/Products/Files/Controls/Tree/tree.js",
                    "~/Products/Files/Controls/EmptyFolder/emptyfolder.js",
                    "~/Products/Files/Controls/FileSelector/fileselector.js",
                    "~/Products/Files/js/common.js",
                    "~/Products/Files/js/templatemanager.js",
                    "~/Products/Files/js/servicemanager.js",
                    "~/Products/Files/js/ui.js",
                    "~/Products/Files/js/eventhandler.js");

            FolderSelectorHolder.Controls.Add(LoadControl(CommonLinkUtility.ToAbsolute("~/Products/Files/Controls/FileSelector/FileSelector.ascx")));
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

        [DataMember(Name = "FolderId")]
        private object folderId;

        public object FolderId
        {
            get
            {
                return folderId ?? Files.Classes.Global.FolderMy;
            }
            set
            {
                folderId = value ?? Files.Classes.Global.FolderMy;
            }
        }
        
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
        public Func<string> Title { get; private set; }
        public string Page { get; private set; }

        public static StartModule ProjectsModule = new StartModule(StartModuleType.Projects, () => ProjectResource.Projects, "projects.aspx");
        public static StartModule TaskModule = new StartModule(StartModuleType.Tasks, () => TaskResource.Tasks, "tasks.aspx");
        public static StartModule DiscussionModule = new StartModule(StartModuleType.Discussions, () => MessageResource.Messages, "messages.aspx");
        public static StartModule TimeTrackingModule = new StartModule(StartModuleType.TimeTracking, () => ProjectsCommonResource.TimeTracking, "timetracking.aspx");
        

        private StartModule(StartModuleType startModuleType, Func<string> title, string page)
        {
            StartModuleType = startModuleType;
            Title = title;
            Page = page;
        }
    }
}