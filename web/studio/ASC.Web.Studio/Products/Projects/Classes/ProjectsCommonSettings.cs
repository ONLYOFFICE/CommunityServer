using System;
using System.Runtime.Serialization;
using ASC.Core.Common.Settings;
using ASC.Web.Projects.Resources;

namespace ASC.Web.Projects
{
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
                StartModuleType = StartModuleType.Tasks,
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