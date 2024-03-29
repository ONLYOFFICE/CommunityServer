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


using System.Collections.Generic;
using System.Runtime.Serialization;

using ASC.Api.Employee;
using ASC.Projects.Core.Domain;
using ASC.Specific;
using ASC.Web.Projects.Classes;

namespace ASC.Api.Projects.Wrappers
{
    ///<inherited>ASC.Api.Projects.Wrappers.ObjectWrapperFullBase, ASC.Api.Projects</inherited>
    [DataContract(Name = "project", Namespace = "")]
    public class ProjectWrapperFull : ObjectWrapperFullBase
    {
        ///<example>false</example>
        [DataMember]
        public bool CanEdit { get; set; }

        ///<example>false</example>
        [DataMember]
        public bool CanDelete { get; set; }

        ///<type>ASC.Web.Projects.Classes.ProjectSecurityInfo, ASC.Web.Projects</type>
        [DataMember]
        public ProjectSecurityInfo Security { get; set; }

        ///<example type="int">13234</example>
        [DataMember(EmitDefaultValue = false)]
        public object ProjectFolder { get; set; }

        ///<example>false</example>
        ///<order>32</order>
        [DataMember(Order = 32)]
        public bool IsPrivate { get; set; }

        ///<example type="int">0</example>
        ///<order>33</order>
        [DataMember(Order = 33)]
        public int TaskCount { get; set; }

        ///<example type="int">0</example>
        ///<order>33</order>
        [DataMember(Order = 33)]
        public int TaskCountTotal { get; set; }

        ///<example type="int">0</example>
        ///<order>34</order>
        [DataMember(Order = 34)]
        public int MilestoneCount { get; set; }

        ///<example type="int">0</example>
        ///<order>34</order>
        [DataMember(Order = 34)]
        public int DiscussionCount { get; set; }

        ///<example type="int">0</example>
        ///<order>35</order>
        [DataMember(Order = 35)]
        public int ParticipantCount { get; set; }

        ///<example>TimeTrackingTotal</example>
        ///<order>35</order>
        [DataMember(Order = 35)]
        public string TimeTrackingTotal { get; set; }

        ///<example type="int">0</example>
        ///<order>35</order>
        [DataMember(Order = 35)]
        public int DocumentsCount { get; set; }

        ///<example type="int">0</example>
        ///<order>36</order>
        [DataMember(Order = 36)]
        public bool IsFollow { get; set; }

        ///<example>Tags1,Tags2</example>
        ///<order>37</order>
        ///<collection split=",">list</collection>
        [DataMember(Order = 37, EmitDefaultValue = false)]
        public IEnumerable<string> Tags { get; set; }


        private ProjectWrapperFull()
        {
        }

        public ProjectWrapperFull(ProjectApiBase projectApiBase, Project project, object filesRoot = null, bool isFollow = false, IEnumerable<string> tags = null)
        {
            Id = project.ID;
            Title = project.Title;
            Description = project.Description;
            Status = (int)project.Status;
            if (projectApiBase.Context.GetRequestValue("simple") != null)
            {
                ResponsibleId = project.Responsible;
                CreatedById = project.CreateBy;
                UpdatedById = project.LastModifiedBy;
            }
            else
            {
                Responsible = projectApiBase.GetEmployeeWraperFull(project.Responsible);
                CreatedBy = projectApiBase.GetEmployeeWraper(project.CreateBy);
                if (project.CreateBy != project.LastModifiedBy)
                {
                    UpdatedBy = projectApiBase.GetEmployeeWraper(project.LastModifiedBy);
                }
            }

            Created = (ApiDateTime)project.CreateOn;
            Updated = (ApiDateTime)project.LastModifiedOn;


            if (project.Security == null)
            {
                projectApiBase.ProjectSecurity.GetProjectSecurityInfo(project);
            }
            Security = project.Security;
            CanEdit = Security.CanEdit;
            CanDelete = Security.CanDelete;
            ProjectFolder = filesRoot;
            IsPrivate = project.Private;

            TaskCount = project.TaskCount;
            TaskCountTotal = project.TaskCountTotal;
            MilestoneCount = project.MilestoneCount;
            DiscussionCount = project.DiscussionCount;
            TimeTrackingTotal = project.TimeTrackingTotal ?? "";
            DocumentsCount = project.DocumentsCount;
            ParticipantCount = project.ParticipantCount;
            IsFollow = isFollow;
            Tags = tags;
        }

        public static ProjectWrapperFull GetSample()
        {
            return new ProjectWrapperFull
            {
                Id = 10,
                Title = "Sample Title",
                Description = "Sample description",
                Status = (int)MilestoneStatus.Open,
                Responsible = EmployeeWraper.GetSample(),
                Created = ApiDateTime.GetSample(),
                CreatedBy = EmployeeWraper.GetSample(),
                Updated = ApiDateTime.GetSample(),
                UpdatedBy = EmployeeWraper.GetSample(),
                ProjectFolder = 13234
            };
        }
    }
}