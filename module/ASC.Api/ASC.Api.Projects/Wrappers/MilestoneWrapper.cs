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
using ASC.Api.Employee;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Specific;

namespace ASC.Api.Projects.Wrappers
{
    [DataContract(Name = "milestone", Namespace = "")]
    public class MilestoneWrapper : ObjectWrapperFullBase
    {
        [DataMember(Order = 14)]
        public SimpleProjectWrapper ProjectOwner { get; set; }

        [DataMember(Order = 20)]
        public ApiDateTime Deadline { get; set; }

        [DataMember(Order = 20)]
        public bool IsKey { get; set; }

        [DataMember(Order = 20)]
        public bool IsNotify { get; set; }

        [DataMember]
        public bool CanEdit { get; set; }

        [DataMember]
        public bool CanDelete { get; set; }

        [DataMember(Order = 20)]
        public int ActiveTaskCount { get; set; }

        [DataMember(Order = 20)]
        public int ClosedTaskCount { get; set; }


        private MilestoneWrapper()
        {
        }

        public MilestoneWrapper(ProjectApiBase projectApiBase, Milestone milestone)
        {
            Id = milestone.ID;
            ProjectOwner = new SimpleProjectWrapper(milestone.Project);
            Title = milestone.Title;
            Description = milestone.Description;
            Created = (ApiDateTime)milestone.CreateOn;
            Updated = (ApiDateTime)milestone.LastModifiedOn;
            Status = (int)milestone.Status;
            Deadline = new ApiDateTime(milestone.DeadLine, TimeZoneInfo.Local);
            IsKey = milestone.IsKey;
            IsNotify = milestone.IsNotify;
            CanEdit = projectApiBase.ProjectSecurity.CanEdit(milestone);
            CanDelete = projectApiBase.ProjectSecurity.CanDelete(milestone);
            ActiveTaskCount = milestone.ActiveTaskCount;
            ClosedTaskCount = milestone.ClosedTaskCount;

            if (projectApiBase.Context.GetRequestValue("simple") != null)
            {
                CreatedById = milestone.CreateBy;
                UpdatedById = milestone.LastModifiedBy;
                if (!milestone.Responsible.Equals(Guid.Empty))
                {
                    ResponsibleId = milestone.Responsible;
                }
            }
            else
            {
                CreatedBy = projectApiBase.GetEmployeeWraper(milestone.CreateBy);
                if (milestone.CreateBy != milestone.LastModifiedBy)
                {
                    UpdatedBy = projectApiBase.GetEmployeeWraper(milestone.LastModifiedBy);
                }
                if (!milestone.Responsible.Equals(Guid.Empty))
                {
                    Responsible = projectApiBase.GetEmployeeWraper(milestone.Responsible);
                }
            }
        }

        public static MilestoneWrapper GetSample()
        {
            return new MilestoneWrapper
                {
                    Id = 10,
                    ProjectOwner = SimpleProjectWrapper.GetSample(),
                    Title = "Sample Title",
                    Description = "Sample description",
                    Created = ApiDateTime.GetSample(),
                    CreatedBy = EmployeeWraper.GetSample(),
                    Updated = ApiDateTime.GetSample(),
                    UpdatedBy = EmployeeWraper.GetSample(),
                    Responsible = EmployeeWraper.GetSample(),
                    Status = (int)MilestoneStatus.Open,
                    Deadline = ApiDateTime.GetSample(),
                    IsKey = false,
                    IsNotify = false,
                    CanEdit = true,
                    ActiveTaskCount = 15,
                    ClosedTaskCount = 5
                };
        }
    }
}