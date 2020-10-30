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


using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Web.Projects.Core;
using Autofac;

namespace ASC.Web.Projects.Classes
{
    [DataContract(Name = "project_security", Namespace = "")]
    public class ProjectSecurityInfo
    {
        [DataMember]
        public bool CanCreateMilestone { get; set; }

        [DataMember]
        public bool CanCreateMessage { get; set; }

        [DataMember]
        public bool CanCreateTask { get; set; }

        [DataMember]
        public bool CanCreateTimeSpend { get; set; }

        [DataMember]
        public bool CanEditTeam { get; set; }

        [DataMember]
        public bool CanReadFiles { get; set; }

        [DataMember]
        public bool CanReadMilestones { get; set; }

        [DataMember]
        public bool CanReadMessages { get; set; }

        [DataMember]
        public bool CanReadTasks { get; set; }

        [DataMember]
        public bool CanLinkContact { get; set; }

        [DataMember]
        public bool CanReadContacts { get; set; }

        [DataMember]
        public bool CanEdit { get; set; }

        [DataMember]
        public bool CanDelete { get; set; }

        [DataMember]
        public bool IsInTeam { get; set; }

        public static ProjectSecurityInfo GetSample()
        {
            return new ProjectSecurityInfo();
        }
    }

    [DataContract(Name = "common_security", Namespace = "")]
    public class CommonSecurityInfo
    {
        [DataMember]
        public bool CanCreateProject { get; set; }

        [DataMember]
        public bool CanCreateTask { get; set; }

        [DataMember]
        public bool CanCreateMilestone { get; set; }

        [DataMember]
        public bool CanCreateMessage { get; set; }

        [DataMember]
        public bool CanCreateTimeSpend { get; set; }

        public CommonSecurityInfo()
        {
            var filter = new TaskFilter
            {
                SortBy = "title",
                SortOrder = true,
                ProjectStatuses = new List<ProjectStatus> {ProjectStatus.Open}
            };

            using (var scope = DIHelper.Resolve())
            {
                var projectSecurity = scope.Resolve<ProjectSecurity>();
                var engineFactory = scope.Resolve<EngineFactory>();
                var projects = engineFactory.ProjectEngine.GetByFilter(filter).ToList();

                CanCreateProject = projectSecurity.CanCreate<Project>(null);
                CanCreateTask = projects.Any(projectSecurity.CanCreate<Task>);
                CanCreateMilestone = projects.Any(projectSecurity.CanCreate<Milestone>);
                CanCreateMessage = projects.Any(projectSecurity.CanCreate<Message>);
                CanCreateTimeSpend = projects.Any(projectSecurity.CanCreate<TimeSpend>);
            }
        }
    }

    public class TaskSecurityInfo
    {
        public bool CanEdit{ get; set; }

        public bool CanCreateSubtask { get; set; }

        public bool CanCreateTimeSpend { get; set; }

        public bool CanDelete { get; set; }

        public bool CanReadFiles{ get; set; }
    }
}