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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using ASC.Projects.Core.Domain;
using ASC.Specific;

namespace ASC.Api.Projects.Wrappers
{
    [DataContract(Name = "task")]
    public class SimpleTaskWrapper
    {
        [DataMember(Name = "id")]
        public int ID { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "deadline")]
        public ApiDateTime Deadline { get; set; }

        [DataMember(Order = 20)]
        public int Status { get; set; }

        [DataMember(Order = 55, EmitDefaultValue = false)]
        public int? CustomTaskStatus { get; set; }

        [DataMember(Order = 50)]
        public ApiDateTime Created { get; set; }

        [DataMember(Order = 51, EmitDefaultValue = false)]
        public Guid CreatedBy { get; set; }

        private ApiDateTime updated;

        [DataMember(Order = 52, EmitDefaultValue = false)]
        public ApiDateTime Updated
        {
            get { return updated < Created ? Created : updated; }
            set { updated = value; }
        }

        [DataMember(Order = 41, EmitDefaultValue = false)]
        public Guid UpdatedBy { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public ApiDateTime StartDate { get; set; }

        [DataMember(Order = 13, EmitDefaultValue = false)]
        public int MilestoneId { get; set; }

        [DataMember(Order = 12)]
        public TaskPriority Priority { get; set; }

        [DataMember(Order = 14)]
        public int ProjectOwner { get; set; }

        [DataMember(Order = 15, EmitDefaultValue = false)]
        public int Progress { get; set; }

        [DataMember(Order = 20, EmitDefaultValue = false)]
        public int SubtasksCount { get; set; }

        [DataMember(Order = 21, EmitDefaultValue = false)]
        public IEnumerable<TaskLinkWrapper> Links { get; set; }

        [DataMember(Order = 53)]
        public List<Guid> Responsibles { get; set; }

        [DataMember]
        public bool CanEdit { get; set; }

        [DataMember]
        public bool CanCreateSubtask { get; set; }

        [DataMember]
        public bool CanCreateTimeSpend { get; set; }

        [DataMember]
        public bool CanDelete { get; set; }

        public SimpleTaskWrapper(ProjectApiBase projectApiBase, Task task)
        {
            ID = task.ID;
            Title = task.Title;
            Description = task.Description;
            Status = (int)task.Status;
            CustomTaskStatus = task.CustomTaskStatus;

            if (task.Responsibles != null)
            {
                Responsibles = task.Responsibles;
            }

            Deadline = (task.Deadline == DateTime.MinValue ? null : new ApiDateTime(task.Deadline, TimeZoneInfo.Local));
            Priority = task.Priority;
            ProjectOwner = task.Project.ID;
            MilestoneId = task.Milestone;
            Created = (ApiDateTime)task.CreateOn;
            CreatedBy = task.CreateBy;
            Updated = (ApiDateTime)task.LastModifiedOn;
            StartDate = task.StartDate.Equals(DateTime.MinValue) ? null : (ApiDateTime)task.StartDate;

            if (task.CreateBy != task.LastModifiedBy)
            {
                UpdatedBy = task.LastModifiedBy;
            }

            if (task.SubTasks != null)
            {
                SubtasksCount = task.SubTasks.Count(r => r.Status == TaskStatus.Open); // somehow don't work :(
            }

            Progress = task.Progress;
            MilestoneId = task.Milestone;

            if (task.Links.Any())
            {
                Links = task.Links.Select(r => new TaskLinkWrapper(r));
            }

            CanEdit = projectApiBase.ProjectSecurity.CanEdit(task);
            CanCreateSubtask = projectApiBase.ProjectSecurity.CanCreateSubtask(task);
            CanCreateTimeSpend = projectApiBase.ProjectSecurity.CanCreateTimeSpend(task);
            CanDelete = projectApiBase.ProjectSecurity.CanDelete(task);
        }
    }
}