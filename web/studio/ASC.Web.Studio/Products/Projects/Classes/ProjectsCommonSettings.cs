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

        public static StartModule ProjectsModule = new StartModule(StartModuleType.Projects, () => ProjectResource.Projects, "Projects.aspx");
        public static StartModule TaskModule = new StartModule(StartModuleType.Tasks, () => TaskResource.Tasks, "Tasks.aspx");
        public static StartModule DiscussionModule = new StartModule(StartModuleType.Discussions, () => MessageResource.Messages, "Messages.aspx");
        public static StartModule TimeTrackingModule = new StartModule(StartModuleType.TimeTracking, () => ProjectsCommonResource.TimeTracking, "TimeTracking.aspx");


        private StartModule(StartModuleType startModuleType, Func<string> title, string page)
        {
            StartModuleType = startModuleType;
            Title = title;
            Page = page;
        }
    }

    
}