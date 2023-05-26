/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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


using System.Runtime.Serialization;

using ASC.Api.Employee;
using ASC.Projects.Core.Domain;

namespace ASC.Api.Projects.Wrappers
{
    ///<inherited>ASC.Api.Projects.Wrappers.ObjectWrapperBase, ASC.Api.Projects</inherited>
    [DataContract(Name = "project", Namespace = "")]
    public class ProjectWrapper : ObjectWrapperBase
    {
        ///<example>false</example>
        ///<order>31</order>
        [DataMember(Order = 31)]
        public bool CanEdit { get; set; }

        ///<example>false</example>
        ///<order>32</order>
        [DataMember(Order = 32)]
        public bool IsPrivate { get; set; }

        public ProjectWrapper(ProjectApiBase projectApiBase, Project project)
        {
            Id = project.ID;
            Title = project.Title;
            Description = project.Description;
            Responsible = projectApiBase.GetEmployeeWraper(project.Responsible);
            Status = (int)project.Status;
            CanEdit = projectApiBase.ProjectSecurity.CanEdit(project);
            IsPrivate = project.Private;
        }

        private ProjectWrapper()
        {
        }


        public static ProjectWrapper GetSample()
        {
            return new ProjectWrapper
            {
                Id = 10,
                Title = "Sample Title",
                Description = "Sample description",
                Responsible = EmployeeWraper.GetSample(),
                Status = (int)ProjectStatus.Open,
            };
        }
    }
}