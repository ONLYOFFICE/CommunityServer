/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using ASC.Api.Employee;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Specific;

namespace ASC.Api.Projects.Wrappers
{
    [DataContract(Name = "task", Namespace = "")]
    public class TaskWrapper : ObjectWrapperFullBase
    {
        [DataMember]
        public bool CanEdit { get; set; }

        [DataMember]
        public bool CanCreateSubtask { get; set; }

        [DataMember]
        public bool CanCreateTimeSpend { get; set; }

        [DataMember]
        public bool CanDelete { get; set; }

        [DataMember(Order = 12, EmitDefaultValue = false)]
        public ApiDateTime Deadline { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public ApiDateTime StartDate { get; set; }

        [DataMember(Order = 13, EmitDefaultValue = false)]
        public int MilestoneId { get; set; }

        [DataMember(Order = 12)]
        public TaskPriority Priority { get; set; }

        [DataMember(Order = 14)]
        public SimpleProjectWrapper ProjectOwner { get; set; }

        [DataMember(Order = 15, EmitDefaultValue = false)]
        public int Progress { get; set; }

        [DataMember(Order = 20, EmitDefaultValue = false)]
        public List<SubtaskWrapper> Subtasks { get; set; }

        [DataMember(Order = 21, EmitDefaultValue = false)]
        public IEnumerable<TaskLinkWrapper> Links { get; set; }

        [DataMember(Order = 53)]
        public List<EmployeeWraper> Responsibles { get; set; }

        [DataMember(Order = 54, EmitDefaultValue = false)]
        public SimpleMilestoneWrapper Milestone { get; set; }


        private TaskWrapper()
        {
        }

        public TaskWrapper(Task task)
        {
            Id = task.ID;
            Title = task.Title;
            Description = task.Description;
            Status = (int)task.Status;

            if (task.Responsibles != null)
            {
                Responsibles = task.Responsibles.Select(EmployeeWraper.Get).OrderBy(r => r.DisplayName).ToList();
            }


            Deadline = (task.Deadline == DateTime.MinValue ? null : new ApiDateTime(task.Deadline, TimeZoneInfo.Local));
            Priority = task.Priority;
            ProjectOwner = new SimpleProjectWrapper(task.Project);
            MilestoneId = task.Milestone;
            Created = (ApiDateTime)task.CreateOn;
            CreatedBy = EmployeeWraper.Get(task.CreateBy);
            Updated = (ApiDateTime)task.LastModifiedOn;
            StartDate = task.StartDate.Equals(DateTime.MinValue) ? null : (ApiDateTime)task.StartDate;

            if (task.CreateBy != task.LastModifiedBy)
            {
                UpdatedBy = EmployeeWraper.Get(task.LastModifiedBy);
            }

            if (task.SubTasks != null)
            {
                Subtasks = task.SubTasks.Select(x => new SubtaskWrapper(x, task)).ToList();
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

            CanEdit = ProjectSecurity.CanEdit(task);
            CanCreateSubtask = ProjectSecurity.CanCreateSubtask(task);
            CanCreateTimeSpend = ProjectSecurity.CanCreateTimeSpend(task);
            CanDelete = ProjectSecurity.CanDelete(task);
        }

        public TaskWrapper(Task task, Milestone milestone) : this(task)
        {
            if (task.Milestone != 0)
                Milestone = new SimpleMilestoneWrapper(milestone);
        }


        public static TaskWrapper GetSample()
        {
            return new TaskWrapper
                {
                    Id = 10,
                    Title = "Sample Title",
                    Description = "Sample description",
                    Deadline = (ApiDateTime)DateTime.Now,
                    Priority = TaskPriority.High,
                    Status = (int)MilestoneStatus.Open,
                    Responsible = EmployeeWraper.GetSample(),
                    ProjectOwner = SimpleProjectWrapper.GetSample(),
                    MilestoneId = 123,
                    Created = (ApiDateTime)DateTime.Now,
                    CreatedBy = EmployeeWraper.GetSample(),
                    Updated = (ApiDateTime)DateTime.Now,
                    UpdatedBy = EmployeeWraper.GetSample(),
                    StartDate = (ApiDateTime)DateTime.Now
                };
        }
    }
}