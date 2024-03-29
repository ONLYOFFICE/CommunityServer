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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using ASC.Api.Employee;
using ASC.Projects.Core.Domain;
using ASC.Specific;

namespace ASC.Api.Projects.Wrappers
{
    ///<inherited>ASC.Api.Projects.Wrappers.ObjectWrapperFullBase, ASC.Api.Projects</inherited>
    [DataContract(Name = "task", Namespace = "")]
    public class TaskWrapper : ObjectWrapperFullBase
    {
        ///<example>false</example>
        [DataMember]
        public bool CanEdit { get; set; }

        ///<example>false</example>
        [DataMember]
        public bool CanCreateSubtask { get; set; }

        ///<example>false</example>
        [DataMember]
        public bool CanCreateTimeSpend { get; set; }

        ///<example>false</example>
        [DataMember]
        public bool CanDelete { get; set; }

        ///<example>false</example>
        [DataMember]
        public bool CanReadFiles { get; set; }

        ///<example>2020-12-22T04:11:56.5768573Z</example>
        ///<order>12</order>
        [DataMember(Order = 12, EmitDefaultValue = false)]
        public ApiDateTime Deadline { get; set; }

        ///<example>2020-12-22T04:11:56.5768573Z</example>
        [DataMember(EmitDefaultValue = false)]
        public ApiDateTime StartDate { get; set; }

        /// <example type="int">123</example>
        /// <order>13</order>
        [DataMember(Order = 13, EmitDefaultValue = false)]
        public int MilestoneId { get; set; }

        /// <example type="int">1</example>
        /// <order>12</order>
        [DataMember(Order = 12)]
        public TaskPriority Priority { get; set; }

        ///<type>ASC.Api.Projects.Wrappers.SimpleProjectWrapper, ASC.Api.Projects</type>
        ///<order>14</order>
        [DataMember(Order = 14)]
        public SimpleProjectWrapper ProjectOwner { get; set; }

        /// <example type="int">55</example>
        /// <order>15</order>
        [DataMember(Order = 15, EmitDefaultValue = false)]
        public int Progress { get; set; }

        ///<type>ASC.Api.Projects.Wrappers.SubtaskWrapper, ASC.Api.Projects</type>
        ///<order>20</order>
        ///<collection>list</collection>
        [DataMember(Order = 20, EmitDefaultValue = false)]
        public List<SubtaskWrapper> Subtasks { get; set; }

        ///<type>ASC.Api.Projects.Wrappers.TaskLinkWrapper, ASC.Api.Projects</type>
        ///<order>21</order>
        ///<collection>list</collection>
        [DataMember(Order = 21, EmitDefaultValue = false)]
        public IEnumerable<TaskLinkWrapper> Links { get; set; }

        ///<type>ASC.Api.Employee.EmployeeWraper, ASC.Api.Employee</type>
        ///<order>53</order>
        ///<collection>list</collection>
        [DataMember(Order = 53)]
        public List<EmployeeWraper> Responsibles { get; set; }

        ///<example>00000000-0000-0000-0000-000000000000</example>
        ///<order>53</order>
        ///<collection>list</collection>
        [DataMember(Order = 53, EmitDefaultValue = false)]
        public List<Guid> ResponsibleIds { get; set; }

        ///<type>ASC.Api.Projects.Wrappers.SimpleMilestoneWrapper, ASC.Api.Projects</type>
        ///<order>54</order>
        [DataMember(Order = 54, EmitDefaultValue = false)]
        public SimpleMilestoneWrapper Milestone { get; set; }

        ///<example type="int">1</example>
        ///<order>55</order>
        [DataMember(Order = 55, EmitDefaultValue = false)]
        public int? CustomTaskStatus { get; set; }

        private TaskWrapper()
        {
        }

        public TaskWrapper(ProjectApiBase projectApiBase, Task task)
        {
            Id = task.ID;
            Title = task.Title;
            Description = task.Description;
            Status = (int)task.Status;

            if (Status > 2)
            {
                Status = 1;
            }

            CustomTaskStatus = task.CustomTaskStatus;

            Deadline = (task.Deadline == DateTime.MinValue ? null : new ApiDateTime(task.Deadline, TimeZoneInfo.Local));
            Priority = task.Priority;
            ProjectOwner = new SimpleProjectWrapper(task.Project);
            MilestoneId = task.Milestone;
            Created = (ApiDateTime)task.CreateOn;
            Updated = (ApiDateTime)task.LastModifiedOn;
            StartDate = task.StartDate.Equals(DateTime.MinValue) ? null : (ApiDateTime)task.StartDate;

            if (task.SubTasks != null)
            {
                Subtasks = task.SubTasks.Select(x => new SubtaskWrapper(projectApiBase, x, task)).ToList();
            }

            Progress = task.Progress;

            if (task.Milestone != 0 && task.MilestoneDesc != null)
            {
                Milestone = new SimpleMilestoneWrapper(task.MilestoneDesc);
            }

            if (task.Links.Any())
            {
                Links = task.Links.Select(r => new TaskLinkWrapper(r));
            }

            if (task.Security == null)
            {
                projectApiBase.ProjectSecurity.GetTaskSecurityInfo(task);
            }

            if (projectApiBase.Context.GetRequestValue("simple") != null)
            {
                CreatedById = task.CreateBy;
                UpdatedById = task.LastModifiedBy;
                if (task.Responsibles != null)
                {
                    ResponsibleIds = task.Responsibles;
                }
            }
            else
            {
                CreatedBy = projectApiBase.GetEmployeeWraper(task.CreateBy);
                if (task.CreateBy != task.LastModifiedBy)
                {
                    UpdatedBy = projectApiBase.GetEmployeeWraper(task.LastModifiedBy);
                }
                if (task.Responsibles != null)
                {
                    Responsibles = task.Responsibles.Select(projectApiBase.GetEmployeeWraper).OrderBy(r => r.DisplayName).ToList();
                }
            }

            CanEdit = task.Security.CanEdit;
            CanCreateSubtask = task.Security.CanCreateSubtask;
            CanCreateTimeSpend = task.Security.CanCreateTimeSpend;
            CanDelete = task.Security.CanDelete;
            CanReadFiles = task.Security.CanReadFiles;
        }

        public TaskWrapper(ProjectApiBase projectApiBase, Task task, Milestone milestone)
            : this(projectApiBase, task)
        {
            if (milestone != null && task.Milestone != 0)
                Milestone = new SimpleMilestoneWrapper(milestone);
        }


        public static TaskWrapper GetSample()
        {
            return new TaskWrapper
            {
                Id = 10,
                Title = "Sample Title",
                Description = "Sample description",
                Deadline = ApiDateTime.GetSample(),
                Priority = TaskPriority.High,
                Status = (int)MilestoneStatus.Open,
                Responsible = EmployeeWraper.GetSample(),
                ProjectOwner = SimpleProjectWrapper.GetSample(),
                MilestoneId = 123,
                Created = ApiDateTime.GetSample(),
                CreatedBy = EmployeeWraper.GetSample(),
                Updated = ApiDateTime.GetSample(),
                UpdatedBy = EmployeeWraper.GetSample(),
                StartDate = ApiDateTime.GetSample()
            };
        }
    }
}